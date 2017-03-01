namespace uDicom.WorkItemService
{
    /// <summary>
	/// Abstract base class for queue processor classes.
	/// </summary>
	/// <remarks>
	/// </remarks>
	public abstract class QueueProcessor
    {
        private volatile bool _stopRequested;

        /// <summary>
        /// Runs the processor.
        /// </summary>
        /// <remarks>
        /// This method is expected to block indefinitely until the <see cref="RequestStop"/>
        /// method is called, at which point it should exit in a timely manner.
        /// </remarks>
        public void Run()
        {
            RunCore();
        }

        /// <summary>
        /// Requests the task to exit gracefully.
        /// </summary>
        /// <remarks>
        /// This method will be called on a thread other than the thread on which the task is executing.
        /// This method should return quickly - it should not block.  A typical implementation simply
        /// sets a flag that causes the <see cref="Run"/> method to terminate.
        /// must be implemented in such a way as to heed
        /// a request to stop within a timely manner.
        /// </remarks>
        public virtual void RequestStop()
        {
            _stopRequested = true;
        }

        /// <summary>
        /// A name for the queue processor.
        /// </summary>
        /// <remarks>
        /// The thread in the corresponding to this QueueProcessor is given its Name.
        /// </remarks>
        public virtual string Name
        {
            get { return null; }
        }

        /// <summary>
        /// Implements the main logic of the processor.
        /// </summary>
        /// <remarks>
        /// Implementation is expected to run indefinitely but must poll the
        /// <see cref="StopRequested"/> property and exit in a timely manner when true.
        /// </remarks>
        protected abstract void RunCore();

        /// <summary>
        /// Gets a value indicating whether this processor has been requested to terminate.
        /// </summary>
        protected bool StopRequested
        {
            get { return _stopRequested; }
        }

    }
}
