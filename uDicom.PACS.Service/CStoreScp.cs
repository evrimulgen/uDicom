#region License

// 
// Copyright (c) 2011 - 2012, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using UIH.Dicom.Network;
using UIH.Dicom.Network.Scp;
using UIH.Dicom.PACS.Service.Interface;
using UIH.Pacs.Services.Dicom;

namespace UIH.Dicom.PACS.Service
{
    [Export(typeof(IDicomScp<DicomScpContext>))]
    public abstract class CStoreScp : BaseScp
    {
        #region Private Members
        private IList<SupportedSop> _list;

        private static readonly List<SopClass> StorageAbstractSyntaxList =
            new List<SopClass>()
                {
                    SopClass.BasicStudyContentNotificationSopClassRetired,
                    SopClass.ProceduralEventLoggingSopClass,
                    SopClass.SubstanceAdministrationLoggingSopClass,
                    SopClass.DetachedPatientManagementSopClassRetired,
                    SopClass.DetachedVisitManagementSopClassRetired,
                    SopClass.DetachedStudyManagementSopClassRetired,
                    SopClass.StudyComponentManagementSopClassRetired,
                    SopClass.ModalityPerformedProcedureStepSopClass,
                    SopClass.ModalityPerformedProcedureStepRetrieveSopClass,
                    SopClass.ModalityPerformedProcedureStepNotificationSopClass,
                    SopClass.DetachedResultsManagementSopClassRetired,
                    SopClass.DetachedInterpretationManagementSopClassRetired,
                    SopClass.BasicFilmSessionSopClass,
                    SopClass.BasicFilmBoxSopClass,
                    SopClass.BasicGrayscaleImageBoxSopClass,
                    SopClass.BasicColorImageBoxSopClass,
                    SopClass.ReferencedImageBoxSopClassRetired,
                    SopClass.PrintJobSopClass,
                    SopClass.BasicAnnotationBoxSopClass,
                    SopClass.PrinterSopClass,
                    SopClass.PrinterConfigurationRetrievalSopClass,
                    SopClass.VoiLutBoxSopClass,
                    SopClass.PresentationLutSopClass,
                    SopClass.ImageOverlayBoxSopClassRetired,
                    SopClass.BasicPrintImageOverlayBoxSopClassRetired,
                    SopClass.PrintQueueManagementSopClassRetired,
                    SopClass.StoredPrintStorageSopClassRetired,
                    SopClass.HardcopyGrayscaleImageStorageSopClassRetired,
                    SopClass.HardcopyColorImageStorageSopClassRetired,
                    SopClass.PullPrintRequestSopClassRetired,
                    SopClass.MediaCreationManagementSopClassUid,
                    SopClass.ComputedRadiographyImageStorage,
                    SopClass.DigitalXRayImageStorageForPresentation,
                    SopClass.DigitalXRayImageStorageForProcessing,
                    SopClass.DigitalMammographyXRayImageStorageForPresentation,
                    SopClass.DigitalMammographyXRayImageStorageForProcessing,
                    SopClass.DigitalIntraOralXRayImageStorageForPresentation,
                    SopClass.DigitalIntraOralXRayImageStorageForProcessing,
                    SopClass.CtImageStorage,
                    SopClass.EnhancedCtImageStorage,
                    SopClass.UltrasoundMultiFrameImageStorageRetired,
                    SopClass.UltrasoundMultiFrameImageStorage,
                    SopClass.MrImageStorage,
                    SopClass.EnhancedMrImageStorage,
                    SopClass.MrSpectroscopyStorage,
                    SopClass.NuclearMedicineImageStorageRetired,
                    SopClass.UltrasoundImageStorageRetired,
                    SopClass.UltrasoundImageStorage,
                    SopClass.SecondaryCaptureImageStorage,
                    SopClass.MultiFrameSingleBitSecondaryCaptureImageStorage,
                    SopClass.MultiFrameGrayscaleByteSecondaryCaptureImageStorage,
                    SopClass.MultiFrameGrayscaleWordSecondaryCaptureImageStorage,
                    SopClass.MultiFrameTrueColorSecondaryCaptureImageStorage,
                    SopClass.StandaloneOverlayStorageRetired,
                    SopClass.StandaloneCurveStorageRetired,
                    SopClass.WaveformStorageTrialRetired,
                    SopClass.Sop12LeadEcgWaveformStorage,
                    SopClass.GeneralEcgWaveformStorage,
                    SopClass.AmbulatoryEcgWaveformStorage,
                    SopClass.HemodynamicWaveformStorage,
                    SopClass.CardiacElectrophysiologyWaveformStorage,
                    SopClass.BasicVoiceAudioWaveformStorage,
                    SopClass.StandaloneModalityLutStorageRetired,
                    SopClass.StandaloneVoiLutStorageRetired,
                    SopClass.GrayscaleSoftcopyPresentationStateStorageSopClass,
                    SopClass.ColorSoftcopyPresentationStateStorageSopClass,
                    SopClass.PseudoColorSoftcopyPresentationStateStorageSopClass,
                    SopClass.BlendingSoftcopyPresentationStateStorageSopClass,
                    SopClass.XRayAngiographicImageStorage,
                    SopClass.EnhancedXaImageStorage,
                    SopClass.XRayRadiofluoroscopicImageStorage,
                    SopClass.EnhancedXrfImageStorage,
                    SopClass.XRay3dAngiographicImageStorage,
                    SopClass.XRay3dCraniofacialImageStorage,
                    SopClass.XRayAngiographicBiPlaneImageStorageRetired,
                    SopClass.NuclearMedicineImageStorage,
                    SopClass.RawDataStorage,
                    SopClass.SpatialRegistrationStorage,
                    SopClass.SpatialFiducialsStorage,
                    SopClass.DeformableSpatialRegistrationStorage,
                    SopClass.SegmentationStorage,
                    SopClass.RealWorldValueMappingStorage,
                    SopClass.VlEndoscopicImageStorage,
                    SopClass.VideoEndoscopicImageStorage,
                    SopClass.VlMicroscopicImageStorage,
                    SopClass.VideoMicroscopicImageStorage,
                    SopClass.VlSlideCoordinatesMicroscopicImageStorage,
                    SopClass.VlPhotographicImageStorage,
                    SopClass.VideoPhotographicImageStorage,
                    SopClass.OphthalmicPhotography8BitImageStorage,
                    SopClass.OphthalmicPhotography16BitImageStorage,
                    SopClass.StereometricRelationshipStorage,
                    SopClass.OphthalmicTomographyImageStorage,
                    SopClass.TextSrStorageTrialRetired,
                    SopClass.AudioSrStorageTrialRetired,
                    SopClass.DetailSrStorageTrialRetired,
                    SopClass.ComprehensiveSrStorage,
                    SopClass.ProcedureLogStorage,
                    SopClass.MammographyCadSrStorage,
                    SopClass.KeyObjectSelectionDocumentStorage,
                    SopClass.ChestCadSrStorage,
                    SopClass.XRayRadiationDoseSrStorage,
                    SopClass.EncapsulatedPdfStorage,
                    SopClass.EncapsulatedCdaStorage,
                    SopClass.PositronEmissionTomographyImageStorage,
                    SopClass.StandalonePetCurveStorageRetired,
                    SopClass.RtImageStorage,
                    SopClass.RtDoseStorage,
                    SopClass.RtStructureSetStorage,
                    SopClass.RtBeamsTreatmentRecordStorage,
                    SopClass.RtPlanStorage,
                    SopClass.RtBrachyTreatmentRecordStorage,
                    SopClass.RtTreatmentSummaryRecordStorage,
                    SopClass.RtIonPlanStorage,
                    SopClass.RtIonBeamsTreatmentRecordStorage,
                    SopClass.InstanceAvailabilityNotificationSopClass,
                    SopClass.UnifiedProcedureStepPushSopClass,
                    SopClass.UnifiedProcedureStepWatchSopClass,
                    SopClass.UnifiedProcedureStepPullSopClass,
                    SopClass.UnifiedProcedureStepEventSopClass,
                    SopClass.GeneralRelevantPatientInformationQuery,
                    SopClass.BreastImagingRelevantPatientInformationQuery,
                    SopClass.CardiacRelevantPatientInformationQuery,
                    SopClass.HangingProtocolStorage,
                    SopClass.HangingProtocolInformationModelFind,
                    SopClass.HangingProtocolInformationModelMove,
                    SopClass.ProductCharacteristicsQuerySopClass,
                    SopClass.SubstanceApprovalQuerySopClass
                };

        private static readonly List<TransferSyntax> TransferSyntaxUidList =
            new List<TransferSyntax>
                {
                    TransferSyntax.ExplicitVrLittleEndian,
                    TransferSyntax.ImplicitVrLittleEndian,
                    TransferSyntax.JpegLosslessNonHierarchicalFirstOrderPredictionProcess14SelectionValue1
                };
        #endregion

        #region IDicomScp Members

        public override bool OnReceiveRequest(DicomServer server, ServerAssociationParameters association, 
            byte presentationId, DicomMessage message)
        {
            try
            {
                // TODO

                //SopInstanceImporterContext context = new SopInstanceImporterContext(
                //    String.Format("{0}_{1}", association.CallingAE, association.TimeStamp.ToString("yyyyMMddhhmmss")),
                //    association.CallingAE, association.CalledAE);

                //SopInstanceImporter importer = new SopInstanceImporter(context);
                //DicomProcessingResult result = importer.Import(message);

                DicomProcessingResult result = new DicomProcessingResult();
                ISopInstanceImporter importer = IoC.Get<ISopInstanceImporter>();
                if (importer != null)
                {
                    importer.Import(message);
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
                Log.Logger.TraceException(ex);
                return false;  // caller will abort the association
            }
            catch (Exception ex)
            {
                Log.Logger.TraceException(ex);
                return false;  // caller will abort the association
            }
        }

        /// <summary>
        /// Returns a list of the services supported by this plugin.
        /// </summary>
        /// <returns></returns>
        public override IList<SupportedSop> GetSupportedSopClasses()
        {
            if (_list == null)
            {
                _list = new List<SupportedSop>();

                foreach (var sopClass in StorageAbstractSyntaxList)
                {
                    var sop = new SupportedSop { SopClass = sopClass };
                    sop.SyntaxList.Add(TransferSyntax.ExplicitVrLittleEndian);
                    sop.SyntaxList.Add(TransferSyntax.ImplicitVrLittleEndian);
                    _list.Add(sop);
                }
            }
            return _list;
        }

        #endregion

        #region Overridden BaseSCP methods

        protected override DicomPresContextResult OnVerifyAssociation(AssociationParameters association, byte pcid)
        {
            if (Device == null)
                return DicomPresContextResult.Accept;

            if (!Device.AllowStorage)
            {
                return DicomPresContextResult.RejectUser;
            }

            //Only accept key objects and presentation states
//             if (Device.AcceptKOPR)
//             {
//                 DicomPresContext context = association.GetPresentationContext(pcid);
//                 if (context.AbstractSyntax.Equals(SopClass.KeyObjectSelectionDocumentStorage)
//                   || context.AbstractSyntax.Equals(SopClass.GrayscaleSoftcopyPresentationStateStorageSopClass)
//                   || context.AbstractSyntax.Equals(SopClass.BlendingSoftcopyPresentationStateStorageSopClass)
//                   || context.AbstractSyntax.Equals(SopClass.ColorSoftcopyPresentationStateStorageSopClass)
//                   || context.AbstractSyntax.Equals(SopClass.PseudoColorSoftcopyPresentationStateStorageSopClass))
//                     return DicomPresContextResult.Accept;
// 
//                 return DicomPresContextResult.RejectUser;
//             }

            return DicomPresContextResult.Accept;
        }

        #endregion 
    }
}