using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItem.Archive.Import
{
    public class ImportFactory
    {
        public string GetWorkQueueType()
        {
            return ImportFilesRequest.WorkItemTypeString;
        }

        public IWorkItemProcessor GetItemProcessor()
        {
            return new ImportItemProcessor();
        }
    }
}