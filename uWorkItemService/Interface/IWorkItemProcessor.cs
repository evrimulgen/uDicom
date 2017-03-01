using System;

namespace uDicom.WorkItemService.Interface
{
    /// <summary>
    /// Interface for processors of WorkItem items
    /// </summary>
    public interface IWorkItemProcessor : IDisposable
    {
        #region Properties

        string Name { get; }

        WorkItemStatusProxy Proxy { get; set; }

        #endregion

        #region Methods

        bool Initialize(WorkItemStatusProxy proxy);

        void Process();

        void Cancel();

        void Stop();

        void Delete();

        #endregion
    }
}
