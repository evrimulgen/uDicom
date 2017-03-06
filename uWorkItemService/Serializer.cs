using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using uDicom.Common;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItemService
{
    internal class Serializer
    {
        private static readonly DataContractSerializer WorkItemRequestSerializer =
             CreateDataContractSerializer<WorkItemRequest>();

        private static readonly DataContractSerializer WorkItemProgressSerializer =
            CreateDataContractSerializer<WorkItemProgress>();

        public static string SerializeWorkItemRequest(WorkItemRequest data)
        {
            if (data == null) return null;

            var sb = new StringBuilder();
            using (var sw = XmlWriter.Create(sb))
            {
                WorkItemRequestSerializer.WriteObject(sw, data);
            }
            return sb.ToString();
        }

        public static WorkItemRequest DeserializeWorkItemRequest(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;

            try
            {
                using (var tr = new StringReader(data))
                using (var sr = XmlReader.Create(tr))
                    return (WorkItemRequest)WorkItemRequestSerializer.ReadObject(sr);
            }
            catch (Exception ex)
            {
                Platform.Log(LogLevel.Debug, ex, "Unable to deserialize work item request");
            }

            return null;
        }

        public static string SerializeWorkItemProgress(WorkItemProgress data)
        {
            if (data == null) return null;

            var sb = new StringBuilder();
            using (var sw = XmlWriter.Create(sb))
            {
                WorkItemProgressSerializer.WriteObject(sw, data);
            }
            return sb.ToString();
        }

        public static WorkItemProgress DeserializeWorkItemProgress(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;

            try
            {
                using (var tr = new StringReader(data))
                using (var sr = XmlReader.Create(tr))
                    return (WorkItemProgress)WorkItemProgressSerializer.ReadObject(sr);
            }
            catch (Exception ex)
            {
                Platform.Log(LogLevel.Debug, ex, "Unable to deserialize work item progress");
            }

            return null;
        }

        private static DataContractSerializer CreateDataContractSerializer<TObject>()
           where TObject: class 
        {
            return new DataContractSerializer(typeof(TObject), null, int.MaxValue, false, false, null, 
                new PolymorphicDataContractResolver<TObject>());
        }
    }
}
