using System.ComponentModel.Composition;
using UIH.Dicom.Network.Scp;
using UIH.Dicom.PACS.Service;

namespace App.PACS.Service
{
    // If you want to support C-Find SCP, inherit from CFindScp and Export by MEF
    [Export(typeof(IDicomScp<DicomScpContext>)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class CFindScpService : CFindScp
    {
    }
}
