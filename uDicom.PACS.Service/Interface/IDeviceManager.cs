#region License

// 
// Copyright (c) 2011 - 2012, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using UIH.Dicom.Network;
using UIH.Dicom.PACS.Service.Model;

namespace UIH.Dicom.PACS.Service.Interface
{
    public interface IDeviceManager
    {
        IDevice LookupDevice(IServerPartition partition, AssociationParameters association, out bool isNew);

        IDevice LookupDevice(IServerPartition partition, string aet);
    }
}