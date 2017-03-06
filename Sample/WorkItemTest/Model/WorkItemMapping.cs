using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using uDicom.WorkItemService.Interface;

namespace WorkItemTest.Model
{
    internal class WorkItemMapping : EntityTypeConfiguration<WorkItem>
    {
        public WorkItemMapping()
        {
            HasKey(t => t.Oid);

            // Table & Colume 
            ToTable("WorkItem");

            Ignore(t => t.Request);
            Ignore(t => t.Progress);

        }
    }
}
