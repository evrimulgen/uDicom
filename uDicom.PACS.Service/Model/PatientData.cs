using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIH.Dicom.PACS.Service.Model
{
    public class PatientData
    {
        public Int32 NumberOfPatientRelatedStudies { get; set; }

        public Int32 NumberOfPatientRelatedSeries { get; set; }

        public Int32 NumberOfPatientRelatedInstances { get; set; }

        public string SpecificCharacterSet { get; set; }

        public string PatientsName { get; set; }

        public string PatientId { get; set; }

        public string IssuerOfPatientId { get; set; }

        public virtual List<StudyData> LoadRelatedStudies()
        {
            throw new NotImplementedException();
        }
    }
}
