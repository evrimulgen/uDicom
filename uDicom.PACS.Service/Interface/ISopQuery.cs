using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIH.Dicom.PACS.Service.Model;

namespace UIH.Dicom.PACS.Service.Interface
{
    public delegate void SelectCallback<T>(T row);

    public interface ISopQuery
    {
        void OnPatientQuery(DicomMessage message, SelectCallback<IPatientData> callback);

        void OnStudyQuery(DicomMessage message, SelectCallback<IStudyData> callback);

        void OnSeriesQuery(DicomMessage message, SelectCallback<ISeriesData> callback );

    }
}
