using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using uDicom.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItemService.Common
{
    public interface IDuplexServiceProvider
    {
        object GetService(Type type, object callback);
    }

    internal class WorkItemServiceProvider : IServiceProvider, IDuplexServiceProvider
    {
        #region Implementation of IDuplexServiceProvider

        public object GetService(Type type, object callback)
        {
            Platform.CheckForNullReference(type, "type");
            if (type != typeof(IWorkItemActivityMonitorService))
                return null;

            Platform.CheckExpectedType(callback, typeof(IWorkItemActivityCallback));

            var client = new WorkItemActivityMonitorServiceClient(new InstanceContext(callback));
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
                var client = new WorkItemServiceClient();
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
