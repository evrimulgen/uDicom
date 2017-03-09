using System;
using System.Collections.Generic;
using uDicom.Common;

namespace uDicom.WorkItemService.Common
{
    public enum WorkItemsChangedEventType
    {
        /// <summary>
        /// The event was raised because the work item(s) have been updated.
        /// </summary>
        Update,

        /// <summary>
        /// The event was raised in response to an explicit refresh request.
        /// </summary>
        Refresh
    }

    public class WorkItemsChangedEventArgs : EventArgs
    {
        public WorkItemsChangedEventArgs(WorkItemsChangedEventType eventType, List<WorkItem> items)
        {
            Platform.CheckForNullReference(items, "items");

            ChangedItems = items;
            EventType = eventType;
        }

        public WorkItemsChangedEventType EventType { get; private set; }

        public List<WorkItem> ChangedItems { get; private set; }
    }
}
