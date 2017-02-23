using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using App.PACS.Model;
using UIH.Dicom.Network;
using UIH.Dicom.PACS.Service.Interface;
using UIH.Dicom.PACS.Service.Model;

namespace App.PACS.Logic
{
    [Export(typeof(IDeviceManager))]
    public class DeviceManager : IDeviceManager
    {
        public IDevice LookupDevice(IServerPartition partition, AssociationParameters association, out bool isNew)
        {
            isNew = false;

            Device device = null;

            using (var ctx = new PacsContext())
            {
                var part = (from p in ctx.ServerPartitions select p).FirstOrDefault();
                if (part != null)
                {
                    device = part.Devices.FirstOrDefault(d => d.AeTitle.Equals(association.CallingAE));
                }

                if (device == null)
                {
                    if (!partition.AcceptAnyDevice)
                    {
                        return null;
                    }

                    if (partition.AutoInsertDevice)
                    {
                        device = new Device()
                        {
                            AeTitle = association.CallingAE,
                            Enabled = true,
                            Description = string.Format("AE: {0}", association.CallingAE),
                            //TODO
                            Port = 104,
                            AllowQuery = true,
                            AllowRetrieve = true,
                            AllowStorage = true,
                            ServerPartitionPK = part.Id,
                            LastAccessTime = DateTime.Now
                        };

                        ctx.Devices.Add(device);
                        ctx.SaveChanges();

                        isNew = true;
                    }
                }

                if (device != null)
                {
                    if (device.Dhcp && !association.RemoteEndPoint.Address.ToString().Equals(device.Hostname))
                    {
                        device.Hostname = association.RemoteEndPoint.Address.ToString();

                        device.LastAccessTime = DateTime.Now;
                        ctx.SaveChanges();
                    }
                    else if (!isNew)
                    {
                        device.LastAccessTime = DateTime.Now;
                        ctx.SaveChanges();
                    }
                }
            }

            return device;
        }
    }
}
