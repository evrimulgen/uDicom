using System.ComponentModel.DataAnnotations;

namespace UIH.Dicom.PACS.Service.Model
{
    public class Device
    {
        public string AETitle { get; set; }

        public string IpAddress { get; set; }

        public int Port { get; set; }

        public bool AllowStorage { get; set; }

        public bool AllowRetrive { get; set; }

        public bool AllowQuery { get; set; }

        public bool Enable { get; set; }

        public string Decripation { get; set; }
    }
}
