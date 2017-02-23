#region License

// Copyright (c) 2011 - 2014, **** Inc.
// All rights reserved.
// http://www.****.com

#endregion

using System.Data.Entity.ModelConfiguration;
using App.PACS.Model;

namespace App.PACS.Mapping
{
    public class DeviceMap : EntityTypeConfiguration<Device>
    {
         // Primary Key 
        public DeviceMap()
        {
            HasKey(t => t.Id);

            // Properties 

            // Table & Columes 
            ToTable("Device");

            // RelationShips 
            this.HasRequired(t => t.ServerPartition)
                .WithMany(s => s.Devices)
                .HasForeignKey(t => t.ServerPartitionPK);
        }
    }
}