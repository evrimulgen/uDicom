#region License

// Copyright (c) 2011 - 2014, **** Inc.
// All rights reserved.
// http://www.****.com

#endregion

using System.Data.Entity.ModelConfiguration;
using uWs.PACS.Model;

namespace App.PACS.Mapping
{
    public class SupportedSopClassMap : EntityTypeConfiguration<SupportedSopClass>
    {
         public SupportedSopClassMap()
         {
             HasKey(t => t.Id);

             Property(t => t.SopClassUid)
                 .HasMaxLength(64);

             ToTable("SupportedSopClass");
         }
    }
}