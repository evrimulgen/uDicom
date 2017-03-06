using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIH.Dicom.PACS.Service.Model
{
    public interface ISeriesData
    {
        string SeriesUid { get; set; }

        string Modality { get; set; }

        Int32 NumberOfRelatedImage { get; set; }

        DateTime PerformedProcedureStepStartDate { get; set; }

        DateTime PerformedProcedureStepStartTime { get; set; }

        string SourceAet { get; set; }

        string SeriesNumber { get; set; }

        string SeriesDescription { get; set; }
    }
}
