using UIH.Dicom.PACS.Service.Model;

namespace UIH.Dicom.PACS.Service.Interface
{
    public class SopInstanceImporterContext
    {
        #region Private Member

        private readonly string _contextId;

        private readonly string _sourceAe;

        private readonly string _destAe;

        #endregion

        #region Construtor

        /// <summary>
        /// create a instance of <see cref="SopInstanceImporterContext" /> to be used 
        /// by <see cref="SopInstanceImporter"/>
        /// </summary>
        /// <param name="sourceAE"></param>
        /// <param name="destAe">source ae title of the image(s) to be import</param>
        /// <param name="destAe">which the image(s) will be import to</param>
        public SopInstanceImporterContext(string contextID, string sourceAE, string destAe)
        {
            _contextId = contextID;
            _sourceAe = sourceAE;
            _destAe = destAe;
        }

        #endregion

        /// <summary>
        /// Gets the ID of this context
        /// </summary>
        public string ContextID
        {
            get { return _contextId; }
        }

        /// <summary>
        /// Gets the source AE title where the image(s) are imported from
        /// </summary>
        public string SourceAE
        {
            get { return _sourceAe; }
        }

        /// <summary>
        /// Gets <see cref="ServerPartition"/> where the image(s) will be imported to
        /// </summary>
        public string DestAE
        {
            get { return _destAe; }
        }
    }
}