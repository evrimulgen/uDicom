using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using uDicom.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItemService.Common
{
    public static class WorkItemRequestTypeProvider
    {
        private static List<Type> _knownTypes;
        private static readonly object SyncLock = new Object();

        public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider ignored)
        {
            lock (SyncLock)
            {
                // TODO (CR Jun 2012): Just do static initialization? Then there's no need for the lock.
                if (_knownTypes == null)
                {
                    var variables = IoC.GetAll<WorkItemRequest>();
                    _knownTypes = new List<Type>();

                    foreach (var workItemRequest in variables)
                    {
                        Type type = workItemRequest.GetType();
                        
                        _knownTypes.Add(type);
                    }
                }

                return _knownTypes;
            }
        }
    }

    [DataContract(Name = "WorkItemConcurrency", Namespace = ImageViewerWorkItemNamespace.Value)]
    public enum WorkItemConcurrency
    {
        [EnumMember]
        Exclusive,
        //Note: This is unlikely to be used for anything other than retrieves, but we want anything "study related" to wait for other study related things,
        //but we also need retrieves and "study receive/process" items to be able to run concurrently. Also, since we know a retrieve will ultimately trigger
        //a study process, it is reasonable to make, say, a send for the same study wait for the retrieve to finish.
        [EnumMember]
        StudyUpdateTrigger,

        [EnumMember]
        StudyUpdate,

        [EnumMember]
        StudyDelete,

        [EnumMember]
        StudyRead,

        [EnumMember]
        NonExclusive
    }

    public abstract class WorkItemRequest : DataContractBase
    {
        protected WorkItemRequest()
        {
            Priority = WorkItemPriorityEnum.Normal;
        }

        public abstract WorkItemConcurrency ConcurrencyType { get; }

        [DataMember]
        public WorkItemPriorityEnum Priority { get; set; }

        [DataMember]
        public string WorkItemType { get; set; }

        [DataMember]
        public string UserName { get; set; }

        public abstract string ActivityDescription { get; }

        public abstract string ActivityTypeString { get; }

        [DataMember]
        public bool CancellationCanResultInPartialStudy { get; protected set; }
    }
}
