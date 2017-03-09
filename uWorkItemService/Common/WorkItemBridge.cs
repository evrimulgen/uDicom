using System;
using System.Threading;
using uDicom.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItemService.Common
{
    public class WorkItemBridge
    {
        public WorkItemData WorkItem { get; set; }
        public WorkItemRequest Request { get; set; }
        public Exception Exception { get; set; }

        public void Cancel()
        {
            if (WorkItem == null)
                return;

            if (WorkItem.Progress != null && !WorkItem.Progress.IsCancelable)
                return;

            if (WorkItem.Status == WorkItemStatusEnum.Deleted)
                return;

            WorkItemUpdateResponse response = null;

            Platform.GetService<IWorkItemService>(s => response = s.Update(new WorkItemUpdateRequest
            {
                Cancel = true,
                Identifier = WorkItem.Identifier
            }));

            // TODO (CR Jun 2012): The passed-in WorkItem contract should not be updated;
            // it should be done by the service and a new instance returned, or something should be returned by this
            // method to let the caller decide what to do.
            if (response.Item == null)
                WorkItem.Status = WorkItemStatusEnum.Deleted;
            else
                WorkItem = response.Item;
        }

        public void Reset()
        {
            if (WorkItem == null)
                return;

            if (WorkItem.Status == WorkItemStatusEnum.Deleted)
                return;

            WorkItemUpdateResponse response = null;

            Platform.GetService<IWorkItemService>(s => response = s.Update(new WorkItemUpdateRequest
            {
                Status = WorkItemStatusEnum.Pending,
                ProcessTime = DateTime.Now,
                Identifier = WorkItem.Identifier
            }));
            // TODO (CR Jun 2012): The passed-in WorkItem contract should not be updated;
            // it should be done by the service and a new instance returned, or something should be returned by this
            // method to let the caller decide what to do.

            if (response.Item == null)
                WorkItem.Status = WorkItemStatusEnum.Deleted;
            else
                WorkItem = response.Item;
        }

        public void Reprioritize(WorkItemPriorityEnum priority)
        {
            if (WorkItem == null)
                return;

            if (WorkItem.Status == WorkItemStatusEnum.Deleted)
                return;

            WorkItemUpdateResponse response = null;

            Platform.GetService<IWorkItemService>(s => response = s.Update(new WorkItemUpdateRequest
            {
                Priority = priority,
                Identifier = WorkItem.Identifier,
                ProcessTime = priority == WorkItemPriorityEnum.Stat ? DateTime.Now : default(DateTime?)
            }));

            // TODO (CR Jun 2012): The passed-in WorkItem contract should not be updated;
            // it should be done by the service and a new instance returned, or something should be returned by this
            // method to let the caller decide what to do.

            if (response.Item == null)
                WorkItem.Status = WorkItemStatusEnum.Deleted;
            else
                WorkItem = response.Item;
        }

        public void Delete()
        {
            if (WorkItem == null)
                return;

            if (WorkItem.Status == WorkItemStatusEnum.Deleted)
                return;

            WorkItemUpdateResponse response = null;

            Platform.GetService<IWorkItemService>(s => response = s.Update(new WorkItemUpdateRequest
            {
                Delete = true, // TODO (Marmot) - This delete flag could be removed, and we could just use the status
                Identifier = WorkItem.Identifier
            }));

            // TODO (CR Jun 2012): The passed-in WorkItem contract should not be updated;
            // it should be done by the service and a new instance returned, or something should be returned by this
            // method to let the caller decide what to do.

            if (response.Item == null)
                WorkItem.Status = WorkItemStatusEnum.Deleted;
            else
                WorkItem = response.Item;
        }

        protected void InsertRequest(WorkItemRequest request, WorkItemProgress progress)
        {
            WorkItemInsertResponse response = null;

            // Used for auditing purposes in the ShredHostService.
            if (string.IsNullOrEmpty(request.UserName))
                request.UserName = GetUserName();

            Request = request;

            Platform.GetService<IWorkItemService>(
                s => response = s.Insert(new WorkItemInsertRequest {Request = request, Progress = progress}));

            // TODO (CR Jun 2012): The passed-in WorkItem contract should not be updated;
            // it should be done by the service and a new instance returned, or something should be returned by this
            // method to let the caller decide what to do.

            if (response.Item == null)
                WorkItem.Status = WorkItemStatusEnum.Deleted;
            else
                WorkItem = response.Item;
        }

        /// <summary>
        ///     Get the first WorkItem where the request type matches <paramref name="request" /> and if its a
        ///     <see cref="WorkItemStudyRequest" />, the
        ///     Study Instance UID also matches.  The WorkItem must be Idle/Pending/InProgress status.
        /// </summary>
        /// <param name="request">The request to match </param>
        /// <returns>The matching WorkItem or null if none found.</returns>
        protected WorkItemData GetMatchingActiveWorkItem(WorkItemRequest request)
        {
            WorkItemData returnedItem = null;

            Platform.GetService(delegate(IWorkItemService s)
            {
                var response = s.Query(new WorkItemQueryRequest
                {
                    Type = request.WorkItemType,
                    //StudyInstanceUid =
                    //    request is WorkItemStudyRequest
                    //        ? (request as WorkItemStudyRequest).Study.StudyInstanceUid
                    //        : null
                });

                foreach (var relatedItem in response.Items)
                {
                    if (relatedItem.Status == WorkItemStatusEnum.Idle
                        || relatedItem.Status == WorkItemStatusEnum.Pending
                        || relatedItem.Status == WorkItemStatusEnum.InProgress)
                    {
                        returnedItem = relatedItem;
                        break;
                    }
                }
            });

            return returnedItem;
        }

        private static string GetUserName()
        {
            var p = Thread.CurrentPrincipal;
            if (p == null || string.IsNullOrEmpty(p.Identity.Name))
                return string.Format("{0}@{1}", Environment.UserName, Environment.UserDomainName);
            return p.Identity.Name;
        }
    }
}
