using System.Data.Entity;
using MySql.Data.Entity;
using uDicom.WorkItemService.WorkItemService;

namespace WorkItemTest.Model
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    internal class WorkItemContext : DbContext
    {
        public WorkItemContext()
            : base("WorkItemContext")
        {
            
        }

        public DbSet<WorkItem> WorkItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Database.SetInitializer(new NullDatabaseInitializer<WorkItemContext>());

            Database.SetInitializer(new DropCreateDatabaseAlways<WorkItemContext>());

            modelBuilder.Configurations.Add(new WorkItemMapping());

            base.OnModelCreating(modelBuilder);
        }
    }
}
