using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIH.Dicom.PACS.Service.Model
{
    public interface IServerPartition
    {
        bool Enable { get; set; }

        string AeTitle { get; set; }

        string Description { get; set; }

        int Port { get; set; }

        bool AcceptAnyDevice { get; set; }

        bool AutoInsertDevice { get; set; }
    }
}
