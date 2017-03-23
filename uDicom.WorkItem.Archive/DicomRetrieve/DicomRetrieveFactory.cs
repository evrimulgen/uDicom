using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItem.Archive.DicomRetrieve
{
    public class DicomRetrieveFactory : IWorkItemProcessorFactory
    {
        public string GetWorkQueueType()
        {
            return DicomRetrieveRequest.WorkItemTypeString;
        }

        public IWorkItemProcessor GetItemProcessor()
        {
            return new DicomRetrieveItemProcessor();
        }
    }
}