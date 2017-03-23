using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using uDicom.Common;
using uDicom.WorkItemService.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItemService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public sealed class WorkItemActivityMonitorServiceType : IWorkItemActivityMonitorService, IDisposable
    {
        private readonly IWorkItemActivityCallback _callback;

        public WorkItemActivityMonitorServiceType()
        {
            _callback = OperationContext.Current.GetCallbackChannel<IWorkItemActivityCallback>();
        }

        #region Implementation of IWorkItemActivityMonitorService

        public WorkItemSubscribeResponse Subscribe(WorkItemSubscribeRequest request)
        {
            try
            {
                WorkItemPublishSubscribeHelper.Subscribe(_callback);
                return new WorkItemSubscribeResponse();
            }
            catch (Exception e)
            {
                var message = "Failed to process WorkItem Subscribe request.";
                var exceptionMessage = String.Format("{0}\nDetail:{1}", message, e.Message);
                throw new WorkItemServiceException(exceptionMessage);
            }
        }

        public WorkItemUnsubscribeResponse Unsubscribe(WorkItemUnsubscribeRequest request)
        {
            try
            {
                WorkItemPublishSubscribeHelper.Unsubscribe(_callback);
                return new WorkItemUnsubscribeResponse();
            }
            catch (Exception e)
            {
                var message = "Failed to process WorkItem Unsubscribe request.";
                var exceptionMessage = String.Format("{0}\nDetail:{1}", message, e.Message);
                throw new WorkItemServiceException(exceptionMessage);
            }
        }

        public void Refresh(WorkItemRefreshRequest request)
        {
            ThreadPool.QueueUserWorkItem(
                delegate
                {
                    try
                    {
                        //using (var context = new DataAccessContext())
                        //{
                        //    var broker = context.GetWorkItemBroker();

                        //    var dbList = broker.GetWorkItems(null, null, null);

                        //    // send in batches of 200
                        //    foreach (var batch in BatchItems(dbList, 200))
                        //    {
                        //        WorkItemPublishSubscribeHelper.PublishWorkItemsChanged(WorkItemsChangedEventType.Refresh, batch.Select(WorkItemDataHelper.FromWorkItem).ToList());
                        //    }
                        //}
                    }
                    catch (Exception e)
                    {
                        var message = "Failed to process WorkItem Update request.";
                        var exceptionMessage = String.Format("{0}\nDetail:{1}", message, e.Message);
                        Platform.Log(LogLevel.Error, e, exceptionMessage);
                        // Don't rethrow here, we're in a thread pool anyways.
                    }
                });
        }

        public WorkItemPublishResponse Publish(WorkItemPublishRequest request)
        {
            try
            {
                WorkItemPublishSubscribeHelper.PublishWorkItemChanged(WorkItemsChangedEventType.Update, request.Item);
                return new WorkItemPublishResponse();
            }
            catch (Exception e)
            {
                var message = "Failed to process WorkItem Publish request.";
                var exceptionMessage = String.Format("{0}\nDetail:{1}", message, e.Message);
                throw new WorkItemServiceException(exceptionMessage);
            }
        }
        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            try
            {
                WorkItemPublishSubscribeHelper.Unsubscribe(_callback);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
            }
        }

        #endregion

        private static IEnumerable<List<T>> BatchItems<T>(IEnumerable<T> items, int batchSize)
        {
            var batch = new List<T>();
            foreach (var item in items)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<T>();
                }
            }

            if (batch.Count > 0)
                yield return batch;
        }
    }
}
