using System;
using uDicom.Common;
using uDicom.WorkItemService.Common;
using uDicom.WorkItemService.Interface;
using uDicom.WorkItemService.ShredHost;

namespace uDicom.WorkItemService
{
    public class WorkItemServiceExtension : WcfShred
    {
        private const string _workItemServiceEndpointName = "WorkItemService";
        private const string _workItemActivityMonitorServiceEndpointName = "WorkItemActivityMonitor";
        private bool _workItemServiceWCFInitialized;
        private bool _workItemActivityMonitorServiceWCFInitialized;

        private const string WorkItemName = "Work Item";
        private const string WorkItemMonitor = "Work Item Activity Monitor";

        private readonly WorkItemProcessorExtension _processor;

        public WorkItemServiceExtension()
        {
            _workItemServiceWCFInitialized = false;
            _workItemActivityMonitorServiceWCFInitialized = false;
            _processor = new WorkItemProcessorExtension();
        }

        public override void Start()
        {
            try
            {
                WorkItemService.Instance.Start();

                string message = String.Format("The {0} service has started successfully.", "Work Item");
                Platform.Log(LogLevel.Info, message);
                Console.WriteLine(message);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
                Console.WriteLine(String.Format("The {0} service has started failed.", "Work Item"));
                return;
            }

            try
            {
                StartNetPipeHost<WorkItemServiceType, IWorkItemService>(_workItemServiceEndpointName,
                    WorkItemName);
                _workItemServiceWCFInitialized = true;
                string message = String.Format("The {0} service has started successfully.", "Work Item");
                Platform.Log(LogLevel.Info, message);
                Console.WriteLine(message);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
                Console.WriteLine(String.Format("The {0} service has started failed.", "Work Item"));
            }

            try
            {
                StartNetPipeHost<WorkItemActivityMonitorServiceType, IWorkItemActivityMonitorService>(
                    _workItemActivityMonitorServiceEndpointName,
                    "WorkItemMonitor");
                _workItemActivityMonitorServiceWCFInitialized = true;
                string message = String.Format("The {0} service has started successfully.", "Work Item");
                Platform.Log(LogLevel.Info, message);
                Console.WriteLine(message);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
                Console.WriteLine(String.Format("The {0} service has started failed.", "Work Item"));
            }

            _processor.Start();
        }

        public override void Stop()
        {
            // Stop the processor first, so status updates go out.
            _processor.Stop();

            if (_workItemActivityMonitorServiceWCFInitialized)
            {
                try
                {
                    StopHost(_workItemActivityMonitorServiceEndpointName);
                    Platform.Log(LogLevel.Info, String.Format("The {0} WCF service has stopped successfully.",
                        WorkItemMonitor));
                }
                catch (Exception e)
                {
                    Platform.Log(LogLevel.Error, e);
                }
            }

            if (_workItemServiceWCFInitialized)
            {
                try
                {
                    StopHost(_workItemServiceEndpointName);
                    Platform.Log(LogLevel.Info, String.Format("The {0} WCF service has stopped successfully.",
                        WorkItemName));
                }
                catch (Exception e)
                {
                    Platform.Log(LogLevel.Error, e);
                }
            }

            try
            {
                WorkItemService.Instance.Stop();
                Platform.Log(LogLevel.Info, String.Format("The {0} WCF service has stopped successfully.", 
                    WorkItemName));
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
            }
        }

        public override string GetDisplayName()
        {
            return "Work Item";
        }

        public override string GetDescription()
        {
            return "Work Item";
        }
    }
}