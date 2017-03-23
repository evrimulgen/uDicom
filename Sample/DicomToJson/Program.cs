using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using uDicom.Core.Converter;
using UIH.Dicom;

namespace DicomToJson
{
    class Program
    {
        static void Main(string[] args)
        {
            string workingDir = AppDomain.CurrentDomain.BaseDirectory;
            string dicomFile = Path.Combine(workingDir, "I10") ;

            var dataset = new DicomFile(dicomFile);
            dataset.Load();

            var converter = new JsonDicomConverter();

            string result = converter.Convert(dataset.DataSet);

            string output = Path.Combine(workingDir, "output.json");

            using (var file = new FileStream(output, FileMode.Create))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(result);
                file.Write(bytes, 0, bytes.Length);
            }

            
        }
    }
}
