using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using uDicom.WorkItemService.Common;
using uDicom.WorkItemService.Interface;
using WorkItemServer.Model;

namespace WorkItemServer
{
    [Export(typeof(IWorkItemOperation))]
    public class WorkItemOperation : IWorkItemOperation
    {
        private readonly List<WorkItem> _nowRunningWorkItems = new List<WorkItem>();

        public bool AddWorkItem(WorkItem item)
        {
            using (var ctx = new WorkItemContext())
            {
                ctx.WorkItems.Add(item);

                ctx.SaveChanges();
            }

            return true;
        }

        public WorkItem GetWorkItem(long oid)
        {
            using (var ctx = new WorkItemContext())
            {
                var obj = (from row in ctx.WorkItems
                    where row.Oid == oid
                    select row).FirstOrDefault();

                return obj;
            }
        }

        public void SaveWorkItem(WorkItem item)
        {
            using (var ctx = new WorkItemContext())
            {
                var obj = (from row in ctx.WorkItems
                           where row.Oid == item.Oid
                           select row).FirstOrDefault();

                if (obj != null)
                {
                    obj.Status = item.Status;
                    obj.Request = item.Request;
                }

                ctx.SaveChanges();
            }
        }

        public List<WorkItem> GetWorkItems(string type, WorkItemStatusEnum? status, string uid, long? oid)
        {
            using (var ctx = new WorkItemContext())
            {
                var workItems = from row in ctx.WorkItems
                    select row;

                if (!string.IsNullOrEmpty(type))
                {
                    workItems = from row in workItems
                        where row.Type.Equals(type)
                        select row;
                }

                if (status != null)
                {
                    workItems = from row in workItems
                        where row.Status == status.Value
                        select row;
                }

                if (!string.IsNullOrEmpty(uid))
                {
                    workItems = from row in workItems
                        where row.StudyInstanceUid.Equals(uid)
                        select row;
                }

                if (oid != null)
                {
                    workItems = from row in workItems
                        where row.Oid == oid.Value
                        select row;
                }

                return workItems.ToList();
            }
        }

        public List<WorkItem> GetWorkItems(int count, WorkItemPriorityEnum priority)
        {
            _nowRunningWorkItems.Clear();

            using (var context = new WorkItemContext())
            {
                var now = DateTime.Now;

                var workItems = (from row in context.WorkItems
                    where row.ScheduledTime < now && row.Priority == priority
                          && (row.Status == WorkItemStatusEnum.Pending || row.Status == WorkItemStatusEnum.Idle)
                    orderby row.Priority, row.ScheduledTime
                    select row).Take(count);


                foreach (var item in workItems)
                {
                    // 标记任务处于正在运行状态，避免任务的多次执行
                    item.Status = WorkItemStatusEnum.InProgress;
                    _nowRunningWorkItems.Add(item);
                }

                return new List<WorkItem>(_nowRunningWorkItems);
            }

        }

        public List<WorkItem> GetWorkItemsToDelete(int count)
        {
            using (var context = new WorkItemContext())
            {
                var now = DateTime.Now;

                var workItems = (from row in context.WorkItems
                    where row.Status == WorkItemStatusEnum.Complete && row.DeleteTime < now
                    select row).Take(count).ToList();

                foreach (var item in workItems)
                {
                    item.Status = WorkItemStatusEnum.DeleteInProgress;
                }

                context.SaveChanges();

                if (workItems.Any())
                    return workItems;

                workItems = (from row in context.WorkItems
                    where row.Status == WorkItemStatusEnum.Deleted
                    select row).Take(count).ToList();

                foreach (var item in workItems)
                {
                    item.Status = WorkItemStatusEnum.DeleteInProgress;
                }

                context.SaveChanges();

                return workItems;
            }
        }

        public void ResetInProgressWorkItems()
        {
            using (var context = new WorkItemContext())
            {
                var query = from row in context.WorkItems
                    where row.Status == WorkItemStatusEnum.InProgress
                          || row.Status == WorkItemStatusEnum.DeleteInProgress
                          || row.Status == WorkItemStatusEnum.Canceling
                            orderby row.Priority
                    select row;

                foreach (var item in query.ToList())
                {
                    switch (item.Status)
                    {
                        case WorkItemStatusEnum.InProgress:
                            item.Status = WorkItemStatusEnum.Pending;
                            break;
                        case WorkItemStatusEnum.DeleteInProgress:
                            item.Status = WorkItemStatusEnum.Deleted;
                            break;
                        case WorkItemStatusEnum.Canceling:
                            item.Status = WorkItemStatusEnum.Canceled;
                            break;
                    }
                }

                context.SaveChanges();
            }
        }
    }
}
