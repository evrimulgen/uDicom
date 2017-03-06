using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using uDicom.WorkItemService.Interface;
using WorkItemTest.Model;

namespace WorkItemTest
{
    [Export(typeof(WorkItemStatusProxy)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class WorkItemStatusProxyImpl : WorkItemStatusProxy
    {
        public override void Fail(WorkItemFailureType failureType, DateTime failureTime, int maxRetryCount)
        {
            using (var ctx = new WorkItemContext())
            {
                Item = (from row in ctx.WorkItems
                             where row.Oid == Item.Oid
                             select row).FirstOrDefault();

                if (Item != null)
                {
                    Item.Progress = Progress;
                    Item.FailureCount = Item.FailureCount + 1;
                    Item.DeleteTime = DateTime.Now.AddMinutes(240);

                    if (Item.FailureCount >= maxRetryCount
                    || failureType == WorkItemFailureType.Fatal)
                    {
                        Item.Status = WorkItemStatusEnum.Failed;
                        Item.ExpirationTime = DateTime.Now;
                    }
                    else
                    {
                        Item.ProcessTime = failureTime;
                        if (Item.ExpirationTime < Item.ProcessTime)
                            Item.ExpirationTime = Item.ProcessTime;
                        Item.Status = WorkItemStatusEnum.Pending;
                    }
                }

                ctx.SaveChanges();
            }

            Publish(false);
        }

        public override void Postpone(TimeSpan delay)
        {
            using (var ctx = new WorkItemContext())
            {
                var result = from row in ctx.WorkItems
                    where row.Oid == Item.Oid
                    select row;
            }
        }

        public override void Complete()
        {
            using (var ctx = new WorkItemContext())
            {
                Item = (from row in ctx.WorkItems
                             where row.Oid == Item.Oid
                             select row).FirstOrDefault();

                DateTime now = DateTime.Now;

                Progress.StatusDetails = string.Empty;

                if (Item != null)
                {
                    Item.Progress = Progress;
                    Item.ExpirationTime = now;
                    Item.DeleteTime = now.AddMinutes(240);
                    Item.Status = WorkItemStatusEnum.Complete;
                }

                ctx.SaveChanges();
            }
        }

        public override void Idle()
        {
            using (var ctx = new WorkItemContext())
            {
                Item = (from row in ctx.WorkItems
                              where row.Oid == Item.Oid
                              select row).FirstOrDefault();

                DateTime now = DateTime.Now;

                if (Item != null)
                {
                    Item.Progress = Progress;
                    Item.ExpirationTime = now;
                    Item.DeleteTime = now.AddMinutes(240);
                    Item.Status = WorkItemStatusEnum.Idle;
                }

                ctx.SaveChanges();
            }
        }

        public override void Canceling()
        {
            using (var ctx = new WorkItemContext())
            {
                Item = (from row in ctx.WorkItems
                              where row.Oid == Item.Oid
                              select row).FirstOrDefault();

                DateTime now = DateTime.Now;

                if (Item != null)
                {
                    Item.Progress = Progress;
                    Item.Status = WorkItemStatusEnum.Canceling;
                }

                ctx.SaveChanges();
            }
        }

        public override void Cancel()
        {
            using (var ctx = new WorkItemContext())
            {
                Item = (from row in ctx.WorkItems
                              where row.Oid == Item.Oid
                              select row).FirstOrDefault();

                DateTime now = DateTime.Now;

                if (Item != null)
                {
                    Item.Progress = Progress;
                    Item.ExpirationTime = now;
                    Item.DeleteTime = now.AddMinutes(240);
                    Item.Status = WorkItemStatusEnum.Canceled;
                }

                ctx.SaveChanges();
            }
        }

        public override void Delete()
        {
            using (var ctx = new WorkItemContext())
            {
                Item = (from row in ctx.WorkItems
                              where row.Oid == Item.Oid
                              select row).FirstOrDefault();

                DateTime now = DateTime.Now;

                if (Item != null)
                {
                    Item.Progress = Progress;
                    Item.Status = WorkItemStatusEnum.Deleted;
                }

                ctx.SaveChanges();
            }

            Publish(false);
        }

        protected override void Publish(bool saveToDatabase)
        {
            
        }
    }
}
