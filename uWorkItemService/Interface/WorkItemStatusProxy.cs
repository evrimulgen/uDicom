using System;
using uDicom.Common;
using uDicom.WorkItemService.Common;

namespace uDicom.WorkItemService.Interface
{
    /// <summary>
    /// Enum telling if a work queue entry had a fatal or nonfatal error.
    /// </summary>
    public enum WorkItemFailureType
    {
        /// <summary>
        /// Fatal errors cause the WorkItem to fail immediately.
        /// </summary>
        Fatal,
        /// <summary>
        /// Non-fatal errors cause a retry of the WorkItem
        /// </summary>
        NonFatal
    }

    public abstract class WorkItemStatusProxy
    {
        #region Private

        private const int MaxRetryCount = 10;
        private const int PostponeSecond = 30;

        private WorkItem _workItem; 

        #endregion

        #region Public Properties

        // Hard-coded log level for proxy
        public LogLevel LogLevel = LogLevel.Debug;

        public WorkItem Item
        {
            get { return _workItem; }

            set
            {
                _workItem = value;
                Progress = _workItem.Progress;
                Request = _workItem.Request;
            }
        }
        public WorkItemProgress Progress { get; set; }
        public WorkItemRequest Request { get; set; }

        #endregion

        #region Public class 

        /// <summary>
        /// Simple routine for failing a <see cref="WorkItem"/> and save a reason.
        /// </summary>
        /// <param name="reason">A non-localized reason for the failure.</param>
        /// <param name="failureType">The type of failure.</param>
        public void Fail(string reason, WorkItemFailureType failureType)
        {
            Progress.StatusDetails = reason;
            Fail(failureType);
        }

        /// <summary>
        /// Simple routine for failing a <see cref="WorkItem"/> and save a reason.
        /// </summary>
        /// <param name="reason">A non-localized reason for the failure.</param>
        /// <param name="failureType">The type of failure.</param>
        /// <param name="scheduledTime">The time to reschedule the WorkItem if it isn't a fatal error. </param>
        public void Fail(string reason, WorkItemFailureType failureType, DateTime scheduledTime)
        {
            Progress.StatusDetails = reason;
            Fail(failureType, scheduledTime, 10);
        }

        /// <summary>
        /// Simple routine for failing a <see cref="WorkItem"/> and save a reason.
        /// </summary>
        /// <param name="reason">A non-localized reason for the failure.</param>
        /// <param name="failureType">The type of failure.</param>
        /// <param name="scheduledTime">The time to reschedule the WorkItem if it isn't a fatal error. </param>
        /// <param name="retryCount"> </param>
        public void Fail(string reason, WorkItemFailureType failureType, DateTime scheduledTime, int retryCount)
        {
            Progress.StatusDetails = reason;
            Fail(failureType, scheduledTime, retryCount);
        }

        /// <summary>
        /// SImple routine for failing a <see cref="WorkItem"/>
        /// </summary>
        /// <param name="failureType"></param>
        public void Fail(WorkItemFailureType failureType)
        {
            Fail(failureType, System.DateTime.Now.AddSeconds(PostponeSecond), MaxRetryCount);
        }

        /// <summary>
        /// Simple routine for failing a <see cref="WorkItem"/> and rescheduling it at a specified time.
        /// </summary>
        /// <param name="failureType"></param>
        /// <param name="failureTime">The time to reschedule the WorkItem if it isn't a fatal error. </param>
        /// <param name="maxRetryCount">The maximum number of times the WorkItem should be retried before a fatal error occurs.</param>
        public abstract void Fail(WorkItemFailureType failureType, DateTime failureTime, int maxRetryCount);


        public void Postpone()
        {
            Postpone(TimeSpan.FromSeconds(PostponeSecond));
        }

        /// <summary>
        /// Postpone a <see cref="WorkItem"/>
        /// </summary>
        public abstract void Postpone(TimeSpan delay);

        /// <summary>
        /// Complete a <see cref="WorkItem"/>.
        /// </summary>
        public abstract void Complete();

        /// <summary>
        /// Make a <see cref="WorkItem"/> Idle.
        /// </summary>
        public abstract void Idle();

        /// <summary>
        /// Mark <see cref="WorkItem"/> as being in the process of canceling
        /// </summary>
        public abstract void Canceling();

        /// <summary>
        /// Cancel a <see cref="WorkItem"/>
        /// </summary>
        public abstract void Cancel();

        /// <summary>
        /// Delete a <see cref="WorkItem"/>.
        /// </summary>
        public abstract void Delete();

        /// <summary>
        /// Update the progress for a <see cref="WorkItem"/>.  Progress will be published.
        /// </summary>
        public void UpdateProgress()
        {
            // We were originally committing to the database here, but decided to only commit when done processing the WorkItem.
            // This could lead to some misleading progress if a Refresh is done.
            Publish(false);
        }

        /// <summary>
        /// Update the progress for a <see cref="WorkItem"/>.  Progress will be published.
        /// </summary>
        public void UpdateProgress(bool updateDatabase)
        {
            // We were originally committing to the database here, but decided to only commit when done processing the WorkItem.
            // This could lead to some misleading progress if a Refresh is done.
            Publish(updateDatabase);
        }

        #endregion

        protected abstract void Publish(bool saveToDatabase);

    }
}
