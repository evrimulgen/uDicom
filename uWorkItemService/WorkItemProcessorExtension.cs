using System.Collections.Generic;
using uDicom.WorkItemService.ShredHost;

namespace uDicom.WorkItemService
{
    // Note, although this is specified as a Shred, we don't actual use it as a Shred
    // due to wanting it to be in the same app domain as the WorkItemActivityMonitorService
    internal class WorkItemProcessorExtension : QueueProcessorShred
    {
        public override string GetDisplayName()
        {
            return "Work Item Processor";
        }

        public override string GetDescription()
        {
            return "Hosts the WorkItem Processor Service";
        }

        protected override IList<QueueProcessor> GetProcessors()
        {
            WorkItemProcessor.CreateProcessor(WorkItemServiceSettings.Default.StatThreadCount, WorkItemServiceSettings.Default.NormalThreadCount, GetDisplayName());
            return new List<QueueProcessor> { WorkItemProcessor.Instance };
        }
    }
}