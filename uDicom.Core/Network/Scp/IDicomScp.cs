/////////////////////////////////////////////////////////////////////////
//// Copyright, (c) Shanghai United Imaging Healthcare Inc
//// All rights reserved. 
//// 
//// author: qiuyang.cao@united-imaging.com
////
//// File: IDicomScp.cs
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

using System.Collections.Generic;

namespace UIH.Dicom.Network.Scp
{
    /// <summary>
    /// Simplified interface for DICOM SCPs.
    /// </summary>
    public interface IDicomScp<TContext>
    {
        /// <summary>
        /// Method called by the handler during association verification.
        /// </summary>
        /// <param name="association">Parameters for the association</param>
        /// <param name="pcid">The presentation context being verified</param>
        /// <returns></returns>
        DicomPresContextResult VerifyAssociation(AssociationParameters association, byte pcid);

        /// <summary>
        /// Method called when a request message is being processed.
        /// </summary>
        /// <param name="server">The <see cref="DicomServer"/> instance for the association.</param>
        /// <param name="association">Parameters for the association.</param>
        /// <param name="presentationId">The presentation context for the association.</param>
        /// <param name="message">The message to process.</param>
        /// <returns>true on success, false on failure.</returns>
        bool OnReceiveRequest(DicomServer server, ServerAssociationParameters association, byte presentationId, DicomMessage message);

        /// <summary>
        /// Called when streaming a file.
        /// </summary>
        /// <param name="server">The <see cref="DicomServer"/> instanace for the association.</param>
        /// <param name="association">Parameters for the association.</param>
        /// <param name="presentationId">The presentation context for the association.</param>
        /// <param name="message">The message received.</param>
        /// <returns></returns>
        IDicomFilestreamHandler OnStartFilestream(DicomServer server, ServerAssociationParameters association, byte presentationId, DicomMessage message);

        /// <summary>
        /// Check to determine if a message should be received as a <see cref="DicomFile"/> stream.
        /// </summary>
        /// <param name="server">The <see cref="DicomServer"/> instance for the association.</param>
        /// <param name="association">Parameters for the association.</param>
        /// <param name="presentationId">The presentation context for the association.</param>
        /// <param name="message">The message (with only a CommandSet) to determine if it should be saved as a file.</param>
        /// <returns></returns>
        bool ReceiveMessageAsFileStream(DicomServer server, ServerAssociationParameters association, byte presentationId,
                                        DicomMessage message);

        /// <summary>
        /// Return a list of the DICOM services and transfer syntaxes supported by the interface.
        /// </summary>
        /// <returns></returns>
        IList<SupportedSop> GetSupportedSopClasses();

        /// <summary>
        /// Used to set user specific parameters to be passed to the interface instance.
        /// </summary>
        /// <param name="context">A user specific context for the <see cref="DicomScp{TContext}"/> instance.</param>
        void SetContext(TContext context);

        /// <summary>
        /// Called when an association is closed/aborted/released.  Note that the routine will be called once on the extension.
        /// </summary>
        void Cleanup();
    }
}
