using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIH.Dicom.Log;

namespace UIH.Dicom.PACS.Service
{
    internal class Log
    {
        public static ILog Logger
        {
            get { return LogManager.GetLog("UIH.Dicom.PACS.Service"); }
        }
    }
}
