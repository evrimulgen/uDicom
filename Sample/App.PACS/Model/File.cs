#region License

// Copyright (c) 2011 - 2014, **** Inc.
// All rights reserved.
// http://www.****.com

#endregion

using System;
using System.ComponentModel.DataAnnotations;

namespace App.PACS.Model
{
    public class File
    {
        [Key]
        public int Id { get; set; }

        public int InstanceFk { get; set; }

        public int FileSystemFk { get; set; }

        public string FilePath { get; set; }

        public string Md5 { get; set; }

        public int FileSize { get; set; }

        public DateTime CreateTime { get; set; }

        public virtual Instance Instance { get; set; }

        public virtual FileSystem FileSystem { get; set; }
    }
}