#region License

// Copyright (c) 2011 - 2014, **** Inc.
// All rights reserved.
// http://www.****.com

#endregion

﻿namespace uWs.PACS.Model
{
    public class SupportedSopClass
    {
        public int Id { get; set; }

        public string SopClassUid { get; set; }

        public string Description { get; set; }

        public bool NonImage { get; set; }
    }
}