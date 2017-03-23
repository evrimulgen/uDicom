using System.Collections.Generic;
using System.Runtime.Serialization;
using uDicom.WorkItemService.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItem.Archive.DicomRetrieve
{
    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public abstract class DicomRetrieveRequest : WorkItemRequest
    {
        public static string WorkItemTypeString = "DicomRetrieve";

        public override WorkItemConcurrency ConcurrencyType
        {
            get { return WorkItemConcurrency.StudyUpdateTrigger; }
        }

        [DataMember]
        public string ServerName { get; set; }

        [DataMember(IsRequired = false)]
        public string ServerHostname { get; set; }

        [DataMember(IsRequired = false)]
        public string ServerAETitle { get; set; }

        [DataMember(IsRequired = false)]
        public int ServerPort { get; set; }
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class DicomRetrieveStudyRequest : DicomRetrieveRequest
    {
        public DicomRetrieveStudyRequest()
        {
            WorkItemType = WorkItemTypeString;
            Priority = WorkItemPriorityEnum.Normal;
        }
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class DicomRetrieveSeriesRequest : DicomRetrieveRequest
    {
        public DicomRetrieveSeriesRequest()
        {
            WorkItemType = WorkItemTypeString;
            Priority = WorkItemPriorityEnum.Normal;
        }

        [DataMember(IsRequired = false)]
        public List<string> SeriesInstanceUids { get; set; }
    }
}