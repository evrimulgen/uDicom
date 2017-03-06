using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using uDicom.WorkItemService.Interface;

namespace WorkItemTest.DicomSend
{
    [Export(typeof(IWorkItemProcessorFactory)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class DicomSendFactory : IWorkItemProcessorFactory
    {
        public string GetWorkQueueType()
        {
            return "DicomSend";
        }

        public IWorkItemProcessor GetItemProcessor()
        {
            return new DicomSendItemProcessor();
        }
    }
}
