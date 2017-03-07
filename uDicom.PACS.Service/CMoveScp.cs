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
using UIH.Dicom.Network.Scp;
using UIH.Dicom.Network.Scu;
using UIH.Dicom.PACS.Service.Interface;

namespace UIH.Dicom.PACS.Service
{
    
    public class CMoveScp : BaseScp
    {
        #region Private members

        private readonly List<SupportedSop> _list = new List<SupportedSop>();
        private PacsStorageScu _theScu;

        #endregion

        public CMoveScp()
        {
            var sop = new SupportedSop
            {
                SopClass = SopClass.PatientRootQueryRetrieveInformationModelMove
            };
            sop.SyntaxList.Add(TransferSyntax.ExplicitVrLittleEndian);
            sop.SyntaxList.Add(TransferSyntax.ImplicitVrLittleEndian);
            _list.Add(sop);

            sop = new SupportedSop
            {
                SopClass = SopClass.StudyRootQueryRetrieveInformationModelMove
            };
            sop.SyntaxList.Add(TransferSyntax.ExplicitVrLittleEndian);
            sop.SyntaxList.Add(TransferSyntax.ImplicitVrLittleEndian);
            _list.Add(sop);
        }

        /// <summary>
        ///     Return a list of SOP Classes and Transfer Syntaxes supported by this extension.
        /// </summary>
        /// <returns></returns>
        public override IList<SupportedSop> GetSupportedSopClasses()
        {
            return _list;
        }

        /// <summary>
        ///     Main routine for processing C-MOVE-RQ messages.  Called by the <see cref="DicomScp{TContext}" /> component.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="association"></param>
        /// <param name="presentationID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool OnReceiveRequest(DicomServer server, ServerAssociationParameters association,
            byte presentationID, DicomMessage message)
        {
            var finalResponseSent = false;
            string errorComment;

            try
            {
                // check for a Cancel Message, and cance the scu 
                if (message.CommandField == DicomCommandField.CCancelRequest)
                {
                    if (_theScu != null)
                    {
                        _theScu.Cancel();
                    }

                    return true;
                }

                var level = message.DataSet[DicomTags.QueryRetrieveLevel].GetString(0, string.Empty);

                var remoteAe = message.MoveDestination.Trim();

                // load remote device for move information 
                var device = IoC.Get<IDeviceManager>().LookupDevice(Partition, remoteAe);

                if (device == null)
                {
                    errorComment = string.Format(
                        "Unknown move destination \"{0}\", failing C-MOVE-RQ from {1} to {2}",
                        remoteAe, association.CallingAE, association.CalledAE);
                    Platform.Log(LogLevel.Error, errorComment);
                    server.SendCMoveResponse(presentationID, message.MessageId, new DicomMessage(),
                        DicomStatuses.QueryRetrieveMoveDestinationUnknown, errorComment);
                    finalResponseSent = true;
                    return true;
                }

                //If the remote node is a DHCP node, use its IP address from the connection information, else
                // use what is configured.Always use the configured port. 
                if (device.Dhcp && association.CallingAE.Equals(remoteAe))
                {
                    device.Hostname = association.RemoteEndPoint.Address.ToString();
                }

                // now setup the storage scu component 
                _theScu = new PacsStorageScu(Partition, device, association.CallingAE, message.MessageId);

                bool bOnline;

                if (level.Equals("PATIENT"))
                {
                    bOnline = GetSopListForPatient(message, out errorComment);
                }
                else if (level.Equals("STUDY"))
                {
                    bOnline = GetSopListForStudy(message, out errorComment);
                }
                else if (level.Equals("SERIES"))
                {
                    bOnline = GetSopListForSeries(message, out errorComment);
                }
                else if (level.Equals("IMAGE"))
                {
                    bOnline = GetSopListForInstance(message, out errorComment);
                }
                else
                {
                    errorComment = string.Format("Unexpected Study Root Move Query/Retrieve level: {0}", level);
                    Platform.Log(LogLevel.Error, errorComment);

                    server.SendCMoveResponse(presentationID, message.MessageId, new DicomMessage(),
                        DicomStatuses.QueryRetrieveIdentifierDoesNotMatchSOPClass,
                        errorComment);
                    finalResponseSent = true;
                    return true;
                }

                // Could not find an online/readable location for the requested objects to move.
                // Note that if the C-MOVE-RQ included a list of study instance uids, and some 
                // were online and some offline, we don't fail now (ie, the check on the Count)
                if (!bOnline || _theScu.StorageInstanceList.Count == 0)
                {
                    Platform.Log(LogLevel.Error, "Unable to find online storage location for C-MOVE-RQ: {0}",
                        errorComment);

                    server.SendCMoveResponse(presentationID, message.MessageId, new DicomMessage(),
                        DicomStatuses.QueryRetrieveUnableToPerformSuboperations,
                        string.IsNullOrEmpty(errorComment) ? string.Empty : errorComment);
                    finalResponseSent = true;
                    _theScu.Dispose();
                    _theScu = null;
                    return true;
                }

                _theScu.ImageStoreCompleted += delegate(object sender, StorageInstance instance)
                {
                    var scu = (StorageScu) sender;
                    var msg = new DicomMessage();
                    DicomStatus status;

                    if (instance.SendStatus.Status == DicomState.Failure)
                    {
                        errorComment =
                            string.IsNullOrEmpty(instance.ExtendedFailureDescription)
                                ? instance.SendStatus.ToString()
                                : instance.ExtendedFailureDescription;
                    }

                    if (scu.RemainingSubOperations == 0)
                    {
                        foreach (var sop in _theScu.StorageInstanceList)
                        {
                            if ((sop.SendStatus.Status != DicomState.Success)
                                && (sop.SendStatus.Status != DicomState.Warning))
                                msg.DataSet[DicomTags.FailedSopInstanceUidList].AppendString(sop.SopInstanceUid);
                        }
                        if (scu.Status == ScuOperationStatus.Canceled)
                            status = DicomStatuses.Cancel;
                        else if (scu.Status == ScuOperationStatus.ConnectFailed)
                            status = DicomStatuses.QueryRetrieveMoveDestinationUnknown;
                        else if (scu.FailureSubOperations > 0)
                            status = DicomStatuses.QueryRetrieveSubOpsOneOrMoreFailures;
                        else if (!bOnline)
                            status = DicomStatuses.QueryRetrieveUnableToPerformSuboperations;
                        else
                            status = DicomStatuses.Success;
                    }
                    else
                    {
                        status = DicomStatuses.Pending;

                        if (scu.RemainingSubOperations%5 != 0)
                            return;
                        // Only send a RSP every 5 to reduce network load
                    }

                    server.SendCMoveResponse(presentationID, message.MessageId,
                        msg, status,
                        (ushort) scu.SuccessSubOperations,
                        (ushort) scu.RemainingSubOperations,
                        (ushort) scu.FailureSubOperations,
                        (ushort) scu.WarningSubOperations,
                        status == DicomStatuses.QueryRetrieveSubOpsOneOrMoreFailures
                            ? errorComment
                            : string.Empty);


                    if (scu.RemainingSubOperations == 0)
                        finalResponseSent = true;
                };

                _theScu.BeginSend(
                    delegate(IAsyncResult ar)
                    {
                        if (_theScu != null)
                        {
                            if (!finalResponseSent)
                            {
                                var msg = new DicomMessage();
                                server.SendCMoveResponse(presentationID, message.MessageId,
                                    msg, DicomStatuses.QueryRetrieveSubOpsOneOrMoreFailures,
                                    (ushort) _theScu.SuccessSubOperations,
                                    0,
                                    (ushort)
                                        (_theScu.FailureSubOperations +
                                         _theScu.RemainingSubOperations),
                                    (ushort) _theScu.WarningSubOperations, errorComment);
                                finalResponseSent = true;
                            }

                            _theScu.EndSend(ar);
                            _theScu.Dispose();
                            _theScu = null;
                        }
                    },
                    _theScu);

                return true;
            }
            catch (Exception e)
            {
                Platform.Log(LogLevel.Error, e, "Unexpected exception when processing C-MOVE-RQ");
                if (finalResponseSent == false)
                {
                    try
                    {
                        server.SendCMoveResponse(presentationID, message.MessageId, new DicomMessage(),
                            DicomStatuses.QueryRetrieveUnableToProcess, e.Message);
                        finalResponseSent = true;
                    }
                    catch (Exception x)
                    {
                        Platform.Log(LogLevel.Error, x,
                            "Unable to send final C-MOVE-RSP message on association from {0} to {1}",
                            association.CallingAE, association.CalledAE);
                        server.Abort();
                    }
                }
            }

            return false;
        }


        protected override DicomPresContextResult OnVerifyAssociation(AssociationParameters association, byte pcid)
        {
            if (Device == null)
                return DicomPresContextResult.Accept;

            if (!Device.AllowRetrieve)
            {
                return DicomPresContextResult.RejectUser;
            }

            return DicomPresContextResult.Accept;
        }

        #region Private Member 

        private bool GetSopListForPatient(DicomMessage message, out string errorComment)
        {
            errorComment = string.Empty;

            var instances = IoC.Get<ISopQuery>().GetSopListForPatient(message);

            foreach (var storageInstance in instances)
            {
                _theScu.AddStorageInstance(storageInstance);
            }

            return true;
        }

        private bool GetSopListForStudy(DicomMessage message, out string errorComment)
        {
            errorComment = string.Empty;

            var instances = IoC.Get<ISopQuery>().GetSopListForStudy(message);

            foreach (var storageInstance in instances)
            {
                _theScu.AddStorageInstance(storageInstance);
            }

            return true;
        }

        private bool GetSopListForSeries(DicomMessage message, out string errorComment)
        {
            errorComment = string.Empty;

            var instances = IoC.Get<ISopQuery>().GetSopListForSeries(message);

            foreach (var storageInstance in instances)
            {
                _theScu.AddStorageInstance(storageInstance);
            }

            return true;
        }

        private bool GetSopListForInstance( DicomMessage message, out string errorComment)
        {
            errorComment = string.Empty;

            var instances = IoC.Get<ISopQuery>().GetSopListForInstance(message);

            foreach (var storageInstance in instances)
            {
                _theScu.AddStorageInstance(storageInstance);
            }

            return true;
        }

        #endregion
    }
}