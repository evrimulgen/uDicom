#region License

// 
// Copyright (c) 2011 - 2012, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using System;
using System.Threading;
using uDicom.Common;

namespace UIH.Dicom.PACS.Service
{
    public abstract class ThreadedService
    {
        #region Private Member

        private readonly string _name;
        private Thread _theThread;
        private int _threadRetryDelay = 60000;

        #endregion

        #region Public Fields

        /// <summary>
        /// Flag set to true if stop has been requested.
        /// </summary>
        public bool StopFlag { get; private set; }

        /// <summary>
        /// Reset event to signal when stopping the service thread.
        /// </summary>
        public ManualResetEvent ThreadStop { get; private set; }

        /// <summary>
        /// The name of the thread.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Retry delay (in ms) before retrying after a failure
        /// </summary>
        public int ThreadRetryDelay
        {
            get { return _threadRetryDelay; }
            set { _threadRetryDelay = value; }
        }

        #endregion

        #region Constructor

        public ThreadedService(string name)
        {
            _name = name;
        }

        #endregion

        #region Protected Abstract Methods

        protected abstract bool Initialize();
        protected abstract void Run();
        protected abstract void Stop();

        #endregion

        #region Public Method 

        public void StartService()
        {
            if (_theThread != null)
                return;

            ThreadStop = new ManualResetEvent(false);
            _theThread = new Thread(delegate()
                                        {
                                            bool init = false;
                                            while (!init)
                                            {
                                                try
                                                {
                                                    init = Initialize();
                                                }
                                                catch (Exception ex)
                                                {
                                                    Platform.Log(LogLevel.Error, ex, "Unexpected exception initializing {0} service",
                                                            Name);
                                                }

                                                if (!init)
                                                {
                                                    ThreadStop.WaitOne(ThreadRetryDelay, false);
                                                    ThreadStop.Reset();

                                                    if (StopFlag)
                                                        return;
                                                }
                                            }

                                            try
                                            {
                                                Run();
                                            }
                                            catch (Exception e)
                                            {
                                                Platform.Log(LogLevel.Error, e, "Unexpected exception running service {0}",
                                                    Name);
                                            }
                                        });
            _theThread.Name = string.Format("{0}:{1}", Name, _theThread.ManagedThreadId);
            _theThread.Start();
        }

        public void StopService()
        {
            StopFlag = true;
            Stop();

            if (_theThread.IsAlive)
            {
                ThreadStop.Set();
                _theThread.Join();
                _theThread = null;
            }
        }

        #endregion
    }
}