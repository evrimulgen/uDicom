using System;

namespace UIH.Dicom.PACS.Service.Model
{
    public abstract class StudyData
    {
        public string StudyInstanceUid { get; set; }

        public Int32 NumberOfStudyRelatedSeries { get; set; }

        public Int32 NumberOfStudyRelatedInstances { get; set; }

        public string PatientsName { get; set; }

        public string PatientId { get; set; }

        public string PatientsBirthDate { get; set; }

        public string PatientsAge { get; set; }

        public string PatientsSex { get; set; }

        public string StudyDate { get; set; }

        public string StudyTime { get; set; }

        public string AccessionNumber { get; set; }

        public string StudyId { get; set; }

        public string StudyDescription { get; set; }

        public string ReferringPhysiciansName { get; set; }

        public string SpecificCharacterSet { get; set; }

        public static StudyData Load(string uid)
        {
            throw new NotImplementedException();
        }

    }
}
