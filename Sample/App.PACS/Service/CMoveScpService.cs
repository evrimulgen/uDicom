using System.ComponentModel.Composition;
using UIH.Dicom.Network.Scp;
using UIH.Dicom.PACS.Service;

namespace App.PACS.Service
{
    [Export(typeof(IDicomScp<DicomScpContext>)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class CMoveScpService : CMoveScp
    {
    }
}
