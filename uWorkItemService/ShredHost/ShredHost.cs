using System;
using System.Reflection;
using uDicom.Common;

namespace uDicom.WorkItemService.ShredHost
{
    public static class ShredHost
    {
        #region Private Members
        private static ShredController _shredController;
        private static RunningState _runningState;
        private static readonly object _lockObject = new object();
        #endregion

        /// <summary>
        /// Starts the ShredHost routine.
        /// </summary>
        /// <returns>true - if the ShredHost is currently running, false - if ShredHost is stopped.</returns>
        public static bool Start()
        {
            // install the unhandled exception event handler
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += MyUnhandledExceptionEventHandler;

            lock (_lockObject)
            {
                if (RunningState.Running == _runningState || RunningState.Transition == _runningState)
                    return (RunningState.Running == _runningState);

                _runningState = RunningState.Transition;
            }

            Platform.Log(LogLevel.Info, "Starting up in AppDomain [" + AppDomain.CurrentDomain.FriendlyName + "]");

            
            // Startup WorkItem Shred 

            StartShreds();
            
            lock (_lockObject)
            {
                _runningState = RunningState.Running;
            }

            return (RunningState.Running == _runningState);
        }

        /// <summary>
        /// Stops the running ShredHost.
        /// </summary>
        /// <returns>true - if the ShredHost is running, false - if the ShredHost is stopped.</returns>
        public static bool Stop()
        {
            lock (_lockObject)
            {
                if (RunningState.Stopped == _runningState || RunningState.Transition == _runningState)
                    return (RunningState.Running == _runningState);

                _runningState = RunningState.Transition;
            }

            // correct sequence should be to stop the WCF host so that we don't
            // receive any more incoming requests
            Platform.Log(LogLevel.Info, "ShredHost stop request received");

            StopShreds();
            Platform.Log(LogLevel.Info, "Completing ShredHost stop.");

            lock (_lockObject)
            {
                _runningState = RunningState.Stopped;
            }

            return (RunningState.Running == _runningState);
        }

        public static bool IsShredHostRunning
        {
            get
            {
                bool isRunning;
                lock (_lockObject)
                {
                    isRunning = (RunningState.Running == _runningState);
                }

                return isRunning;
            }
        }

        static ShredHost()
        {
            _runningState = RunningState.Stopped;
        }

        private static void StartShreds()
        {
            _shredController = new ShredController(new WorkItemServiceExtension());
            _shredController.Start();
        }

        private static void StopShreds()
        {
            Exception savedException = null;
            
            try
            {
                string displayName = _shredController.Shred.GetDisplayName();
                Platform.Log(LogLevel.Info, displayName + ": Signalling stop");
                _shredController.Stop();
                Platform.Log(LogLevel.Info, displayName + ": Stopped");
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Fatal, e, "Unexepected exception stopping Shred (shred still running): {0}",
                    _shredController.Shred.GetDisplayName());
                savedException = e;
            }
           

            if (savedException != null) throw savedException;
        }

        #region Print asms in AD helper f(x)
        public static void PrintAllAssembliesInAppDomain(AppDomain ad)
        {
            Assembly[] loadedAssemblies = ad.GetAssemblies();
            Console.WriteLine(@"***** Here are the assemblies loaded in {0} *****\n",
                ad.FriendlyName);
            foreach (Assembly a in loadedAssemblies)
            {
                Console.WriteLine(@"-> Name: {0}", a.GetName().Name);
                Console.WriteLine(@"-> Version: {0}", a.GetName().Version);
            }
        }
        #endregion

        internal static void MyUnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Platform.Log(LogLevel.Fatal, e, 
                "Fatal error - unhandled exception in running Shred; ShredHost must terminate");
        }

        
    }
}