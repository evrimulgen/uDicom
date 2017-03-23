using System;
using System.Collections.Concurrent;
using System.Xml;
using UIH.Dicom;

namespace uDicom.Core.Converter
{
    public class XmlDicomWriterService
    {
        static XmlDicomWriterService()
        {
            _vrWriters = new ConcurrentDictionary<string, IDicomXmlVrWriter>();
        }

        public XmlDicomWriterService()
        {
        }

        public XmlDicomWriterService(DicomElement dicomElement)
        {
            DicomElement = dicomElement;
        }

        internal string WriteElement(DicomDataset ds, DicomElement element, XmlWriter writer)
        {
            var vrWriter = GetVrWriter(element);

            return vrWriter.WriteElement(element, writer);
        }

        private IDicomXmlVrWriter GetVrWriter(DicomElement element)
        {
            return _vrWriters.GetOrAdd(element.Tag.VR.Name, CreateDefualtVrWriter(element.Tag.VR));
        }

        protected virtual IDicomXmlVrWriter CreateDefualtVrWriter(DicomVr dicomVr)
        {
            IDicomXmlVrWriter writer = null;

            if (!_defaultVrWriters.TryGetValue(dicomVr.Name, out writer))
            {
                throw new ApplicationException("Default VR writer not registered!");
            }

            return writer;
        }

        public DicomElement DicomElement { get; set; }


        private static readonly ConcurrentDictionary<string, IDicomXmlVrWriter> _vrWriters;
        private static ConcurrentDictionary<string, IDicomXmlVrWriter> _defaultVrWriters;
    }
}