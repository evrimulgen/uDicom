using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using uDicom.WorkItemService.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItem.Archive.Import
{
    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class ImportFilesRequest : WorkItemRequest
    {
        public static string WorkItemTypeString = "Import";

        public ImportFilesRequest()
        {
            WorkItemType = WorkItemTypeString;
            Priority = WorkItemPriorityEnum.High;
            CancellationCanResultInPartialStudy = true;

            BadFileBehaviour = BadFileBehaviourEnum.Ignore;
            FileImportBehaviour = FileImportBehaviourEnum.Copy;
        }

        public override WorkItemConcurrency ConcurrencyType
        {
            get { return WorkItemConcurrency.NonExclusive; }
        }

        [DataMember(IsRequired = true)]
        public bool Recursive { get; set; }

        [DataMember(IsRequired = true)]
        public List<string> FileExtensions { get; set; }

        [DataMember(IsRequired = true)]
        public List<string> FilePaths { get; set; }

        [DataMember(IsRequired = true)]
        public BadFileBehaviourEnum BadFileBehaviour { get; set; }

        [DataMember(IsRequired = true)]
        public FileImportBehaviourEnum FileImportBehaviour { get; set; }
    }

    [DataContract(Name = "BadFileBehaviour", Namespace = ImageViewerWorkItemNamespace.Value)]
    public enum BadFileBehaviourEnum
    {
        [EnumMember]
        Ignore = 0,

        [EnumMember]
        Move,

        [EnumMember]
        Delete
    }

    [DataContract(Name = "FileImportBehaviour", Namespace = ImageViewerWorkItemNamespace.Value)]
    public enum FileImportBehaviourEnum
    {
        [EnumMember]
        Move = 0,

        [EnumMember]
        Copy,

        [EnumMember]
        Save
    }

    public class ImportFilesProgress : WorkItemProgress
    {
        public ImportFilesProgress()
        {
            IsCancelable = true;
        }

        [DataMember(IsRequired = true)]
        public int TotalFilesToImport { get; set; }

        [DataMember(IsRequired = true)]
        public int NumberOfFilesImported { get; set; }

        [DataMember(IsRequired = true)]
        public int NumberOfImportFailures { get; set; }

        [DataMember(IsRequired = true)]
        public int PathsToImport { get; set; }

        [DataMember(IsRequired = true)]
        public int PathsImported { get; set; }

        /// <summary>
        /// When the work item completes, this value is set to either true or false to indicate
        /// whether or not all the files were enumerated. If false, the work item terminated prematurely.
        /// </summary>
        [DataMember(IsRequired = true)]
        public bool? CompletedEnumeration { get; set; }

        public int TotalImportsProcessed
        {
            get { return NumberOfFilesImported + NumberOfImportFailures; }
        }

        public override string Status
        {
            get
            {
                //If enumeration didn't complete, don't give the user a false impression of either
                //the total number of files to import, or the total number of failures.
                //If there were failures, the status will be "Failed", and that's good enough.
                if (CompletedEnumeration.HasValue && !CompletedEnumeration.Value)
                {
                    return string.Format(SR.ImportFilesProgress_StatusEnumerationIncomplete, NumberOfFilesImported);
                }

                //If the work item hasn't completed yet, or if it is complete and all files were enumerated (and possibly failed),
                //only then do we report all the numbers (#imported, total, #failures).
                return string.Format(SR.ImportFilesProgress_Status, NumberOfFilesImported, TotalFilesToImport, NumberOfImportFailures);
            }
        }

        public override Decimal PercentComplete
        {
            get
            {
                // TODO (CR Jun 2012 - Med): Shouldn't this be based on files, not paths?
                // (SW) Paths was used because we don't know the total file count up front, and didn't want to enumerate the files before processing
                // them.  This was a work around to show real progress.
                if (PathsToImport > 0)
                    return (Decimal)PathsImported / PathsToImport;

                return new decimal(0.0);
            }
        }

        public override Decimal PercentFailed
        {
            get
            {
                if (NumberOfImportFailures > 0)
                    return (Decimal)NumberOfImportFailures / TotalImportsProcessed;

                return new decimal(0.0);
            }
        }
    }
}