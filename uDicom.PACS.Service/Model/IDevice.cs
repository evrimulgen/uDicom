using System.ComponentModel.DataAnnotations;

namespace UIH.Dicom.PACS.Service.Model
{
    public interface IDevice
    {
        string AeTitle { get; set; }

        string Hostname { get; set; }

        int Port { get; set; }

        bool AllowStorage { get; set; }

        bool AllowRetrieve { get; set; }

        bool AllowQuery { get; set; }

        bool Enabled { get; set; }

        bool Dhcp { get; set; }

        string Description { get; set; }
    }
}
