using System;
using System.Collections.Generic;
using uDicom.Common;
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

        protected DicomScpContext _context;

        #endregion

        #region Properties

        protected IServerPartition Partition
        {
            get { return _context.Partition; }
        }

        protected IDevice Device { get; set; }

        #endregion

        protected abstract DicomPresContextResult OnVerifyAssociation(AssociationParameters association,
                                                                      byte pcid);

        public DicomPresContextResult VerifyAssociation(AssociationParameters association, byte pcid)
        {
            bool isNew;

            var dm = IoC.Get<IDeviceManager>();
            Device = dm.LookupDevice(_context.Partition, association, out isNew);

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

        public virtual IDicomFilestreamHandler OnStartFilestream(DicomServer server, ServerAssociationParameters association,
                                                                 byte presentationId, DicomMessage message)
        {
            return null;
        }

        public virtual bool ReceiveMessageAsFileStream(DicomServer server, ServerAssociationParameters association, byte presentationId,
                                               DicomMessage message)
        {
            return false;
        }


        public virtual IList<SupportedSop> GetSupportedSopClasses()
        {
            throw new System.NotImplementedException("The method must be implement");
        }

        public void SetContext(DicomScpContext context)
        {
            _context = context;
        }

        public virtual void Cleanup()
        {
            
        }
    }
}
