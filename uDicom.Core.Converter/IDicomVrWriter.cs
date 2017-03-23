using System.Xml;
using UIH.Dicom;

namespace uDicom.Core.Converter
{
    public interface IDicomVrWriter<T, N>
    {
        T WriteElement(DicomElement element, N writer);
    }

    public interface IDicomXmlVrWriter : IDicomVrWriter<string, XmlWriter>
    {
        //        string WriteElement ( DicomAttribute element, XmlWriter writer ) ;
    }
}