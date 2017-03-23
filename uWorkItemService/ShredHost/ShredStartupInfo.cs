using System;
using uDicom.Common;

namespace uDicom.WorkItemService.ShredHost
{
    internal class ShredStartupInfo : MarshalByRefObject
    {
        public ShredStartupInfo(Uri assemblyPath, string shredName, string shredTypeName, ShredIsolationLevel isolationLevel)
        {
            Platform.CheckForNullReference(assemblyPath, "assemblyPath");
            Platform.CheckForEmptyString(shredName, "shredName");
            Platform.CheckForEmptyString(shredTypeName, "shredTypeName");

            AssemblyPath = assemblyPath;
            ShredName = shredName;
            ShredTypeName = shredTypeName;
            IsolationLevel = isolationLevel;
        }

        #region Properties

        public string ShredTypeName { get; }

        public string ShredName { get; }

        public Uri AssemblyPath { get; }

        public ShredIsolationLevel IsolationLevel { get; }

        #endregion
    }
}