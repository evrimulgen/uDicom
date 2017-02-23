#region License

// 
// Copyright (c) 2011 - 2012, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using System.Collections.Generic;
using UIH.Dicom.PACS.Service.Model;

namespace App.PACS.Model
{
    public class ServerPartition : IServerPartition
    {
        public ServerPartition()
        {
            Devices = new List<Device>();
        }

        public int Id { get; set; }

        public int FileSystemFk { get; set; }

        public string AeTitle { get; set; }

        public bool Enable { get; set; }

        public string Hostname { get; set; }

        public int Port { get; set; }

        public string Description { get; set; }

        public string StationName { get; set; }

        public string Institution { get; set; }

        public string Department { get; set; }

        public string WadoUrl { get; set; }

        public string PatitionFolder { get; set; }

        public bool AcceptAnyDevice { get; set; }

        public bool AutoInsertDevice { get; set; }

        public virtual ICollection<Device> Devices { get; set; }

        public virtual FileSystem FileSystem { get; set; }
    }
}