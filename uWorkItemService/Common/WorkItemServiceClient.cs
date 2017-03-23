using System.ServiceModel;
using System.ServiceModel.Channels;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItemService.Common
{
    internal class WorkItemActivityMonitorServiceClient : DuplexClientBase<IWorkItemActivityMonitorService>,
        IWorkItemActivityMonitorService
    {
        public WorkItemActivityMonitorServiceClient(InstanceContext callbackInstance,
            Binding binding, EndpointAddress address)
            : base(callbackInstance, binding, address)
        {
        }

        public WorkItemSubscribeResponse Subscribe(WorkItemSubscribeRequest request)
        {
            return Channel.Subscribe(request);
        }

        public WorkItemUnsubscribeResponse Unsubscribe(WorkItemUnsubscribeRequest request)
        {
            return Channel.Unsubscribe(request);
        }

        public void Refresh(WorkItemRefreshRequest request)
        {
            Channel.Refresh(request);
        }

        public WorkItemPublishResponse Publish(WorkItemPublishRequest request)
        {
            return Channel.Publish(request);
        }
    }

    internal class WorkItemServiceClient : ClientBase<IWorkItemService>, IWorkItemService
    {
        public WorkItemServiceClient(Binding binding, EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
            
        }

        public WorkItemInsertResponse Insert(WorkItemInsertRequest request)
        {
            return Channel.Insert(request);
        }

        public WorkItemUpdateResponse Update(WorkItemUpdateRequest request)
        {
            return Channel.Update(request);
        }

        public WorkItemQueryResponse Query(WorkItemQueryRequest request)
        {
            return Channel.Query(request);
        }
    }
}
