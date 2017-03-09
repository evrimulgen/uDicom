using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uDicom.WorkItemService.Common;

namespace uDicom.WorkItemService.WorkItemService
{
    public static class WorkItemDataHelper
    {
        public static WorkItemData FromWorkItem(WorkItem item)
        {
            return new WorkItemData
            {
                Type = item.Type,
                Status = item.Status,
                Priority = item.Priority,
                DeleteTime = item.DeleteTime,
                ExpirationTime = item.ExpirationTime,
                ScheduledTime = item.ScheduledTime,
                RequestedTime = item.RequestedTime,
                FailureCount = item.FailureCount,
                Identifier = item.Oid,
                ProcessTime = item.ProcessTime,
                StudyInstanceUid = item.StudyInstanceUid,
                Request = item.Request,
                Progress = item.Progress
            };
        }
    }
}
