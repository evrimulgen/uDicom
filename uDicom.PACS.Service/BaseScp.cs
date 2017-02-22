using System;
using System.Collections.Generic;
using UIH.Dicom.Log;
using UIH.Dicom.Network;
using UIH.Dicom.Network.Scp;
using UIH.Dicom.PACS.Service.Interface;
using UIH.Dicom.PACS.Service.Model;
using UIH.Pacs.Services.Dicom;

namespace UIH.Dicom.PACS.Service
{
    public abstract class BaseScp : IDicomScp<DicomScpContext>
    {
        #region Protected Members

        private DicomScpContext _context;

        #endregion

        #region Properties

        protected ServerPartition Partition
        {
            get { return _context.Partition; }
        }

        protected Device Device { get; set; }

        #endregion

        protected abstract DicomPresContextResult OnVerifyAssociation(AssociationParameters association,
                                                                      byte pcid);

        public DicomPresContextResult VerifyAssociation(AssociationParameters association, byte pcid)
        {
            bool isNew;

            var dm = IoC.Get<IDeviceManager>();
            Device = dm.LookupDevice(association, out isNew);

            var result = OnVerifyAssociation(association, pcid);
            if(result != DicomPresContextResult.Accept)
            {
                Log.Logger.Info(
                    "Rejecting Presentation Context {0}:{1} in association between {2} and {3}.",
                    pcid, association.GetAbstractSyntax(pcid).Description,
                    association.CallingAE, association.CalledAE);
            }

            return result;

        }

        public virtual bool OnReceiveRequest(DicomServer server, ServerAssociationParameters association,
                                     byte presentationId, DicomMessage message)
        {
            throw new NotImplementedException("The method must be implement");
        }

        public void AssociationRelease(DicomServer server, AssociationParameters assoc)
        {
            throw new NotImplementedException();
        }

        public void AssociationAbort(DicomServer server, AssociationParameters assoc)
        {
            throw new NotImplementedException();
        }

        public void OnNetworkError(DicomServer server, AssociationParameters assoc)
        {
            throw new NotImplementedException();
        }

        public virtual IList<SupportedSop> GetSupportedSopClasses()
        {
            throw new System.NotImplementedException("The method must be implement");
        }

        public void SetContext(DicomScpContext context)
        {
            _context = context;
        }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }
    }
}
