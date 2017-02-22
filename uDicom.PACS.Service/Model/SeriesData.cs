using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIH.Dicom.PACS.Service.Model
{
    public class SeriesData
    {
        public string SeriesInstanceUid { get; set; }

        public string Modality { get; set; }

        public Int32 NumberOfSeriesRelatedInstances { get; set; }

        public string PerformedProcedureStepStartDate { get; set; }

        public string PerformedProcedureStepStartTime { get; set; }

        public string SourceApplicationEntityTitle { get; set; }

        public string SeriesNumber { get; set; }

        public string SeriesDescription { get; set; }
    }
}
