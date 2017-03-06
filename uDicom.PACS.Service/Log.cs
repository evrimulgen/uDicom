using uDicom.Common;

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
