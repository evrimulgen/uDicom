#region License

// Copyright (c) 2011 - 2014, **** Inc.
// All rights reserved.
// http://www.****.com

#endregion

using System.Data.Entity.ModelConfiguration;
using App.PACS.Model;

namespace App.PACS.Mapping
{
    public class PatientMap : EntityTypeConfiguration<Patient>
    {
         public PatientMap()
         {
             HasKey(t => t.Id);

             Property(t => t.PatientId)
                 .HasMaxLength(64);

            Property(t => t.PatientName)
                 .HasMaxLength(256);

            // Table & Column Mappings
            ToTable("Patient");
         }
    }
}