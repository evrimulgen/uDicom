using System;

namespace UIH.Dicom.PACS.Service.Model
{
    public interface IStudyData
    {
        string StudyUid { get; set; }

        Int32 NumberOfRelatedSeries { get; set; }

        Int32 NumberOfRelatedImage { get; set; }

        string PatientName { get; set; }

        string PatientId { get; set; }

        string PatientBirthday { get; set; }

        string PatientAge { get; set; }

        string PatientSex { get; set; }

        string StudyDate { get; set; }

        string StudyTime { get; set; }

        string AccessionNumber { get; set; }

        string StudyId { get; set; }

        string StudyDescription { get; set; }

        string RefPhysician { get; set; }

        //string SpecificCharacterSet { get; set; }

        IStudyData Load(string uid);


    }
}
