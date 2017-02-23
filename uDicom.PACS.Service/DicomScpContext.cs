#region License
// 
// Copyright (c) 2011 - 2012, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com
#endregion

using UIH.Dicom.PACS.Service.Model;

namespace UIH.Dicom.PACS.Service
{
    public class DicomScpContext
    {
        #region Constructors
        public DicomScpContext(IServerPartition partition)
        {
            Partition = partition;
        }
        #endregion

        #region Properties

        public IServerPartition Partition { get; set; }

        #endregion
    }
}