using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using System.Threading;
using uDicom.Common;
using uDicom.WorkItemService;
using uDicom.WorkItemService.Interface;
using UIH.Dicom.Network;
using UIH.Dicom.Network.Scu;

namespace uDicom.WorkItem.Archive.DicomSend
{
    public class DicomSendItemProcessor : BaseItemProcessor<DicomSendRequest, DicomSendProgress>
    {
        #region Private Members

        private StorageScu _scu;

        #endregion

        #region Public Properties

        public DicomSendStudyRequest SendStudy
        {
            get { return Request as DicomSendStudyRequest; }
        }

        public DicomSendSeriesRequest SendSeries
        {
            get { return Request as DicomSendSeriesRequest; }
        }

        public DicomSendSopRequest SendSops
        {
            get { return Request as DicomSendSopRequest; }
        }

        #endregion

        public override void Process()
        {
            string localAe = Request.DestinationServerAETitle;
            string remoteAe = Request.DestinationServerAETitle;
            string remoteHostname = Request.DestinationServerHostname;
            int remotePort = Request.DestinationServerPort;

            _scu = new StorageScu(localAe, remoteAe, remoteHostname, remotePort);

            Progress.TotalImagesToSend = _scu.TotalSubOperations;
            Progress.FailureSubOperations = 0;
            Progress.WarningSubOperations = 0;
            Progress.SuccessSubOperations = 0;
            Progress.IsCancelable = true;
            Proxy.UpdateProgress();

            _scu.Send();

        }


        #region Private Methods

        private void LoadImagesToSend()
        {
            if (SendStudy != null)
            {
                
            }
            else if (SendSeries != null)
            {
                 
            }else if (SendSops != null)
            {
                
            }
        }

        #endregion 
    }
}
