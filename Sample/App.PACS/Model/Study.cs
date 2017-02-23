#region License

// 
// Copyright (c) 2011 - 2012, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using System;
using System.Collections.Generic;
using UIH.Dicom.PACS.Service.Model;

namespace App.PACS.Model
{
    public partial class Study : IStudyData
    {
        public int Id { get; set; }

        // Foreign Key to Patient Primary Key 
        public int PatientForeignKey { get; set; }

        public string StudyUid { get; set; }

        public string StudyId { get; set; }

        public string AccessionNumber { get; set; }

        public string StudyDate { get; set; }

        public string StudyTime { get; set; }

        public string ModaliyInStudy { get; set; }

        public string RefPhysician { get; set; }

        public string StudyDescription { get; set; }

        #region Study-Patient 

        public string PatientId { get; set; }

        public string PatientName { get; set; }

        public string PatientBirthday { get; set; }

        public string PatientAge { get; set; }

        public string PatientSize { get; set; }

        public string PatientWeight { get; set; }

        public string PatientSex { get; set; }

        #endregion

        public int NumberOfRelatedSeries { get; set; }

        public int NumberOfRelatedImage { get; set; }

        public DateTime InsertTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public virtual Patient Patient { get; set; }

        public virtual ICollection<Series> Series { get; set; }

        public IStudyData Load(string uid)
        {
            throw new NotImplementedException();
        }

    }
}