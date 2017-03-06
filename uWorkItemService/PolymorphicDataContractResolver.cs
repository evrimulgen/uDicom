using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using uDicom.Common;

namespace uDicom.WorkItemService
{
    public class PolymorphicDataContractResolver<T> : DataContractResolver
        where T : class
    {
        private static Dictionary<string, Type> _contractMaps = GetContractMap();

        public static Dictionary<string, Type> GetContractMap()
        {
            var variables = IoC.GetAll<T>();

            var dict = new Dictionary<string , Type>();

            foreach (var variable in variables)
            {
                Type t = variable.GetType();

                dict.Add(t.Name, t);
            }

            return dict;
        }


        public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver,
            out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
        {
            return knownTypeResolver.TryResolveType(type, declaredType, knownTypeResolver, out typeName, out typeNamespace);
        }

        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType,
            DataContractResolver knownTypeResolver)
        {
            if (!string.IsNullOrEmpty(typeName))
            {
                Type contract;
                if (!_contractMaps.TryGetValue(typeName, out contract))
                {
                    throw new ArgumentException();
                }

                return contract;
            }

            return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
        }
    }
}
