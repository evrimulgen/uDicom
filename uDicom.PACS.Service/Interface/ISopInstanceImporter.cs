using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIH.Pacs.Services.Dicom;

namespace UIH.Dicom.PACS.Service.Interface
{
    public interface ISopInstanceImporter
    {
        DicomProcessingResult Import(DicomMessageBase message);
    }
}
