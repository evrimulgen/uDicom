#region License

// 
// Copyright (c) 2011 - 2012, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UIH.Dicom.PACS.Service.Model;

namespace App.PACS.Model
{
    public partial class Device : IDevice
    {
        public int Id { get; set; }

        public int ServerPartitionPK { get; set; }

        [MaxLength(16)]
        public string AeTitle { get; set; }

        public string Hostname { get; set; }

        public int Port { get; set; }

        public bool Enabled { get; set; }

        public string Description { get; set; }

        public bool Dhcp { get; set; }

        public bool AllowStorage { get; set; }

        public bool AllowRetrieve { get; set; }

        public bool AllowQuery { get; set; }

        public bool AllowWorkList { get; set; }

        public DateTime LastAccessTime { get; set; }

        public virtual ServerPartition ServerPartition { get; set; }
    }
}