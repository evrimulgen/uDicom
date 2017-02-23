#region License

// Copyright (c) 2011 - 2014, **** Inc.
// All rights reserved.
// http://www.****.com

#endregion

using System.Collections.Generic;
using uWs.PACS.Model;

namespace App.PACS.Model
{
    public class FileSystem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string DirPath { get; set; }

        // public int NextFileSytemFK { get; set; }

        public int LowWatermark { get; set; }

        public int HighWatermark { get; set; }

        public string Description { get; set; }

        public virtual ICollection<File> Files { get; set; }

        public virtual ICollection<ServerPartition> ServerPartitions { get; set; } 

        //public virtual FileSystem NextFileSystem { get; set; }

        //public virtual ICollection<FileSystem> PriviouseFileSystems { get; set; } 
    }
}