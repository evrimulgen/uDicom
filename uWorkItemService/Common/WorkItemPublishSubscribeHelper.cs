using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uDicom.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItemService.Common
{
    public static class WorkItemPublishSubscribeHelper
    {
        private const string WorkItemsChanged = "WorkItemsChanged";
        private const string StudiesCleared = "StudiesCleared";

        public static void Subscribe(IWorkItemActivityCallback callback)
        {
            try
            {
                SubscriptionManager<IWorkItemActivityCallback>.Subscribe(callback, WorkItemsChanged);
                SubscriptionManager<IWorkItemActivityCallback>.Subscribe(callback, StudiesCleared);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
                throw;
            }
        }

        public static void Unsubscribe(IWorkItemActivityCallback callback)
        {
            try
            {
                SubscriptionManager<IWorkItemActivityCallback>.Unsubscribe(callback, WorkItemsChanged);
                SubscriptionManager<IWorkItemActivityCallback>.Unsubscribe(callback, StudiesCleared);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
                throw;
            }
        }

        public static void PublishWorkItemChanged(WorkItemsChangedEventType eventType, WorkItemData workItem)
        {
            PublishWorkItemsChanged(eventType, new List<WorkItemData> { workItem });
        }

        public static void PublishWorkItemsChanged(WorkItemsChangedEventType eventType, List<WorkItemData> workItems)
        {
            try
            {
                PublishManager<IWorkItemActivityCallback>.Publish(WorkItemsChanged, eventType, workItems);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Warn, e, "Unexpected error attempting to publish WorkItemsChanged notification.");
            }
        }

        public static void PublishStudiesCleared()
        {
            try
            {
                PublishManager<IWorkItemActivityCallback>.Publish(StudiesCleared);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Warn, e, "Unexpected error attempting to publish StudiesCleared notification.");
            }
        }
    }
}
