using System;
using uDicom.WorkItemService.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItemService.WorkItemService
{
    public class WorkItem
    {
        public long Oid { get; set; }

        public DateTime ScheduledTime { get; set; }

        public DateTime ProcessTime { get; set; }

        public DateTime ExpirationTime { get; set; }

        public DateTime DeleteTime { get; set; }

        public DateTime RequestedTime { get; set; }

        public WorkItemStatusEnum Status { get; set; }

        public string Type { get; set; }

        public WorkItemPriorityEnum Priority { get; set; }

        public string StudyInstanceUid { get; set; }

        public string SerializedProgress { get; set; }

        public string SerializedRequest { get; set; }

        public int FailureCount { get; set; }

        public WorkItemRequest Request
        {
            get { return Serializer.DeserializeWorkItemRequest(SerializedRequest); }
            set { SerializedRequest = Serializer.SerializeWorkItemRequest(value); }
        }

        public WorkItemProgress Progress
        {
            get { return Serializer.DeserializeWorkItemProgress(SerializedProgress); }
            set { SerializedProgress = Serializer.SerializeWorkItemProgress(value); }
        }
    }
}
