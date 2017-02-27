using System.Collections.Generic;
using UIH.Dicom.Network;
using UIH.Pacs.Services.Dicom;

namespace UIH.Dicom.PACS.Service.Interface
{
    public interface ISopInstanceImporter
    {
        SopInstanceImporterContext Context { get; set; }

        #region interface 

        IList<SupportedSop> GetStroageSupportedSopClasses();

        DicomProcessingResult Import(DicomMessage message);

        DicomProcessingResult ImportFile(DicomMessage message, string filename);

        bool GetStreamedFileStorageFolder(DicomMessage message, out string sourceFolder,
            out string filesystemStreamingFolder);

        #endregion-

    }
}
