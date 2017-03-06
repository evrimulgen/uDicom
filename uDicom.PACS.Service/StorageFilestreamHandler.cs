using System;
using System.IO;
using uDicom.Common;
using uDicom.Common.Utilities;
using UIH.Dicom.Network;
using UIH.Dicom.PACS.Service.Interface;
using UIH.Pacs.Services.Dicom;

namespace UIH.Dicom.PACS.Service
{
    class StorageFilestreamHandler : IDicomFilestreamHandler, IDisposable
    {
        private readonly DicomScpContext _context;
        //private readonly Device _remoteDevice;
        private readonly SopInstanceImporterContext _importContext;
        private FileStream _fileStream;
        private string _sourceFolder;
        private string _sourceFilename;
        private string _filesystemStreamingFolder;

        public StorageFilestreamHandler(DicomScpContext context, ServerAssociationParameters assoc)
        {
            _context = context;
            //_remoteDevice = remoteDevice;
            _importContext = new SopInstanceImporterContext(
                String.Format("{0}_{1}", assoc.CallingAE, assoc.TimeStamp.ToString("yyyyMMddhhmmss")),
                assoc.CallingAE, assoc.CalledAE);

            //if (_remoteDevice != null && _remoteDevice.DeviceTypeEnum.Equals(DeviceTypeEnum.PrimaryPacs))
            //{
            //    _importContext.DuplicateProcessing = DuplicateProcessingEnum.OverwriteSopAndUpdateDatabase;
            //}
        }

        public bool SaveStreamData(DicomMessage message, byte[] data, int offset, int count)
        {
            ISopInstanceImporter importer = IoC.Get<ISopInstanceImporter>();
            importer.Context = _importContext;

            var sopInstanceUid = message.DataSet[DicomTags.SopInstanceUid].GetString(0, string.Empty);

            if (_fileStream == null)
            {
                if (!importer.GetStreamedFileStorageFolder(message, out _sourceFolder, out _filesystemStreamingFolder))
                {
                    Platform.Log(LogLevel.Warn, "Unable to create a folder to save SOP Instance, rejecting: {0}", sopInstanceUid);
                    return false;
                }

                _sourceFilename = Path.Combine(_sourceFolder, Guid.NewGuid().ToString() + "dcm");

                try
                {
                    _fileStream = FileStreamOpener.OpenForSoleUpdate(_sourceFilename, FileMode.Create);
                }
                catch (Exception x)
                {
                    Platform.Log(LogLevel.Warn, x, "Unable to open file for saving filestream: {0}", _sourceFilename);
                    return false;
                }
            }

            _fileStream.Write(data, offset, count);

            return true;
        }

        public void CancelStream()
        {
            if (_fileStream != null)
            {
                try
                {
                    _fileStream.Close();
                    _fileStream = null;
                }
                catch (Exception)
                { }
            }

            if (!string.IsNullOrEmpty(_sourceFilename))
            {
                try
                {
                    FileUtils.Delete(_sourceFilename);
                    _sourceFilename = null;
                }
                catch (Exception)
                {
                }
            }

            CleanupDirectory();
        }

        public bool CompleteStream(DicomServer server, ServerAssociationParameters assoc, byte presentationId, DicomMessage message)
        {

            DicomProcessingResult result;
            try
            {
                if (_fileStream != null)
                {
                    _fileStream.Flush(true);
                    _fileStream.Close();
                    _fileStream.Dispose();
                    _fileStream = null;
                }

                ISopInstanceImporter importer = IoC.Get<ISopInstanceImporter>();
                importer.Context = _importContext;

                result = importer.ImportFile(message, _sourceFilename);

                if (result.Successful)
                {
                    if (!String.IsNullOrEmpty(result.AccessionNumber))
                        Platform.Log(LogLevel.Info, "Received SOP Instance {0} from {1} to {2} (A#:{3} StudyUid:{4})",
                                     result.SopInstanceUid, assoc.CallingAE, assoc.CalledAE, result.AccessionNumber,
                                     result.StudyInstanceUid);
                    else
                        Platform.Log(LogLevel.Info, "Received SOP Instance {0} from {1} to {2} (StudyUid:{3})",
                                     result.SopInstanceUid, assoc.CallingAE, assoc.CalledAE,
                                     result.StudyInstanceUid);
                }
            }
            catch (Exception e)
            {
                result = new DicomProcessingResult { DicomStatus = DicomStatuses.ProcessingFailure, ErrorMessage = e.Message };
            }

            if (!result.Successful)
            {
                Platform.Log(LogLevel.Warn, "Failure importing sop: {0}", result.ErrorMessage);
            }

            CleanupDirectory();

            server.SendCStoreResponse(presentationId, message.MessageId, message.AffectedSopInstanceUid, result.DicomStatus);
            return true;
        }

        private void CleanupDirectory()
        {
            if (!string.IsNullOrEmpty(_sourceFilename))
                try
                {
                    FileUtils.Delete(_sourceFilename);
                    _sourceFilename = null;
                }
                catch (Exception x)
                {
                    Platform.Log(LogLevel.Warn, x, "Unexpected error cleaning up streaming SOP Instance file.");
                }

            //try
            //{
            //    DirectoryUtility.DeleteIfEmpty(_sourceFolder, _filesystemStreamingFolder);
            //}
            //catch (Exception)
            //{
            //}
        }

        public void Dispose()
        {
            if (_fileStream != null)
            {
                try
                {
                    _fileStream.Flush(true);
                    _fileStream.Close();
                    _fileStream.Dispose();
                    _fileStream = null;
                }
                catch (Exception)
                {
                }
            }

            CleanupDirectory();
        }
    }
}
