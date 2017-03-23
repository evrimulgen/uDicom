#region License

// 
// Copyright (c) 2011 - 2012, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using System;
using System.Collections.Generic;
using uDicom.Common;
using UIH.Dicom.Network;
using UIH.Dicom.PACS.Service.Interface;
using UIH.Pacs.Services.Dicom;

namespace UIH.Dicom.PACS.Service
{
    public class CStoreScp : BaseScp
    {
        #region Private Members

        private static IList<SupportedSop> _list;

        #endregion

        #region IDicomScp Members

        public override bool OnReceiveRequest(DicomServer server, ServerAssociationParameters association, 
            byte presentationId, DicomMessage message)
        {
            try
            {
                SopInstanceImporterContext context = new SopInstanceImporterContext(
                    String.Format("{0}_{1}", association.CallingAE, association.TimeStamp.ToString("yyyyMMddhhmmss")),
                    association.CallingAE, Partition.AeTitle);

                DicomProcessingResult result = new DicomProcessingResult();
                ISopInstanceImporter importer = IoC.Get<ISopInstanceImporter>();
                if (importer != null)
                {
                    importer.Context = context;
                    result = importer.Import(message);
                }
                
                if (result.Successful)
                {
                    if (!String.IsNullOrEmpty(result.AccessionNumber))
                    {
                        Log.Logger.Info("Received SOP Instance {0} from {1} to {2} (A#:{3} StudyUid:{4})",
                                     result.SopInstanceUid, association.CallingAE, association.CalledAE, result.AccessionNumber,
                                     result.StudyInstanceUid);
                    }
                    else
                    {
                        Log.Logger.Info("Received SOP Instance {0} from {1} to {2} (StudyUid:{3})",
                                     result.SopInstanceUid, association.CallingAE, association.CalledAE,
                                     result.StudyInstanceUid);
                    }
                }
                else
                {
                    Log.Logger.Warn("Failure importing sop: {0}", result.ErrorMessage);
                }

                server.SendCStoreResponse(presentationId, message.MessageId, 
                    message.AffectedSopInstanceUid, result.DicomStatus);
                return true;
            }
            catch (DicomDataException ex)
            {
                Log.Logger.Error(ex, "Error when import {0}", message.AffectedSopInstanceUid);
                return false;  // caller will abort the association
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error when import {0}", message.AffectedSopInstanceUid);
                return false;  // caller will abort the association
            }
        }

        #endregion

        #region Overridden BaseSCP methods

        public override IList<SupportedSop> GetSupportedSopClasses()
        {
            if (_list == null)
            {
                ISopInstanceImporter importer = IoC.Get<ISopInstanceImporter>();
                _list = importer.GetStroageSupportedSopClasses();
            }

            return _list;
        }

        protected override DicomPresContextResult OnVerifyAssociation(AssociationParameters association, byte pcid)
        {
            if (Device == null)
                return DicomPresContextResult.Accept;

            if (!Device.AllowStorage)
            {
                return DicomPresContextResult.RejectUser;
            }

            return DicomPresContextResult.Accept;
        }


        public override bool ReceiveMessageAsFileStream(DicomServer server, ServerAssociationParameters association,
            byte presentationId, DicomMessage message)
        {
            var sopClassUid = message.AffectedSopClassUid;

            if (sopClassUid.Equals(SopClass.BreastTomosynthesisImageStorageUid)
                || sopClassUid.Equals(SopClass.EnhancedCtImageStorageUid)
                || sopClassUid.Equals(SopClass.EnhancedMrColorImageStorageUid)
                || sopClassUid.Equals(SopClass.EnhancedMrImageStorageUid)
                || sopClassUid.Equals(SopClass.EnhancedPetImageStorageUid)
                || sopClassUid.Equals(SopClass.EnhancedUsVolumeStorageUid)
                || sopClassUid.Equals(SopClass.EnhancedXaImageStorageUid)
                || sopClassUid.Equals(SopClass.EnhancedXrfImageStorageUid)
                || sopClassUid.Equals(SopClass.UltrasoundMultiFrameImageStorageUid)
                || sopClassUid.Equals(SopClass.MultiFrameGrayscaleByteSecondaryCaptureImageStorageUid)
                || sopClassUid.Equals(SopClass.MultiFrameGrayscaleWordSecondaryCaptureImageStorageUid)
                || sopClassUid.Equals(SopClass.MultiFrameSingleBitSecondaryCaptureImageStorageUid)
                || sopClassUid.Equals(SopClass.MultiFrameTrueColorSecondaryCaptureImageStorageUid))
            {
                server.DimseDatasetStopTag = DicomTagDictionary.GetDicomTag(DicomTags.ReconstructionIndex); // Random tag at the end of group 20
                server.StreamMessage = true;
                return true;
            }

            return false;
        }

        #endregion

        #region IDicomScp Members

        public override IDicomFilestreamHandler OnStartFilestream(DicomServer server, ServerAssociationParameters association,
                                                                  byte presentationId, DicomMessage message)
        {
            return new StorageFilestreamHandler(_context, association);
        }

        #endregion
    }
}