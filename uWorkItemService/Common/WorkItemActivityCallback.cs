using System.Collections.Generic;
using System.ServiceModel;

namespace uDicom.WorkItemService.Common
{
    [ServiceContract]
    [ServiceKnownType("GetKnownTypes", typeof(WorkItemRequestTypeProvider))]
    public interface IWorkItemActivityCallback
    {
        [OperationContract(IsOneWay = true)]
        void StudiesCleared();

        [OperationContract(IsOneWay = true)]
        void WorkItemsChanged(WorkItemsChangedEventType eventType, List<WorkItemData> workItems);
    }

    public abstract class WorkItemActivityCallback : IWorkItemActivityCallback
    {
        private class NilCallback : WorkItemActivityCallback
        {
            public override void WorkItemsChanged(WorkItemsChangedEventType eventType, List<WorkItemData> workItems)
            {
            }

            public override void StudiesCleared()
            {
            }
        }

        public static readonly IWorkItemActivityCallback Nil = new NilCallback();

        #region IWorkItemActivityCallback Members

        public abstract void WorkItemsChanged(WorkItemsChangedEventType eventType, List<WorkItemData> workItems);

        public abstract void StudiesCleared();

        #endregion
    }
}
