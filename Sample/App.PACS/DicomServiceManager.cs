using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using uDicom.Common;
using UIH.Dicom.Network.Scp;
using UIH.Dicom.PACS.Service;
using UIH.Dicom.PACS.Service.Model;

namespace App.PACS
{
    public class DicomServiceManager : ThreadedService
    {
        #region Private Members

        private readonly List<DicomScp<DicomScpContext>> _listenerList = new List<DicomScp<DicomScpContext>>();
        private IList<IServerPartition> _partitions;
        private readonly object _syncLock = new object();
        private EventHandler<ServerPartitionChangedEventArgs> _changedEvent;

        private static ILog Log = LogManager.GetLog(typeof(DicomServiceManager).FullName);

        #endregion

        public static DicomServiceManager Instance = new DicomServiceManager("Dicom Service Manager");

        #region Private Methods

        protected DicomServiceManager(string name)
            : base(name)
        {

        }

        private void StartListeners(IServerPartition part)
        {
            var parms = new DicomScpContext(part);

            //TODO support IPV6
            var scp = new DicomScp<DicomScpContext>(parms, AssociationVerifier.Verify)
            {
                ListenPort = part.Port,
                AeTitle = part.AeTitle
            };

            if (scp.Start(IPAddress.Any))
            {
                _listenerList.Add(scp);
                Log.Info("Start listen on {0} for server destAe {1}",
                    part.Port, part.Description);
            }
            else
            {
                Log.Error("Unable to listen on {0} for server destAe {1}",
                    part.Port, part.Description);
                Log.Error("destAe {0} will not accept any DICOM associations", part.Description);
            }
        }

        #endregion

        #region Protected Methods

        protected override bool Initialize()
        {
            if (_partitions == null)
            {
                _changedEvent = delegate
                {
                    CheckPartitions();
                };

                ServerPartitionMonitor.Instance.Changed += _changedEvent;

                _partitions = new List<IServerPartition>(ServerPartitionMonitor.Instance);
                
            }

            return true;
        }

        private void CheckPartitions()
        {
            lock (_syncLock)
            {
                _partitions = new List<IServerPartition>(ServerPartitionMonitor.Instance);
                IList<DicomScp<DicomScpContext>> scpsToDelete = new List<DicomScp<DicomScpContext>>();

                foreach (var scp in _listenerList)
                {
                    bool found =
                        _partitions.Any(
                            part =>
                                part.Port == scp.ListenPort && part.AeTitle.Equals(scp.AeTitle) && part.Enable);

                    if (!found)
                    {
                        Log.Info("destAe was deleted, shutting down listener {0}:{1}",
                            scp.AeTitle, scp.ListenPort);
                        scp.Stop();
                        scpsToDelete.Add(scp);
                    }
                }

                foreach (var scp in scpsToDelete)
                {
                    _listenerList.Remove(scp);
                }

                foreach (var part in _partitions)
                {
                    if (!part.Enable)
                        continue;

                    bool found = false;
                    foreach (var scp in _listenerList)
                    {
                        if (part.Port != scp.ListenPort || !part.AeTitle.Equals(scp.AeTitle))
                            continue;

                        scp.Context.Partition = part;

                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        Log.Info("Detected destAe was added, starting listener {0}:{1}",
                            part.AeTitle, part.Port);
                        StartListeners(part);
                    }
                }

            }
        }

        protected override void Run()
        {
            foreach (var part in _partitions.Where(part => part.Enable))
            {
                StartListeners(part);
            }
        }

        protected override void Stop()
        {
            lock (_syncLock)
            {
                foreach (var scp in _listenerList)
                {
                    scp.Stop();
                }

                ServerPartitionMonitor.Instance.Changed -= _changedEvent;
            }
        }

        #endregion
    }
}
