using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using uDicom.Common;
using uDicom.WorkItemService.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItemService
{
    /// <summary>
    /// 业务场景 
    /// 1. 用户创建了m+n个优先级是Normal 任务，处理单元容许同时有 m个（ 优先级为normal的任务执行
    /// 容许n个 Stat & High优先级的任务执行，当用户将一个任务的优先级提高后，被提高优先级的任务可以 
    /// 立即被调度到可以运行高优先任务的预留线程中，直到 m + n 线程都处于运行状态。
    /// </summary>
    public sealed class WorkItemProcessor : QueueProcessor
    {
        private readonly Dictionary<string, IWorkItemProcessorFactory> _extensions =
            new Dictionary<string, IWorkItemProcessorFactory>();

        private readonly WorkItemThreadPool _threadPool;
        private readonly ManualResetEvent _threadStop;

        #region Constructor

        private WorkItemProcessor(int numberStatThreads, int numberNormalThreads,
            string name)
        {
            Name = name;

            _threadStop = new ManualResetEvent(false);

            _threadPool = new WorkItemThreadPool(numberStatThreads, numberNormalThreads)
            {
                ThreadPoolName = name + "Pool"
            };

            // IOC Get all item processor 

            var factories = IoC.GetAll<IWorkItemProcessorFactory>().ToList();

            if (factories.Count == 0)
            {
                // Log Something
                Platform.Log(LogLevel.Error, "No WorkItemFactory Extensions found.");
            }
            else
            {
                foreach (var factory in factories)
                {
                    var type = factory.GetWorkQueueType();
                    _extensions.Add(type, factory);
                }
            }
        }

        #endregion

        #region Public Properties 

        public static WorkItemProcessor Instance { get; private set; }

        /// <summary>
        ///     The Thread Name.
        /// </summary>
        public override string Name { get; }

        /// <summary>
        ///     The processing thread.
        /// </summary>
        /// <remarks>
        ///     This method queries the database for WorkItem entries to work on, and then uses
        ///     a thread pool to process the entries.
        /// </remarks>
        protected override void RunCore()
        {
            // Reset any in progress WorkItems if we crashed while processing.
            ResetInProgressWorkItems();

            if (!_threadPool.Active)
                _threadPool.Start();

            Platform.Log(LogLevel.Info, "WorkItem Processor running...");

            while (true)
            {
                if (StopRequested)
                    return;

                // First, use upto 1/2 of the normal threads to delete 
                // work items that should be purged, we had testing issues where the deletion items were
                // never removed, and decided to be a bit aggressive with ensuring they get removed.
                if (_threadPool.NormalThreadsAvailable > 1)
                {
                    var deleteList = GetWorkItemsToDelete(_threadPool.NormalThreadsAvailable/2);
                    if (deleteList != null && deleteList.Count > 0)
                    {
                        QueueWorkItems(deleteList);
                    }
                }

                var list = GetWorkItems(_threadPool.StatThreadsAvailable, _threadPool.NormalThreadsAvailable);

                if (list == null)
                {
                    /* No threads available, wait for one to complete. */
                    if (_threadStop.WaitOne(2500, false))
                        _threadStop.Reset();
                    continue;
                }
                if (list.Count == 0)
                {
                    // No result found 
                    if (_threadStop.WaitOne(2500, false))
                        _threadStop.Reset();
                    continue;
                }

                // Queue up the workItems
                QueueWorkItems(list);
            }
        }

        internal static List<WorkItem> GetWorkItems(int statThreadsAvailable, int normalThreadsAvailable)
        {
            var query = IoC.Get<IWorkItemOperation>();
            List<WorkItem> list = null;

            if (statThreadsAvailable > 0)
            {
                list = query.GetWorkItems(statThreadsAvailable, WorkItemPriorityEnum.Stat);
            }

            if ((list == null || list.Count == 0) && normalThreadsAvailable > 0)
            {
                list = query.GetWorkItems(normalThreadsAvailable, WorkItemPriorityEnum.High);
            }

            if ((list == null || list.Count == 0) && normalThreadsAvailable > 0)
            {
                list = query.GetWorkItems(normalThreadsAvailable, WorkItemPriorityEnum.Normal);
            }

            return list;
        }

        #endregion

        #region Public Static method 

        /// <summary>
        ///     Initialize the singleton <see cref="WorkItemProcessor" />.
        /// </summary>
        /// <param name="numberStatThreads">The number of thread to process stat and high priority work item.</param>
        /// <param name="numberNormalThreads">The number of thread to process normal priority work item.</param>
        /// <param name="name"></param>
        public static void CreateProcessor(int numberStatThreads, int numberNormalThreads, string name)
        {
            if (Instance != null) throw new ApplicationException("Processor already created!");

            Instance = new WorkItemProcessor(numberStatThreads, numberNormalThreads, name);
        }

        #endregion

        #region Public Method 

        /// <summary>
        ///     Signal the processor to stop sleeping and check for Shutdown or new WorkItem
        /// </summary>
        public void SignalThread()
        {
            _threadStop.Set();
        }

        /// <summary>
        ///     Stop the WorkItem processor
        /// </summary>
        public override void RequestStop()
        {
            base.RequestStop();

            _threadStop.Set();

            if (_threadPool.Active)
                _threadPool.Stop();
        }


        /// <summary>
        ///     Cancel a current running WorkItem
        /// </summary>
        /// <param name="workItemOid"></param>
        public void Cancel(long workItemOid)
        {
            _threadPool.Cancel(workItemOid);
        }

        public void Pause(long workItemOid)
        {
            _threadPool.Pause(workItemOid);
        }

        #endregion

        #region Private method 

        private void QueueWorkItems(IEnumerable<WorkItem> list)
        {
            try
            {
                foreach (var item in list)
                {
                    if (!_extensions.ContainsKey(item.Request.WorkItemType))
                    {
                        Platform.Log(LogLevel.Error,
                            "No extensions loaded for WorkItem item type: {0}.  Failing item.",
                            item.Type);

                        //Just fail the WorkQueue item, not much else we can do
                        var proxy = IoC.Get<WorkItemStatusProxy>();
                        proxy.Item = item;
                        proxy.Fail("No plugin to handle WorkItem type: " + item.Type, WorkItemFailureType.Fatal);
                        continue;
                    }

                    try
                    {
                        var factory = _extensions[item.Request.WorkItemType];
                        var processor = factory.GetItemProcessor();

                        // Enqueue the actual processing of the item to the thread pool.  
                        _threadPool.Enqueue(processor, item, ExecuteProcessor);
                    }
                    catch (Exception e)
                    {
                        Platform.Log(LogLevel.Error, e, "Unexpected exception creating WorkItem processor.");

                        var proxy = IoC.Get<WorkItemStatusProxy>();
                        proxy.Item = item;
                        proxy.Fail("No plugin to handle WorkItem type: " + item.Type, WorkItemFailureType.Fatal);
                    }
                }
            }
            catch (Exception e)
            {
                // Wait for only 3 seconds
                Platform.Log(LogLevel.Error, e, "Exception occured when processing WorkItem item.");
                _threadStop.WaitOne(3000, false);
            }
        }

        /// <summary>
        ///     The actual delegate
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="queueItem"></param>
        private void ExecuteProcessor(IWorkItemProcessor processor, WorkItem queueItem)
        {
            var proxy = IoC.Get<WorkItemStatusProxy>();
            proxy.Item = queueItem;

            try
            {
                Platform.Log(proxy.LogLevel, "Starting processing of {0} WorkItem for OID {1}", queueItem.Type,
                    queueItem.Oid);

                if (proxy.Item.Status == WorkItemStatusEnum.Deleted ||
                    proxy.Item.Status == WorkItemStatusEnum.DeleteInProgress)
                {
                    if (!processor.Initialize(proxy))
                    {
                        Platform.Log(LogLevel.Error,
                            "Unable to intialize WorkItem processor for: {0}.  Directly deleting.",
                            proxy.Request.WorkItemType);
                        proxy.Delete();
                        return;
                    }

                    // Delete the entry
                    processor.Delete();
                    return;
                }

                if (!processor.Initialize(proxy))
                {
                    proxy.Postpone();
                    return;
                }

                processor.Process();
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e,
                    "Unexpected exception when processing WorkQueue item of type {0}.  Failing Queue item. (Oid: {1})",
                    queueItem.Type,
                    queueItem.Oid);
                var error = e.InnerException != null ? e.InnerException.Message : e.Message;

                proxy.Fail(error, WorkItemFailureType.NonFatal);
            }
            finally
            {
                // Signal the parent thread, so it can query again
                _threadStop.Set();

                // Cleanup the processor
                processor.Dispose();
                Platform.Log(proxy.LogLevel, "Done processing of {0} WorkItem for OID {1} and status {2}",
                    proxy.Item.Type, proxy.Item.Oid, proxy.Item.Status);
            }
        }


        /// <summary>
        ///     Method for getting next <see cref="WorkItem" /> entry.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <remarks>
        /// </remarks>
        /// <returns>
        ///     A <see cref="WorkItem" /> entry if found, or else null;
        /// </returns>
        private List<WorkItem> GetWorkItemsToDelete(int count)
        {
            try
            {
                var query = IoC.Get<IWorkItemOperation>();

                return query.GetWorkItemsToDelete(count);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e, "Can't find IWorkItemOperation from IoC container");
            }

            return null;
        }

        /// <summary>
        ///     Called on startup to reset InProgress WorkItems back to Pending.
        /// </summary>
        private void ResetInProgressWorkItems()
        {
            try
            {
                var query = IoC.Get<IWorkItemOperation>();

                query.ResetInProgressWorkItems();
            }
            catch (ArgumentNullException ex)
            {
                Platform.Log(LogLevel.Error, ex, "Can't find IWorkItemOperation from Ioc Container");
            }
            
        }

        #endregion
    }
}
