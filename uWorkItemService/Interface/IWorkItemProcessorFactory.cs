namespace uDicom.WorkItemService.Interface
{
    /// <summary>
    /// Factory for creating <see cref="IWorkItemProcessor"/> instances.
    /// </summary>
    public interface IWorkItemProcessorFactory
    {
        string GetWorkQueueType();

        IWorkItemProcessor GetItemProcessor();
    }
}
