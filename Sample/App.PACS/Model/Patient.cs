using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using UIH.Dicom.PACS.Service.Model;

namespace App.PACS.Model
{
    public partial class Patient : IPatientData
    {
        public int Id { get; set; }

        public string PatientId { get; set; }

        public string PatientName { get; set; }

        public string IssuerOfPatientId { get; set; }
        
        public string PatientBirthDate { get; set; }

        public int NumberOfRelatedStudies { get; set; }

        public int NumberOfRelatedSeries { get; set; }

        public int NumberOfRelatedInstances { get; set; }

        public DateTime InsertTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public virtual ICollection<Study> Studies { get; set; }

        public List<IStudyData> LoadRelatedStudies()
        {
            return this.Studies.Cast<IStudyData>().ToList();
        }
    }
}