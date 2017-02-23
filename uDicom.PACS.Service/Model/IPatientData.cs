using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIH.Dicom.PACS.Service.Model
{
    public interface IPatientData
    {
        Int32 NumberOfRelatedStudies { get; set; }

        Int32 NumberOfRelatedSeries { get; set; }

        Int32 NumberOfRelatedInstances { get; set; }

        //string SpecificCharacterSet { get; set; }

        string PatientName { get; set; }

        string PatientId { get; set; }

        string IssuerOfPatientId { get; set; }

        List<IStudyData> LoadRelatedStudies();

    }
}
