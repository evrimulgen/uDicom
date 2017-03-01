using System.Runtime.Serialization;

namespace uDicom.WorkItemService.Interface
{
    [DataContract]
    public abstract class WorkItemRequest
    {
        [DataMember]
        public WorkItemPriorityEnum Priority { get; set; }

        [DataMember]
        public string WorkItemType { get; set; }
    }
}
