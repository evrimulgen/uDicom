using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIH.Dicom.Network.Scu;
using UIH.Dicom.PACS.Service.Model;

namespace UIH.Dicom.PACS.Service
{
    public class PacsStorageScu : StorageScu
    {
        #region Private Fileds 

        private readonly IDevice _remoteDevice;

        #endregion

        #region Constructor

        public PacsStorageScu(IServerPartition partition, IDevice remoteDevice)
             : base(partition.AeTitle, remoteDevice.AeTitle, remoteDevice.Hostname, remoteDevice.Port)
        {
            _remoteDevice = remoteDevice;
        }

        public PacsStorageScu(IServerPartition partition, IDevice remoteDevice, string moveOriginatorAe,
                              ushort moveOrginatorMessageId)
            : base(
                partition.AeTitle, remoteDevice.AeTitle, remoteDevice.Hostname, remoteDevice.Port,
                moveOriginatorAe, moveOrginatorMessageId)
        {
            _remoteDevice = remoteDevice;
        }

        #endregion

        #region Public Method 

        #endregion

    }
}
