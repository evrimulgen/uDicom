/////////////////////////////////////////////////////////////////////////
//// Copyright, (c) Shanghai United Imaging Healthcare Inc
//// All rights reserved. 
//// 
//// author: qiuyang.cao@united-imaging.com
////
//// File: DicomScpHandler.cs
////
//// Summary:
////
////
//// Date: 2014/08/19
//////////////////////////////////////////////////////////////////////////
#region License

// Copyright (c) 2011 - 2013, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uDicom.Common;
using UIH.Dicom.Log;
using UIH.Dicom.Network.Scu;

namespace UIH.Dicom.Network.Scp
{
    /// <summary>
    /// Class used to handle incoming SCP associations.
    /// </summary>
    internal class DicomScpHandler<TContext> : IDicomServerHandler
    {
        #region Private Members
        private readonly TContext _context;
        private readonly IDictionary<byte, IDicomScp<TContext>> _extensionList = new Dictionary<byte, IDicomScp<TContext>>();
        private readonly DicomScp<TContext>.AssociationVerifyCallback _verifier;
    	private readonly DicomScp<TContext>.AssociationComplete _complete;
    	private readonly List<StorageInstance> _instances = new List<StorageInstance>();
        private bool _cleanedUp = false;
        #endregion

        #region Contructor
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// The constructor creates a dictionary of each presentation context negotiated for the 
        /// association, and the plugin that will handle it.  This is used later when incoming request
        /// messages are processed.
        /// </remarks>
        /// <param name="server">The server.</param>
        /// <param name="parameters">Association parameters for the negotiated association.</param>
        /// <param name="userParms">User parameters to be passed to the plugins called by the class.</param>
        /// <param name="verifier">Delegate to call to verify an association before its accepted.</param>
        /// <param name="complete">Delegate to call when the association is closed/complete.  Can be null.</param>
        public DicomScpHandler(DicomServer server, ServerAssociationParameters parameters, TContext userParms, DicomScp<TContext>.AssociationVerifyCallback verifier, DicomScp<TContext>.AssociationComplete complete)
        {
            _context = userParms;
            _verifier = verifier;
        	_complete = complete;

           List<IDicomScp<TContext>> scps = IoC.GetAll<IDicomScp<TContext>>().ToList();

            // First set the user parms for each of the extensions before we do anything with them.
            foreach (object obj in scps)
            {
                IDicomScp<TContext> scp = obj as IDicomScp<TContext>;
                scp.SetContext(_context);
            }

            // Now, create a dictionary with the extension to be used for each presentation context.
            foreach (byte pcid in parameters.GetPresentationContextIDs())
            {
                if (parameters.GetPresentationContextResult(pcid) == DicomPresContextResult.Accept)
                {
                    SopClass acceptedSop = SopClass.GetSopClass(parameters.GetAbstractSyntax(pcid).UID);
                    TransferSyntax acceptedSyntax = parameters.GetAcceptedTransferSyntax(pcid);
                    foreach (object obj in scps)
                    {
                        IDicomScp<TContext> scp = obj as IDicomScp<TContext>;

                        IList<SupportedSop> sops = scp.GetSupportedSopClasses();
                        foreach (SupportedSop sop in sops)
                        {
                            if (sop.SopClass.Equals(acceptedSop))
                            {
                                if (sop.SyntaxList.Contains(acceptedSyntax))
                                {
                                    if (!_extensionList.ContainsKey(pcid))
                                    {
                                        _extensionList.Add(pcid, scp);
                                        break;
                                    }
                                    else
                                        LogAdapter.Logger.Error("SOP Class {0} supported by more than one extension", sop.SopClass.Name);
                                }
                            }
                        }
                    }
                }
            }

        }
        #endregion

        #region Private Methods

        public void Cleanup()
        {
            if (_cleanedUp) return;

            _cleanedUp = true;

            foreach (IDicomScp<TContext> scp in _extensionList.Values)
            {
                try
                {
                    scp.Cleanup();
                }
                catch (Exception)
                {}
            }
        }

        #endregion

        #region IDicomServerHandler Members

        void IDicomServerHandler.OnReceiveAssociateRequest(DicomServer server, ServerAssociationParameters association)
        {
            if (_verifier != null)
            {
                DicomRejectResult result;
                DicomRejectReason reason;
                bool verified = _verifier(_context, association, out result, out reason);
                if (verified == false)
                {
                    server.SendAssociateReject(result,DicomRejectSource.ServiceUser,reason);
                    LogAdapter.Logger.Info("Association rejected from {0} to {1}", 
                        association.CallingAE, association.CalledAE);
                    return;
                }
            }

            // Let the extensions have its say on whether a presentation context is really acceptable.
            //
            bool atLeastOneAccepted = false;
            foreach (byte pcid in association.GetPresentationContextIDs())
            {
                if (association.GetPresentationContextResult(pcid)==DicomPresContextResult.Accept)
                {
                    IDicomScp<TContext> scp = _extensionList[pcid];
                    DicomPresContextResult res = scp.VerifyAssociation(association, pcid);
                    if (res!=DicomPresContextResult.Accept)
                    {
                        association.GetPresentationContext(pcid).ClearTransfers();
                        association.SetPresentationContextResult(pcid, res);
                    }
                    else
                    {
                        atLeastOneAccepted = true;
                    }

                }
            }

            if (!atLeastOneAccepted)
            {
                LogAdapter.Logger.Info("None of the proposed presentation context is accepted. Rejecting association from {0} to {1}", association.CallingAE, association.CalledAE);
                server.SendAssociateReject(DicomRejectResult.Permanent, DicomRejectSource.ServiceUser, DicomRejectReason.NoReasonGiven);
                return;
            }

            server.SendAssociateAccept(association);

			// Optimization to speed query performance
	        Task.Factory.StartNew(() => LogAdapter.Logger.Info("Received association:\r\n{0}", association.ToString()));
        }

	    void IDicomServerHandler.OnReceiveRequestMessage(DicomServer server, ServerAssociationParameters association,
	                                                     byte presentationId, DicomMessage message)
	    {
		    Task.Factory.StartNew(delegate
			    {
				    try
				    {
					    IDicomScp<TContext> scp = _extensionList[presentationId];

					    bool ok = scp.OnReceiveRequest(server, association, presentationId, message);
					    if (!ok)
					    {
                            LogAdapter.Logger.Error("Unexpected error processing message of type {0}.  Aborting association.",
						                 message.SopClass.Name);

						    server.SendAssociateAbort(DicomAbortSource.ServiceProvider, DicomAbortReason.NotSpecified);

					    }
					    else if (_complete != null)
					    {
						    // Only save C-STORE-RQ messages
						    if (message.CommandField == DicomCommandField.CStoreRequest)
							    _instances.Add(new StorageInstance(message));
					    }
				    }
				    catch (Exception x)
				    {
                        LogAdapter.Logger.Error("Unexpected exception in OnReceiveImageLevelQuery.");
						server.SendAssociateAbort(DicomAbortSource.ServiceProvider, DicomAbortReason.NotSpecified);
				    }
			    });
	    }

        void IDicomServerHandler.OnReceiveResponseMessage(DicomServer server, ServerAssociationParameters association, byte presentationID, DicomMessage message)
        {
            LogAdapter.Logger.Error("Unexpectedly received OnReceiveResponseMessage callback from {0} to {1}.  Aborting association.", association.CallingAE, association.CalledAE);
            server.SendAssociateAbort(DicomAbortSource.ServiceUser, DicomAbortReason.UnexpectedPDU);
        }
        
        void IDicomServerHandler.OnReceiveReleaseRequest(DicomServer server, ServerAssociationParameters association)
        {
            LogAdapter.Logger.Info("Received association release request from {0} to {1}.", association.CallingAE, association.CalledAE);
			if (_complete != null)
				_complete(_context, association, _instances);
            Cleanup();
        }

        void IDicomServerHandler.OnReceiveAbort(DicomServer server, ServerAssociationParameters association, DicomAbortSource source, DicomAbortReason reason)
        {
            LogAdapter.Logger.Error("Received association abort from {0} to {1}", association.CallingAE, association.CalledAE);
			if (_complete != null)
				_complete(_context, association, _instances);
            Cleanup();
		}

        void IDicomServerHandler.OnNetworkError(DicomServer server, ServerAssociationParameters association, Exception e)
        {
            LogAdapter.Logger.Error("Unexpectedly received OnNetworkError callback from {0} to {1}.  Aborting association.", association.CallingAE, association.CalledAE);
			if (_complete != null)
				_complete(_context, association, _instances);
            Cleanup();
        }

        void IDicomServerHandler.OnDimseTimeout(DicomServer server, ServerAssociationParameters association)
        {
            // Annoying log
            //LogAdapter.Logger.Info("Unexpected timeout waiting for activity on association from {0} to {1}.", association.CallingAE, association.CalledAE);
        }

		void IDicomServerHandler.OnReceiveDimseCommand(DicomServer server, ServerAssociationParameters association, byte presentationId,
		                           DicomDataset command)
		{
			IDicomScp<TContext> scp = _extensionList[presentationId];

			scp.ReceiveMessageAsFileStream(server, association, presentationId,
			                               new DicomMessage(command, new DicomDataset()));
		}

		IDicomFilestreamHandler IDicomServerHandler.OnStartFilestream(DicomServer server, ServerAssociationParameters association, byte presentationId, DicomMessage message)
		{
			IDicomScp<TContext> scp = _extensionList[presentationId];
			
			if (message.CommandField == DicomCommandField.CStoreRequest)
				_instances.Add(new StorageInstance(message));

			return scp.OnStartFilestream(server, association, presentationId, message);
		}
        
        #endregion
    }
}
