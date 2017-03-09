using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uDicom.WorkItemService.Common
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class WorkItemRequestAttribute : Attribute
    {
    }
}
