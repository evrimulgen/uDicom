#region License

// 
// Copyright (c) 2011 - 2012, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using UIH.Dicom.Common.Utilities;
using UIH.Dicom.PACS.Service.Model;
using Timer = System.Threading.Timer;

namespace UIH.Dicom.PACS.Service
{
    /// <summary>
    /// Event arguments for partition monitor
    /// </summary>
    public class ServerPartitionChangedEventArgs : EventArgs
    {
        private readonly ServerPartitionMonitor _monitor;

        public ServerPartitionChangedEventArgs(ServerPartitionMonitor theMonitor)
        {
            _monitor = theMonitor;
        }

        public ServerPartitionMonitor Monitor
        {
            get { return _monitor; }
        }
    }

    public class ServerPartitionMonitor : IEnumerable<ServerPartition>, IDisposable
    {
        #region Private Member
        private readonly object _partitionsLock = new Object();
        private Dictionary<string, ServerPartition> _partitions = new Dictionary<string, ServerPartition>();
        private EventHandler<ServerPartitionChangedEventArgs> _changedListener;
        private readonly Timer _timer;
        #endregion

        #region Static Properties
        public static ServerPartitionMonitor Instance = new ServerPartitionMonitor();
        #endregion

        public event EventHandler<ServerPartitionChangedEventArgs> Changed
        {
            add { _changedListener += value; }
            remove { if (_changedListener != null) _changedListener -= value; }
        }

        #region Private Constructors
        /// <summary>
        /// ***** internal use only ****
        /// </summary>
        private ServerPartitionMonitor()
        {
            LoadPartitions();

            //_timer = new Timer(SynchDB, null, TimeSpan.FromSeconds(Settings.Default.DbChangeDelaySeconds), TimeSpan.FromSeconds(Settings.Default.DbChangeDelaySeconds));
        }
        #endregion

        #region Private Methods
        private void LoadPartitions()
        {
            bool changed = false;
            lock (_partitionsLock)
            {
                var templist = new Dictionary<string, ServerPartition>();

                //using (var ctx = new PacsContext())
                //{
                //    var partitions = (from p in ctx.ServerPartitions where (true) select p).ToList();
                //    foreach (var serverPartition in partitions)
                //    {
                //        if(IsChanged(serverPartition))
                //        {
                //            changed = true;
                //        }

                //        templist.Add(serverPartition.AeTitle, serverPartition);
                //    }
                //}

                _partitions = templist;

                if(changed && _changedListener != null)
                {
                    EventsHelper.Fire(_changedListener, this, new ServerPartitionChangedEventArgs(this));
                }
            }
        }

        private bool IsChanged(ServerPartition partition)
        {
            // new partitions 
            if (!_partitions.ContainsKey(partition.AeTitle))
                return true;

            ServerPartition origalPartition = _partitions[partition.AeTitle];
            if(origalPartition.AcceptAnyDevice != partition.AcceptAnyDevice)
            {
                return true;
            }

            if(origalPartition.AutoInsertDevice != partition.AutoInsertDevice)
            {
                return true;
            }

            if(origalPartition.Description != partition.Description)
            {
                return true;
            }

            if(origalPartition.Enable != partition.Enable)
            {
                return true;
            }

            if(origalPartition.Port != partition.Port)
            {
                return true;
            }

            return false;
        }
        #endregion

        public IEnumerator<ServerPartition> GetEnumerator()
        {
            return _partitions.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}