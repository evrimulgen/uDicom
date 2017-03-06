using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uDicom.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItemService
{
    /// <summary>
    /// Abstract base class for processing WorkItems.
    /// </summary>
    /// <typeparam name="TRequest">The request object for the work item.</typeparam>
    /// <typeparam name="TProgress">The progress object for the work item.</typeparam>
    public abstract class BaseItemProcessor<TRequest, TProgress> : IWorkItemProcessor
        where TProgress : WorkItemProgress, new()
        where TRequest : WorkItemRequest
    {
        #region Private Fields

        private const int MAX_DB_RETRY = 5;
        private string _name = "Work Item";
        private volatile bool _cancelPending;
        private volatile bool _stopPending;
        private readonly object _syncRoot = new object();
        #endregion

        #region Properties

        /// <summary>
        /// The progress object for the WorkItem.  Note that all updates to the progress should
        /// be done through this object, and not through the <see cref="Proxy"/> property.
        /// </summary>
        public TProgress Progress
        {
            get { return Proxy.Progress as TProgress; }
        }

        /// <summary>
        /// The request object for the WorkItem.
        /// </summary>
        public TRequest Request
        {
            get { return Proxy.Request as TRequest; }
        }

        
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public WorkItemStatusProxy Proxy { get; set; }

        protected bool CancelPending
        {
            get { return _cancelPending; }
        }

        protected bool StopPending
        {
            get { return _stopPending; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called by the base to initialize the processor.
        /// </summary>
        public virtual bool Initialize(WorkItemStatusProxy proxy)
        {
            Proxy = proxy;
            if (proxy.Progress == null)
                proxy.Progress = new TProgress();
            else if (!(proxy.Progress is TProgress))
                proxy.Progress = new TProgress();

            if (Request == null)
                throw new ApplicationException("Internal Error");

            return true;
        }

        public virtual void Cancel()
        {
            Proxy.Canceling();

            lock (_syncRoot)
                _cancelPending = true;
        }

        public virtual void Stop()
        {
            lock (_syncRoot)
                _stopPending = true;
        }


        public abstract void Process();

        public virtual void Delete()
        {
            Proxy.Delete();
        }

        /// <summary>
        /// Dispose of any native resources.
        /// </summary>
        public virtual void Dispose()
        {
        }

        #endregion
        
    }

    public class NotEnoughStorageException : Exception
    {
        public NotEnoughStorageException() : base("Additional storage space is required")
        {
        }
    }
}
