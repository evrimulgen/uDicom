using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using uDicom.Common;
using uDicom.Common.Utilities;
using uDicom.WorkItemService;
using uDicom.WorkItemService.Interface;
using UIH.Dicom;
using UIH.Dicom.Network;

namespace uDicom.WorkItem.Archive.Import
{
    /// <summary>
	/// Processor for import of files.
	/// </summary>
    internal class ImportItemProcessor : BaseItemProcessor<ImportFilesRequest, ImportFilesProgress>
    {
        private readonly object _syncLock = new object();
        private IWorkItemActivityMonitorService _workItemActivityMonitorService;

        private IWorkItemActivityMonitorService WorkItemActivityMonitorService
        {
            get
            {
                if (_workItemActivityMonitorService != null) return _workItemActivityMonitorService;
                lock (_syncLock)
                {
                    if (_workItemActivityMonitorService != null) return _workItemActivityMonitorService;
                    return _workItemActivityMonitorService = Platform.GetService<IWorkItemActivityMonitorService>();
                }
            }
        }

        #region Public Methods

        public override bool Initialize(WorkItemStatusProxy proxy)
        {
            bool initResult = base.Initialize(proxy);

            return initResult;
        }

        public override void Dispose()
        {
            var disposableService = _workItemActivityMonitorService as IDisposable;
            if (disposableService != null) disposableService.Dispose();

            base.Dispose();
        }

        public override void Process()
        {
            Progress.TotalFilesToImport = 0;
            Progress.NumberOfFilesImported = 0;
            Progress.NumberOfImportFailures = 0;
            Progress.PathsImported = 0;
            Progress.PathsToImport = 0;
            Progress.CompletedEnumeration = null;

            Progress.StatusDetails = Request.FilePaths.Count > 1
                                         ? String.Format("+ { 0}", Request.FilePaths[0])
                                         : Request.FilePaths[0];
            Proxy.UpdateProgress();

            if (CancelPending)
            {
                Proxy.Cancel();
                return;
            }
            if (StopPending)
            {
                Proxy.Postpone();
                return;
            }

            bool fatalError = false;

            //it's ok to read this property unsynchronized because this is the only thread that is adding to the queue for the particular job.
            if (Request.FilePaths.Count == 0)
            {
                Progress.StatusDetails = "Complete.  No files could be found to import.";
                Progress.IsCancelable = false;
            }
            else
            {
                using (UserIdentityCache.Get(Proxy.Item.Oid).Impersonate())
                {
                    string failureReason;
                    if (!ValidateRequest(Request, out failureReason))
                    {
                        Proxy.Fail(failureReason, WorkItemFailureType.Fatal);
                        return;
                    }

                    fatalError = ImportFiles(Request.FilePaths, Request.FileExtensions, Request.Recursive);
                }

                GC.Collect();
            }

            if (CancelPending)
                Proxy.Cancel();
            else if (StopPending)
                Proxy.Postpone();
            else if (fatalError || Progress.NumberOfImportFailures > 0)
                Proxy.Fail(string.Format(SR.ImportFailedPartialStudies, Progress.StatusDetails), WorkItemFailureType.Fatal);
            else
                Proxy.Complete();
        }

        #endregion

        #region Private Methods

        private bool ValidateRequest(ImportFilesRequest filesRequest, out string reason)
        {
            reason = string.Empty;

            if (filesRequest == null)
            {
                reason = "No files have been specified to import.";
                return false;
            }

            if (filesRequest.FilePaths == null)
            {
                reason = "No files have been specified to import.";
                return false;
            }

            if (!filesRequest.FilePaths.Any(path => Directory.Exists(path) || File.Exists(path)))
            {
                reason = "Files specified for import are no longer accessible.";
                return false;
            }

            return true;
        }

        private void ImportFile(string file, ImportStudyContext context)
        {
            // Note, we're not doing impersonation of the user's identity, so we may have failures here
            // which would be new in Marmot.
            try
            {
                EnsureMaxUsedSpaceNotExceeded();

                var dicomFile = new DicomFile(file);

                DicomReadOptions readOptions = Request.FileImportBehaviour == FileImportBehaviourEnum.Save
                                                   ? DicomReadOptions.Default
                                                   : DicomReadOptions.Default | DicomReadOptions.StorePixelDataReferences;

                dicomFile.Load(readOptions);

                var importer = new ImportFilesUtility(context);

                DicomProcessingResult result = importer.Import(dicomFile, Request.BadFileBehaviour, Request.FileImportBehaviour);

                if (result.DicomStatus == DicomStatuses.Success)
                {
                    Progress.NumberOfFilesImported++;
                }
                else
                {
                    Progress.NumberOfImportFailures++;
                    Progress.StatusDetails = result.ErrorMessage;
                }
            }
            catch (NotEnoughStorageException)
            {
                Progress.NumberOfImportFailures++;
                Progress.StatusDetails = SR.ExceptionNotEnoughStorage;
                context.FatalError = true;
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Warn, "Unable to import DICOM File ({0}): {1}", file, e.Message);
                Progress.NumberOfImportFailures++;
                Progress.StatusDetails = string.Format("{0}: {1}", file, e.Message);
            }
        }

        private bool ImportFiles(IList<string> filePaths,
                                 IEnumerable<string> fileExtensions,
                                 bool recursive)
        {
            var configuration = GetServerConfiguration();

            var context = new ImportStudyContext(configuration.AETitle, StudyStore.GetConfiguration(), string.IsNullOrEmpty(Request.UserName) ? EventSource.CurrentProcess : EventSource.GetUserEventSource(Request.UserName));

            // Publish the creation of the StudyImport WorkItems
            lock (context.StudyWorkItemsSyncLock)
            {
                context.StudyWorkItems.ItemAdded += (sender, args) => WorkItemActivityMonitorService.Publish(new WorkItemPublishRequest { Item = WorkItemDataHelper.FromWorkItem(args.Item) });
                context.StudyWorkItems.ItemChanged += (sender, args) => WorkItemActivityMonitorService.Publish(new WorkItemPublishRequest { Item = WorkItemDataHelper.FromWorkItem(args.Item) });
            }

            var extensions = new List<string>();

            if (fileExtensions != null)
                extensions.AddRange(fileExtensions.Where(extension => !String.IsNullOrEmpty(extension)));

            Progress.PathsToImport = filePaths.Count;

            bool completedEnumeration = true;
            foreach (string path in filePaths)
            {
                FileProcessor.Process(path, string.Empty,
                                      delegate (string file, out bool cancel)
                                      {
                                          cancel = false;

                                          if (CancelPending || StopPending || context.FatalError)
                                          {
                                              cancel = true;
                                              return;
                                          }

                                          if (extensions.Count == 0 || extensions.Any(file.EndsWith))
                                          {
                                              ++Progress.TotalFilesToImport;

                                              Proxy.UpdateProgress();

                                              ImportFile(file, context);
                                          }
                                      }, recursive);

                Progress.PathsImported++;
                Proxy.UpdateProgress();

                if (CancelPending || StopPending || context.FatalError)
                    completedEnumeration = false;
            }

            // pulses any idle study process work items related to the files we imported
            new ImportFilesUtility(context).PulseStudyWorkItems();

            Progress.CompletedEnumeration = completedEnumeration;

            return context.FatalError;
        }

        #endregion
    }
}
