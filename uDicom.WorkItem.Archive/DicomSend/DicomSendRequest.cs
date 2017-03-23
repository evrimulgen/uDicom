using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using uDicom.WorkItemService.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItem.Archive.DicomSend
{
    [Export(typeof(WorkItemRequest))]
    [DataContract]
    public abstract class DicomSendRequest : WorkItemRequest
    {
        public static string WorkItemTypeString = "DicomSend";

        [DataMember]
        public string DestinationServerName { get; set; }

        [DataMember(IsRequired = false)]
        public string DestinationServerHostname { get; set; }

        [DataMember(IsRequired = false)]
        public string DestinationServerAETitle { get; set; }

        [DataMember(IsRequired = false)]
        public int DestinationServerPort { get; set; }

        public override WorkItemConcurrency ConcurrencyType
        {
            get
            {
                return WorkItemConcurrency.NonExclusive;
            }
        }
    }

    [Export(typeof(WorkItemRequest))]
    [DataContract]
    public class DicomSendStudyRequest : DicomSendRequest
    {
        public DicomSendStudyRequest()
        {
            WorkItemType = WorkItemTypeString;
            Priority = WorkItemPriorityEnum.High;
            CancellationCanResultInPartialStudy = true;
        }

        [DataMember(IsRequired = false)]
        public List<string> StudyInstanceUids { get; set; }
    }

    [Export(typeof(WorkItemRequest))]
    [DataContract]
    public class DicomSendSeriesRequest : DicomSendRequest
    {
        public DicomSendSeriesRequest()
        {
            WorkItemType = WorkItemTypeString;
            Priority = WorkItemPriorityEnum.High;
            CancellationCanResultInPartialStudy = true;
        }

        [DataMember(IsRequired = false)]
        public List<string> SeriesInstanceUids { get; set; }
    }

    [Export(typeof(WorkItemRequest))]
    [DataContract]
    public class DicomSendSopRequest : DicomSendRequest
    {
        public DicomSendSopRequest()
        {
            WorkItemType = WorkItemTypeString;
            Priority = WorkItemPriorityEnum.Normal;
            CancellationCanResultInPartialStudy = true;
        }

        [DataMember(IsRequired = true)]
        public string SeriesInstanceUid { get; set; }

        [DataMember(IsRequired = true)]
        public List<string> SopInstanceUids { get; set; }
    }
    

    [Export(typeof(WorkItemProgress))]
    [DataContract]
    public class DicomSendProgress : WorkItemProgress
    {
        public DicomSendProgress()
        {
            IsCancelable = false;
        }

        [DataMember(IsRequired = true)]
        public int TotalImagesToSend { get; set; }

        [DataMember(IsRequired = true)]
        public int WarningSubOperations { get; set; }

        [DataMember(IsRequired = true)]
        public int FailureSubOperations { get; set; }

        [DataMember(IsRequired = true)]
        public int SuccessSubOperations { get; set; }

        public int RemainingSubOperations
        {
            get { return TotalImagesToSend - 
                    (WarningSubOperations + FailureSubOperations + SuccessSubOperations); }
        }

        public override string Status
        {
            get
            {
                if (TotalImagesToSend == 0)
                    return string.Empty;

                return string.Format(SR.DicomSendProgress_Status,
                    SuccessSubOperations + WarningSubOperations, FailureSubOperations,
                    RemainingSubOperations);
            }
        }

        public override decimal PercentComplete
        {
            get
            {
                if (TotalImagesToSend > 0)
                    return (decimal) (WarningSubOperations + FailureSubOperations + SuccessSubOperations)/
                           TotalImagesToSend;

                return new decimal(0.0);
            }
        }

        public override decimal PercentFailed
        {
            get
            {
                if (TotalImagesToSend > 0 && FailureSubOperations > 0)
                    return (decimal) FailureSubOperations/TotalImagesToSend;

                return new decimal(0.0);
            }
        }
    }
}
