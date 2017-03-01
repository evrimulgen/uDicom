using System.Collections.Generic;

namespace uDicom.WorkItemService.Interface
{
    public interface IWorkItemQuery
    {
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
