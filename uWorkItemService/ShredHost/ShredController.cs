using System;
using System.Threading;
using uDicom.Common;

namespace uDicom.WorkItemService.ShredHost
{
    internal enum RunningState
    {
        Stopped,
        Running,
        Transition
    }

    internal class ShredController : MarshalByRefObject
    {
        public ShredController(IShred shred)
        {
            _shredObject = shred;
            Id = NextId;
            _runningState = RunningState.Stopped;
        }

        static ShredController()
        {
            _nextId = 101;
        }

        public bool Start()
        {
            lock (_lockRunningState)
            {
                if (RunningState.Running == _runningState || RunningState.Transition == _runningState)
                    return RunningState.Running == _runningState;

                _runningState = RunningState.Transition;
            }

            // cache the shred's details so that even if the shred is stopped and unloaded
            // we still have it's display name 
            _shredCacheObject = new ShredCacheObject(_shredObject.GetDisplayName(), _shredObject.GetDescription());

            // create the thread and start it
            _thread = new Thread(StartupShred)
            {
                Name = string.Format("{0}", _shredCacheObject.GetDisplayName())
            };

            _thread.Start(this);

            lock (_lockRunningState)
            {
                _runningState = RunningState.Running;
            }

            return RunningState.Running == _runningState;
        }


        public bool Stop()
        {
            lock (_lockRunningState)
            {
                if (RunningState.Stopped == _runningState || RunningState.Transition == _runningState)
                    return RunningState.Running == _runningState;

                _runningState = RunningState.Transition;
            }

            _shredObject.Stop();
            _thread.Join();
            
            _shredObject = null;
            _thread = null;

            lock (_lockRunningState)
            {
                _runningState = RunningState.Stopped;
            }

            return RunningState.Running == _runningState;
        }

        private void StartupShred(object threadData)
        {
            var shredController = threadData as ShredController;
            var wcfShred = shredController.Shred as IWcfShred;

            try
            {
                if (wcfShred != null)
                {
                    //wcfShred.SharedHttpPort = ShredHostServiceSettings.Instance.SharedHttpPort;
                    //wcfShred.SharedTcpPort = ShredHostServiceSettings.Instance.SharedTcpPort;
                    //wcfShred.ServiceAddressBase = ShredHostServiceSettings.Instance.ServiceAddressBase;

                    wcfShred.ServiceAddressBase = "";
                }

                BeginStartupTimer();

                shredController.Shred.Start();
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e, "Unexpected exception when starting up Shred {0}",
                    shredController.Shred.GetDescription());
            }
            finally
            {
                EndStartupTimer();
            }
        }

        private void BeginStartupTimer()
        {
            _startingUp = true;
            TimerCallback callback = o =>
            {
                var shred = _shredObject;
                if (_startingUp && shred != null)
                    Platform.Log(LogLevel.Warn,
                        "The shred '{0}' has not returned from its Start() method after 30 seconds; shreds should start up and return quickly and should never block until Stop() is called.",
                        shred.GetType().FullName);

                EndStartupTimer();
            };

            //Use a timer on the thread pool to check that the shred's Start method returned after a reasonable startup time.
            var thirtySeconds = TimeSpan.FromSeconds(30);
            _startupTimer = new Timer(callback, null, thirtySeconds, thirtySeconds);
        }

        private void EndStartupTimer()
        {
            try
            {
                _startingUp = false;
                _startupTimer.Dispose();
                _startupTimer = null;
            }
            catch
            {
            }
        }

        private class ShredCacheObject : IShred
        {
            public ShredCacheObject(string displayName, string description)
            {
                _displayName = displayName;
                _description = description;
            }

            #region Private fields

            private readonly string _displayName;
            private readonly string _description;

            #endregion

            #region IShred Members

            public void Start()
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public void Stop()
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public string GetDisplayName()
            {
                return _displayName;
            }

            public string GetDescription()
            {
                return _description;
            }

            #endregion
        }

        #region Private fields

        private readonly object _lockRunningState = new object();
        private RunningState _runningState;

        #endregion

        #region Properties

        private Thread _thread;
        private IShred _shredObject;
        private ShredCacheObject _shredCacheObject;
        private static int _nextId;

        private bool _startingUp;
        private Timer _startupTimer;

        protected static int NextId
        {
            get { return _nextId++; }
        }

        public int Id { get; }

        public IShred Shred
        {
            get
            {
                if (null == _shredObject)
                    return _shredCacheObject;
                return _shredObject;
            }
        }

        #endregion
    }
}