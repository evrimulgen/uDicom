using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uDicom.WorkItemService
{
    public class PolymorphicDataContractException : Exception
    {
        public PolymorphicDataContractException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
