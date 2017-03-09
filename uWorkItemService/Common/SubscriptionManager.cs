using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;

// the design idea is based on Juval Lowy's publish-subscribe framework.

namespace uDicom.WorkItemService.Common
{
    internal static class SubscriptionManager<T>
        where T : class
    {
        private static readonly object _syncLock;
        private static readonly Dictionary<string, List<T>> _subscribers;

        static SubscriptionManager()
        {
            _syncLock = new object();

            Type operationContractType = typeof(OperationContractAttribute);
            Type callbackType = typeof(T);
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

            _subscribers = new Dictionary<string, List<T>>();
            foreach (MethodInfo info in callbackType.GetMethods(bindingFlags))
            {
                if (info.IsDefined(operationContractType, false))
                    _subscribers[info.Name] = new List<T>();
            }
        }

        public static IEnumerable<string> GetMethods()
        {
            foreach (string eventName in _subscribers.Keys)
                yield return eventName;
        }

        public static void Subscribe(string eventOperation)
        {
            Subscribe(OperationContext.Current.GetCallbackChannel<T>(), eventOperation);
        }

        public static void Subscribe(T callback, string eventOperation)
        {
            if (String.IsNullOrEmpty(eventOperation))
            {
                lock (_syncLock)
                {
                    foreach (string subscribeMethod in _subscribers.Keys)
                    {
                        if (!_subscribers[subscribeMethod].Contains(callback))
                            _subscribers[subscribeMethod].Add(callback);
                    }
                }
            }
            else if (_subscribers.ContainsKey(eventOperation))
            {
                lock (_syncLock)
                {
                    if (!_subscribers[eventOperation].Contains(callback))
                        _subscribers[eventOperation].Add(callback);
                }
            }
            else
            {
                Debug.Assert(false);
            }
        }

        public static void Unsubscribe(string eventOperation)
        {
            Unsubscribe(OperationContext.Current.GetCallbackChannel<T>(), eventOperation);
        }

        public static void Unsubscribe(T callback, string eventOperation)
        {
            if (String.IsNullOrEmpty(eventOperation))
            {
                lock (_syncLock)
                {
                    foreach (string method in _subscribers.Keys)
                        _subscribers[method].Remove(callback);
                }
            }
            else if (_subscribers.ContainsKey(eventOperation))
            {
                lock (_syncLock)
                {
                    _subscribers[eventOperation].Remove(callback);
                }
            }
        }

        public static T[] GetSubscribers(string eventOperation)
        {
            Debug.Assert(eventOperation != null && _subscribers.ContainsKey(eventOperation));

            lock (_syncLock)
            {
                return _subscribers[eventOperation].ToArray();
            }
        }
    }
}
