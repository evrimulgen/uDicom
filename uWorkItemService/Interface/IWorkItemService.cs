using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using uDicom.WorkItemService.Common;

namespace uDicom.WorkItemService.Interface
{
    public static class ImageViewerNamespace
    {
        public const string Value = "http://www.clearcanvas.ca/imageviewer";
    }

    public static class ImageViewerWorkItemNamespace
    {
        public const string Value = ImageViewerNamespace.Value + "/workitem";
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemInsertRequest
    {
        [DataMember]
        public WorkItemRequest Request { get; set; }

        [DataMember]
        public WorkItemProgress Progress { get; set; }
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemInsertResponse
    {
        [DataMember]
        public WorkItemData Item { get; set; }
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemUpdateRequest
    {
        [DataMember(IsRequired = true)]
        public long Identifier { get; set; }

        [DataMember]
        public WorkItemPriorityEnum? Priority { get; set; }

        [DataMember]
        public WorkItemStatusEnum? Status { get; set; }

        [DataMember]
        public DateTime? ProcessTime { get; set; }

        [DataMember]
        public DateTime? ExpirationTime { get; set; }

        [DataMember]
        public bool? Cancel { get; set; }

        [DataMember]
        public bool? Pause { get; set; }

        [DataMember]
        public bool? Delete { get; set; }

    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemUpdateResponse 
    {
        [DataMember]
        public WorkItemData Item { get; set; }
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemQueryRequest 
    {
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public WorkItemStatusEnum? Status { get; set; }

        [DataMember]
        public string StudyInstanceUid { get; set; }

        [DataMember]
        public long? Identifier { get; set; }
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemQueryResponse 
    {
        [DataMember]
        public IEnumerable<WorkItemData> Items { get; set; }
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemSubscribeRequest : DataContractBase
    {
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemSubscribeResponse : DataContractBase
    {
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemUnsubscribeRequest : DataContractBase
    {
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemUnsubscribeResponse : DataContractBase
    {
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemRefreshRequest : DataContractBase
    {
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemRefreshResponse : DataContractBase
    {
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemPublishRequest : DataContractBase
    {
        // TODO (CR Jun 2012): We can only publish changes to single items, but the callback accepts an array?
        [DataMember]
        public WorkItemData Item { get; set; }
    }

    [DataContract(Namespace = ImageViewerWorkItemNamespace.Value)]
    public class WorkItemPublishResponse : DataContractBase
    {
    }

    [ServiceContract(SessionMode = SessionMode.Required,
                        CallbackContract = typeof(IWorkItemActivityCallback),
                        ConfigurationName = "IWorkItemActivityMonitorService",
                        Namespace = ImageViewerWorkItemNamespace.Value)]
    [ServiceKnownType("GetKnownTypes", typeof(WorkItemRequestTypeProvider))]
    public interface IWorkItemActivityMonitorService
    {
        [OperationContract]
        WorkItemSubscribeResponse Subscribe(WorkItemSubscribeRequest request);

        [OperationContract]
        WorkItemUnsubscribeResponse Unsubscribe(WorkItemUnsubscribeRequest request);

        [OperationContract(IsOneWay = true)]
        void Refresh(WorkItemRefreshRequest request);

        // TODO (CR Jun 2012): this should be renamed "PublishWorkedItemChanged". Still wish we could get rid of it.
        [OperationContract]
        WorkItemPublishResponse Publish(WorkItemPublishRequest request);
    }

    /// <summary>
    /// Service for the creation, manipulation, and monitoring of WorkItems.
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Required,
        ConfigurationName = "IWorkItemService",
        Namespace = ImageViewerWorkItemNamespace.Value)]
    [ServiceKnownType("GetKnownTypes", typeof(WorkItemRequestTypeProvider))]
    public interface IWorkItemService
    {
        [OperationContract]
        WorkItemInsertResponse Insert(WorkItemInsertRequest request);

        [OperationContract]
        WorkItemUpdateResponse Update(WorkItemUpdateRequest request);

        [OperationContract]
        WorkItemQueryResponse Query(WorkItemQueryRequest request);
    }
}
