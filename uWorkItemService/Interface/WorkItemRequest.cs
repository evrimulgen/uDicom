using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using uDicom.Common;

namespace uDicom.WorkItemService.Interface
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

    [DataContract]
    public abstract class WorkItemRequest
    {
        [DataMember]
        public WorkItemPriorityEnum Priority { get; set; }

        [DataMember]
        public string WorkItemType { get; set; }
    }
}
