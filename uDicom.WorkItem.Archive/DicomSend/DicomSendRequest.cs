using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItem.Archive.DicomSend
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
}
