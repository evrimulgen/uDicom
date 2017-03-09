using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using uDicom.Common;
using uDicom.WorkItemService;
using uDicom.WorkItemService.Common;
using uDicom.WorkItemService.Interface;
using WorkItemPriorityEnum = uDicom.WorkItemService.Interface.WorkItemPriorityEnum;

namespace WorkItemTest.DicomSend
{
    [Export(typeof(WorkItemRequest))]
    [DataContract]
    public class DicomSendRequest : WorkItemRequest
    {
        public DicomSendRequest()
        {
            WorkItemType = "DicomSend";

            Priority = WorkItemPriorityEnum.Normal;
        }
    }

    [Export(typeof(WorkItemProgress))]
    [DataContract]
    public class DicomSendProgress : WorkItemProgress
    {
        public DicomSendProgress()
        {
            IsCancelable = false;
        }
    }

    public class DicomSendItemProcessor : BaseItemProcessor<DicomSendRequest, DicomSendProgress>
    {
        public override void Process()
        {
            for (int i = 0; i < 10; i++)
            {
                Platform.Log(LogLevel.Warn, "Processing ....");
                Thread.Sleep(1000);
            }

            Proxy.Complete();
            
        }
    }
}
