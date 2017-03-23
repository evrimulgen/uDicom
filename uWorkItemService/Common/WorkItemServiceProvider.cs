using System;
using System.ComponentModel.Composition;
using System.ServiceModel;
using uDicom.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItemService.Common
{
    [Export(typeof(IServiceProvider))]
    [Export(typeof(IDuplexServiceProvider))]
    internal class WorkItemServiceProvider : IServiceProvider, IDuplexServiceProvider
    {
        #region Implementation of IDuplexServiceProvider

        public object GetService(Type type, object callback)
        {
            Platform.CheckForNullReference(type, "type");
            if (type != typeof(IWorkItemActivityMonitorService))
                return null;

            Platform.CheckExpectedType(callback, typeof(IWorkItemActivityCallback));

            // 将binding的协议地址写死在代码中，这样就不需要配置文件了，缺点就是代码和服务端紧耦合
            // 优点，也不是优点，就是不用考虑B/R 那些烦人的东西了
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            EndpointAddress address = new EndpointAddress("net.pipe://localhost/DICOMViewer/WorkItemService");


            var client = new WorkItemActivityMonitorServiceClient(new InstanceContext(callback),
                binding, address);
            if (client.State != CommunicationState.Opened)
                client.Open();

            return client;
        }

        #endregion

        #region IServiceProvider Members

        public object GetService(Type serviceType)
        {
            Platform.CheckForNullReference(serviceType, "serviceType");
            if (serviceType == typeof(IWorkItemService))
            {
                // 将binding的协议地址写死在代码中，这样就不需要配置文件了
                NetNamedPipeBinding binding = new NetNamedPipeBinding();
                EndpointAddress address = new EndpointAddress("net.pipe://localhost/DICOMViewer/WorkItemService");

                var client = new WorkItemServiceClient(binding, address);
                if (client.State != CommunicationState.Opened)
                    client.Open();

                return client;
            }

            //Someone could be requesting a single-use instance of the activity monitor, I suppose.
            return GetService(serviceType, WorkItemActivityCallback.Nil);
        }

        #endregion
    }
}
