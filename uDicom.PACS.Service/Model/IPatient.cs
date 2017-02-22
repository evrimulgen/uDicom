using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIH.Dicom.PACS.Service.Model
{
    public interface IPatient
    {
        Int32 NumberOfPatientRelatedStudies { get; set; }

        Int32 NumberOfPatientRelatedSeries { get; set; }

        Int32 NumberOfPatientRelatedInstances { get; set; }

        string SpecificCharacterSet { get; set; }

        string PatientsName { get; set; }

        string PatientId { get; set; }

        string IssuerOfPatientId { get; set; }

        List<StudyData> LoadRelatedStudies();
    }
}
