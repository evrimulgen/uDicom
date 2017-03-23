using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using uDicom.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItemService
{
    [Serializable]
    internal class WorkItemServiceException : Exception
    {
        public WorkItemServiceException(string message)
            : base(message)
        {
        }

        protected WorkItemServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WorkItemServiceType : IWorkItemService
    {
        public WorkItemInsertResponse Insert(WorkItemInsertRequest request)
        {
            try
            {
                return WorkItemService.Instance.Insert(request);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
                var message = "Failed to process WorkItem Insert request.";
                var exceptionMessage = String.Format("{0}\nDetail:{1}", message, e.Message);
                throw new WorkItemServiceException(exceptionMessage);
            }
        }

        public WorkItemUpdateResponse Update(WorkItemUpdateRequest request)
        {
            try
            {
                return WorkItemService.Instance.Update(request);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
                var message = "Failed to process WorkItem Update request.";
                var exceptionMessage = String.Format("{0}\nDetail:{1}", message, e.Message);
                throw new WorkItemServiceException(exceptionMessage);
            }
        }

        public WorkItemQueryResponse Query(WorkItemQueryRequest request)
        {
            try
            {
                return WorkItemService.Instance.Query(request);
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e);
                var message = "Failed to process WorkItem Query request.";
                var exceptionMessage = String.Format("{0}\nDetail:{1}", message, e.Message);
                throw new WorkItemServiceException(exceptionMessage);
            }
        }
    }
}
