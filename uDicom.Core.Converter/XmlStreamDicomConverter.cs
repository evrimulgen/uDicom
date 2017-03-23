using System.IO;
using System.Text;
using UIH.Dicom;

namespace uDicom.Core.Converter
{
    public interface IXmlStreamDicomConverter : IDicomConverter<Stream>
    { }

    public class XmlStreamDicomConverter : IXmlStreamDicomConverter
    {
        public XmlStreamDicomConverter() : this(new XmlDicomConverter())
        { }

        public XmlStreamDicomConverter(IXmlDicomConverter xmlconverter)
        {
            XmlConverter = xmlconverter;
        }

        public Stream Convert(DicomDataset ds)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(XmlConverter.Convert(ds)));
        }

        public IXmlDicomConverter XmlConverter { get; set; }
    }
}