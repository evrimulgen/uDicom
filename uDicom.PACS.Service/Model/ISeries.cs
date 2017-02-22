using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIH.Dicom.PACS.Service.Model
{
    public interface ISeries
    {
        string SeriesInstanceUid { get; set; }

        string Modality { get; set; }

        Int32 NumberOfSeriesRelatedInstances { get; set; }

        string PerformedProcedureStepStartDate { get; set; }

        string PerformedProcedureStepStartTime { get; set; }

        string SourceApplicationEntityTitle { get; set; }

        string SeriesNumber { get; set; }

        string SeriesDescription { get; set; }
    }
}
