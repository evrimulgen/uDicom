using System;
using System.Runtime.Serialization;

namespace uDicom.WorkItemService.Interface
{
    [DataContract]
    public abstract class WorkItemProgress
    {
        public virtual string Status { get { return string.Empty; } }

        [DataMember(IsRequired = false)]
        public string StatusDetails { get; set; }

        public virtual Decimal PercentComplete { get { return new decimal(0.0); } }

        public virtual Decimal PercentFailed { get { return new decimal(0.0); } }

        [DataMember(IsRequired = true)]
        public bool IsCancelable { get; set; }
    }
}
