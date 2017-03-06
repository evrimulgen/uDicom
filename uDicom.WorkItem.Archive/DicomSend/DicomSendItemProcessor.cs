using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using System.Threading;
using uDicom.Common;
using uDicom.WorkItemService;
using uDicom.WorkItemService.Interface;

namespace uDicom.WorkItem.Archive.DicomSend
{
    public class DicomSendItemProcessor : BaseItemProcessor<DicomSendRequest, DicomSendProgress>
    {
        public override void Process()
        {
            for (int i = 0; i < 10; i++)
            {
                Platform.Log(LogLevel.Warn, "Processing ....");
                Thread.Sleep(1000);
            }

            Proxy.Complete();
            
        }
    }
}
