#region License

// Copyright (c) 2011 - 2014, **** Inc.
// All rights reserved.
// http://www.****.com

#endregion

using System.Data.Entity.ModelConfiguration;
using App.PACS.Model;

namespace App.PACS.Mapping
{
    public class ServerPartitionMap : EntityTypeConfiguration<ServerPartition>
    {
         public ServerPartitionMap()
         {
             // Primary Key 
             HasKey(t => t.Id);

             // Columns 

             // Tables 
             ToTable("ServerPartition");

             // relationship
             this.HasRequired(t => t.FileSystem)
                 .WithMany(t => t.ServerPartitions)
                 .HasForeignKey(t => t.FileSystemFk);
         }
    }
}