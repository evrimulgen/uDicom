#region License

// 
// Copyright (c) 2011 - 2012, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using System;
using UIH.Dicom.Network;

namespace UIH.Pacs.Services.Dicom
{
    public class DicomProcessingResult
    {
        public string AccessionNumber;
        public string StudyInstanceUid;
        public string SeriesInstanceUid;
        public string SopInstanceUid;
        public bool Successful;
        public string ErrorMessage;
        public DicomStatus DicomStatus;
        public bool RestoreRequested;

        /// <summary>
        /// Indicates whether the sop being processed is a duplicate.
        /// </summary>
        /// <remarks>
        /// The result of the processing depends on the duplicate policy used.
        /// </remarks>
        public bool Duplicate;

        public void SetError(DicomStatus status, string message)
        {
            Successful = false;
            DicomStatus = status;
            ErrorMessage = message;
        }
    }
}