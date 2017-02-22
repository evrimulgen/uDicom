using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIH.Dicom.PACS.Service.Model
{
    public class ServerPartition
    {
        public bool Enable { get; set; }

        public string AeTitle { get; set; }

        public string Description { get; set; }

        public int Port { get; set; }

        public bool AcceptAnyDevice { get; set; }

        public bool AutoInsertDevice { get; set; }
    }
}
