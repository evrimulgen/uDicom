using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uDicom.Common;
using uDicom.WorkItemService.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItemService.WorkItemService
{
    /// <summary>
    /// Implementation of <see cref="IWorkItemService"/> for processing requests to manipulate WorkItems.
    /// </summary>
    public class WorkItemService : IWorkItemService
    {
        #region Private Members

        private static WorkItemService _instance;

        #endregion

        #region Properties

        public static WorkItemService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new WorkItemService();
                }

                return _instance;
            }
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            try
            {
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Warn, e, "Failed to start WorkItemService.");
            }
        }

        public void Stop()
        {
        }

        public WorkItemInsertResponse Insert(WorkItemInsertRequest request)
        {
            // TODO (CR Jun 2012): The fact that there is special processing in here for particular types of work items
            // indicates there is something wrong with the design that may make adding custom work item types difficult.
            // Maybe the different "processors" need to perform the insert, or at least have some kind of method (rule)
            // for processing the insert?

            var response = new WorkItemInsertResponse();

            var now = DateTime.Now;

            var item = new WorkItem
            {
                Request = request.Request,
                Progress = request.Progress,
                Type = request.Request.WorkItemType,
                Priority = request.Request.Priority,
                ScheduledTime = now.AddSeconds(WorkItemServiceSettings.Default.InsertDelaySeconds),
                ProcessTime = now.AddSeconds(WorkItemServiceSettings.Default.InsertDelaySeconds),
                DeleteTime = now.AddMinutes(WorkItemServiceSettings.Default.DeleteDelayMinutes),
                ExpirationTime = now.AddSeconds(WorkItemServiceSettings.Default.ExpireDelaySeconds),
                RequestedTime = now,
                Status = WorkItemStatusEnum.Pending
            };

            IoC.Get<IWorkItemOperation>().AddWorkItem(item);
            
            WorkItemPublishSubscribeHelper.PublishWorkItemChanged(WorkItemsChangedEventType.Update, response.Item);
            if (WorkItemProcessor.Instance != null)
                WorkItemProcessor.Instance.SignalThread();

            return response;
        }

        public WorkItemUpdateResponse Update(WorkItemUpdateRequest request)
        {
            var response = new WorkItemUpdateResponse();

            var workItem = IoC.Get<IWorkItemOperation>().GetWorkItem(request.Identifier);
            if (workItem == null)
            {
                response.Item = null;
                return response;
            }

            var deleted = false;

            if (request.Delete.HasValue && request.Delete.Value)
            {
                if (workItem.Status != WorkItemStatusEnum.InProgress)
                {
                    workItem.Status = WorkItemStatusEnum.Deleted;
                    deleted = true;

                    // If StudyDelete we're removing, "undelete" the study
                    // CheckDeleteStudyCanceled(context, workItem);
                }
            }
            if (!deleted)
            {
                if (request.ExpirationTime.HasValue)
                    workItem.ExpirationTime = request.ExpirationTime.Value;
                if (request.Priority.HasValue)
                    workItem.Priority = request.Priority.Value;
                if (request.Status.HasValue && workItem.Status != WorkItemStatusEnum.InProgress)
                {
                    workItem.Status = request.Status.Value;
                    if (request.Status.Value == WorkItemStatusEnum.Canceled)
                        workItem.DeleteTime =
                            DateTime.Now.AddMinutes(WorkItemServiceSettings.Default.DeleteDelayMinutes);
                    else if (request.Status.Value == WorkItemStatusEnum.Pending)
                    {
                        workItem.ScheduledTime = DateTime.Now;
                        workItem.FailureCount = 0;
                    }

                    // Cache the UserIdentityContext for later use by the shred
                    //if (workItem.Request.WorkItemType.Equals(ImportFilesRequest.WorkItemTypeString) &&
                    //    request.Status.Value == WorkItemStatusEnum.Pending)
                    //    UserIdentityCache.Put(workItem.Oid, UserIdentityContext.CreateFromCurrentThreadPrincipal());
                }
                if (request.ProcessTime.HasValue)
                    workItem.ProcessTime = request.ProcessTime.Value;

                // Cancel 
                if (request.Cancel.HasValue && request.Cancel.Value)
                {
                    if (workItem.Progress == null || workItem.Progress.IsCancelable)
                    {
                        if (workItem.Status.Equals(WorkItemStatusEnum.Idle)
                            || workItem.Status.Equals(WorkItemStatusEnum.Pending))
                        {
                            workItem.Status = WorkItemStatusEnum.Canceled;

                            // If StudyDelete we're removing, "undelete" the study
                            // CheckDeleteStudyCanceled(context, workItem);
                        }
                        else if (workItem.Status.Equals(WorkItemStatusEnum.InProgress))
                        {
                            // Abort the WorkItem
                            WorkItemProcessor.Instance.Cancel(workItem.Oid);
                        }
                    }
                }

                // Pause 
                if (request.Pause.HasValue && request.Pause.Value)
                {
                    if (workItem.Progress == null || workItem.Progress.IsPauseable)
                    {
                        if (workItem.Status.Equals(WorkItemStatusEnum.Idle)
                            || workItem.Status.Equals(WorkItemStatusEnum.Pending))
                        {
                            workItem.Status = WorkItemStatusEnum.Pause;
                        }
                        else if (workItem.Status.Equals(WorkItemStatusEnum.InProgress))
                        {
                            // Pause the WorkItem 
                            
                        }
                    }
                }
               

                response.Item = WorkItemDataHelper.FromWorkItem(workItem);
            }

            WorkItemPublishSubscribeHelper.PublishWorkItemChanged(WorkItemsChangedEventType.Update, response.Item);

            return response;
        }

        public WorkItemQueryResponse Query(WorkItemQueryRequest request)
        {
            var response = new WorkItemQueryResponse();

            var dbList = IoC.Get<IWorkItemOperation>()
                .GetWorkItems(request.Type, request.Status, request.StudyInstanceUid, request.Identifier);

            var results = new List<WorkItemData>();

            foreach (var dbItem in dbList)
            {
                results.Add(WorkItemDataHelper.FromWorkItem(dbItem));
            }

            response.Items = results.ToArray();

            return response;
        }

        #endregion

        #region Private Methods

        //private void CheckDeleteStudyCanceled(DataAccessContext context, WorkItem workItem)
        //{
        //    // Force the study to be visible again if its a DeleteStudyRequest we're canceling
        //    if (workItem.Type.Equals(DeleteStudyRequest.WorkItemTypeString))
        //    {
        //        var studyBroker = context.GetStudyBroker();
        //        var study = studyBroker.GetStudy(workItem.StudyInstanceUid);
        //        if (study != null)
        //        {
        //            study.Deleted = false;
        //        }
        //    }
        //}

        #endregion
    }
}
