using System.Collections.Generic;
using uDicom.WorkItemService.Common;
using uDicom.WorkItemService.WorkItemService;

namespace uDicom.WorkItemService.Interface
{
    public interface IWorkItemOperation
    {
        bool AddWorkItem(WorkItem item);

        WorkItem GetWorkItem(long oid);

        List<WorkItem> GetWorkItems(string type, WorkItemStatusEnum? status, string uid, long? oid );
        List<WorkItem> GetWorkItems(int count, WorkItemPriorityEnum priority);

        /// <summary>
        /// // Get WorkItems that have expired that need to be deleted
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        List<WorkItem> GetWorkItemsToDelete(int count);

        /// <summary>
        /// Called on startup to reset InProgress WorkItems back to Pending
        /// </summary>
        void ResetInProgressWorkItems();
    }

    
}
