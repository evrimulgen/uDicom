using System.Runtime.Serialization;

namespace uDicom.WorkItemService.Interface
{
    public enum WorkItemPriorityEnum
    {
        [EnumMember]
        Stat = 1,

        [EnumMember]
        High = 2,

        [EnumMember]
        Normal = 3
    }

    public enum WorkItemStatusEnum
    {
        [EnumMember]
        Pending = 1,

        [EnumMember]
        InProgress = 2,

        [EnumMember]
        Complete = 3,

        [EnumMember]
        Idle = 4,

        [EnumMember]
        Deleted = 5,

        [EnumMember]
        Canceled = 6,

        [EnumMember]
        Failed = 7,

        [EnumMember]
        DeleteInProgress = 8,

        [EnumMember]
        Canceling = 9,
    }
}
