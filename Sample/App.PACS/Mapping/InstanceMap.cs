#region License

// Copyright (c) 2011 - 2014, **** Inc.
// All rights reserved.
// http://www.****.com

#endregion

using System.Data.Entity.ModelConfiguration;
using App.PACS.Model;

namespace App.PACS.Mapping
{
    public class InstanceMap : EntityTypeConfiguration<Instance>
    {
         public InstanceMap()
         {
             // Primary Key
             this.HasKey(t => t.Id);

             // Properties 
             this.Property(t => t.SopInstanceUid)
                 .HasMaxLength(64)
                 .IsRequired();

             // Table & Columns
             ToTable("Instance");

             // RelationShip 
             this.HasRequired(t => t.Series)
                 .WithMany(s => s.Instances)
                 .HasForeignKey(t => t.SeriesForeignKey);
         }
    }
}