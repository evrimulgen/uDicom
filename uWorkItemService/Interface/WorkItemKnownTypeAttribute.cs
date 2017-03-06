using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uDicom.WorkItemService.Interface
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class WorkItemKnownTypeAttribute : Attribute
    {
    }
}
