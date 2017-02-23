using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.History;
using System.Linq.Expressions;
using App.PACS.Mapping;
using MySql.Data.Entity;
using uWs.PACS.Model;
using UIH.Dicom;

namespace App.PACS.Model
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class PacsContext : DbContext
    {
        public PacsContext() :
            base("Name = PacsContext")
        {
            
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Study> Studies { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<Instance> Instances { get; set; }
        public DbSet<File> Files { get; set; }

        public DbSet<FileSystem> FileSystems { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<ServerPartition> ServerPartitions { get; set; }

        public DbSet<SupportedSopClass> SupportedSopClasses { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new PacsServerInitializer());

            modelBuilder.Configurations.Add(new PatientMap());
            modelBuilder.Configurations.Add(new StudyMap());
            modelBuilder.Configurations.Add(new FileSystemMap());
            modelBuilder.Configurations.Add(new SeriesMap());
            modelBuilder.Configurations.Add(new InstanceMap());
            modelBuilder.Configurations.Add(new FileMap());
            modelBuilder.Configurations.Add(new DeviceMap());
            modelBuilder.Configurations.Add(new SupportedSopClassMap());
            modelBuilder.Configurations.Add(new ServerPartitionMap());

            base.OnModelCreating(modelBuilder);
        }

        #region Database initializer 

        public class PacsServerInitializer : DropCreateDatabaseAlways<PacsContext>
        {
            #region Private member

            private static readonly List<SopClass> StorageAbstractSyntaxList =
                new List<SopClass>()
                    {
                    SopClass.CtImageStorage,
                    SopClass.MrImageStorage,
                    SopClass.EnhancedMrImageStorage,
                    SopClass.SecondaryCaptureImageStorage
                    };

            #endregion

            protected override void Seed(PacsContext context)
            {
                var fs = new FileSystem()
                {
                    DirPath = @"D:\Fs",
                    Description = "Primary Filesystem",
                    Name = "Primary",
                    HighWatermark = 80,
                    LowWatermark = 20
                };

                var partition = new ServerPartition
                {
                    AeTitle = "ServerAE",
                    Port = 10004,
                    PatitionFolder = @"D:\fs",
                    AcceptAnyDevice = true,
                    AutoInsertDevice = true,
                    Enable = true,
                    FileSystem = fs,
                };

                context.ServerPartitions.Add(partition);

                // Supported Sop Class Uid 
                foreach (SopClass sopClass in StorageAbstractSyntaxList)
                {
                    context.SupportedSopClasses.Add(new SupportedSopClass()
                    {
                        SopClassUid = sopClass.Uid,
                        Description = sopClass.Name,
                        NonImage = true
                    });
                }

                context.SaveChanges();

            }
        }

        #endregion


    }

    
}