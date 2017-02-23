#region License

// Copyright (c) 2011 - 2014, **** Inc.
// All rights reserved.
// http://www.****.com

#endregion

using System.Data.Entity.ModelConfiguration;
using App.PACS.Model;

namespace App.PACS.Mapping
{
    public class FileSystemMap : EntityTypeConfiguration<FileSystem>
    {
         public FileSystemMap()
         {
             HasKey(t => t.Id);

             ToTable("FileSystem");

             // Relationship 
             //this.HasOptional(t => t.NextFileSystem)
             //                .WithMany(t => t.PriviouseFileSystems)
             //                .HasForeignKey(t => t.NextFileSytemFK);

            
         }
    }
}