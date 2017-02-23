/////////////////////////////////////////////////////////////////////////
//// Copyright, (c) Shanghai United Imaging Healthcare Inc
//// All rights reserved. 
//// 
//// author: qiuyang.cao@united-imaging.com
////
//// File: IDicomServerHandler.cs
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

namespace UIH.Dicom.Network
{
    public interface IDicomFilestreamHandler
    {
        bool SaveStreamData(DicomMessage message, byte[] data, int offset, int count);
        void CancelStream();
        bool CompleteStream(DicomServer server, ServerAssociationParameters assoc, 
            byte presentationId, DicomMessage message);
    }

    public interface IDicomServerHandler
    {
        void OnReceiveAssociateRequest(DicomServer server, ServerAssociationParameters association);
        void OnReceiveRequestMessage(DicomServer server, ServerAssociationParameters association, byte presentationId, DicomMessage message);
        void OnReceiveResponseMessage(DicomServer server, ServerAssociationParameters association, byte presentationId, DicomMessage message);
        void OnReceiveReleaseRequest(DicomServer server, ServerAssociationParameters association);
        void OnReceiveDimseCommand(DicomServer server, ServerAssociationParameters association, byte presentationId, DicomDataset command);

        IDicomFilestreamHandler OnStartFilestream(DicomServer server, ServerAssociationParameters association, byte presentationId, DicomMessage message);

        void OnReceiveAbort(DicomServer server, ServerAssociationParameters association, DicomAbortSource source, DicomAbortReason reason);
        void OnNetworkError(DicomServer server, ServerAssociationParameters association, Exception e);
        void OnDimseTimeout(DicomServer server, ServerAssociationParameters association);
    }
}
