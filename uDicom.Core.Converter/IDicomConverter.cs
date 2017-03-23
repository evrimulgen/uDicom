using UIH.Dicom;

namespace uDicom.Core.Converter
{
    public interface IDicomConverter<out T>
    {
        T Convert(DicomDataset dicom);
    }
}