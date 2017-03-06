using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using log4net.Appender;
using uDicom.WorkItemService.Interface;
using WorkItemTest.Model;

namespace WorkItemTest
{
    [Export(typeof(IWorkItemQuery))]
    public class WorkItemQuery : IWorkItemQuery
    {
        private readonly List<WorkItem> _nowRunningWorkItems = new List<WorkItem>();
        
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
