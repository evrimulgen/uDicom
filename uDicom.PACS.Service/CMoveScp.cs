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
using UIH.Dicom.Network.Scu;

namespace UIH.Dicom.PACS.Service
{
    [Export(typeof(IDicomScp<DicomScpContext>))]
    public class CMoveScp : BaseScp
    {
        #region Private members
        private readonly List<SupportedSop> _list = new List<SupportedSop>();
        private StorageScu _scu;
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
        /// Return a list of SOP Classes and Transfer Syntaxes supported by this extension.
        /// </summary>
        /// <returns></returns>
        public override IList<SupportedSop> GetSupportedSopClasses()
        {
            return _list;
        }

        /// <summary>
        /// Main routine for processing C-MOVE-RQ messages.  Called by the <see cref="DicomScp{DicomScpParameters}"/> component.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="association"></param>
        /// <param name="presentationID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool OnReceiveRequest(DicomServer server, ServerAssociationParameters association, byte presentationID, DicomMessage message)
        {
            bool finalResponseSent = false;

            try
            {
                if(message.CommandField == DicomCommandField.CCancelRequest)
                {
                    if(_scu != null)
                    {
                        _scu.Cancel();
                    }

                    return true;
                }

                // Get the level of the Move.
                String level = message.DataSet[DicomTags.QueryRetrieveLevel].GetString(0, "");

                // Trim the remote AE, see extra spaces at the end before which has caused problems
                string remoteAe = message.MoveDestination.Trim();

                _scu = new StorageScu("SimplePacs", "any", "127.0.0.1", 2999);

                finalResponseSent = true;

                return true;

            }catch(Exception e)
            {
                Log.Logger.Error("Unexpected exception when processing C-MOVE-RQ");
                Log.Logger.TraceException(e);

                if (finalResponseSent == false)
                {
                    try
                    {
                        server.SendCMoveResponse(presentationID, message.MessageId, new DicomMessage(),
                                                 DicomStatuses.QueryRetrieveUnableToProcess);
                    }
                    catch (Exception x)
                    {
                        Log.Logger.Error("Unable to send final C-MOVE-RSP message on association from {0} to {1}",
                                     association.CallingAE, association.CalledAE);
                        Log.Logger.TraceException(x);
                    }
                }
            }

            return false;
        }

        protected override DicomPresContextResult OnVerifyAssociation(AssociationParameters association, byte pcid)
        {
            return DicomPresContextResult.Accept;
        }
    }
}