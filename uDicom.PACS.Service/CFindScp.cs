#region License

// 
// Copyright (c) 2011 - 2012, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using UIH.Dicom.Network;
using UIH.Dicom.Network.Scp;
using UIH.Dicom.PACS.Service.Interface;
using UIH.Dicom.PACS.Service.Model;

namespace UIH.Dicom.PACS.Service
{
    [Export(typeof (IDicomScp<DicomScpContext>))]
    public class CFindScp : BaseScp
    {
        #region Private members

        private readonly List<SupportedSop> _list = new List<SupportedSop>();
        private bool _cancelReceived;
        private readonly Queue<DicomMessage> _responseQueue = 
            new Queue<DicomMessage>(10);

        private readonly int _bufferedQueryResponses = 10;
        private readonly int _maxQueryResponses = 500;
        private readonly bool _cfindRspAlwaysInUnicode = true;

        private readonly object _syncLock = new object();

        #endregion

        #region Public Properties

        public bool CancelReceived
        {
            get
            {
                lock (_syncLock)
                    return _cancelReceived;
            }
            set
            {
                lock (_syncLock)
                    _cancelReceived = value;
            }
        }

        #endregion

        #region Contructors

		/// <summary>
        /// Public default constructor.  Implements the Find and Move services for 
        /// PatientData Root and Study Root queries.
        /// </summary>
        public CFindScp()
		{
		    var sop = new SupportedSop
		                  {
		                      SopClass = SopClass.PatientRootQueryRetrieveInformationModelFind
		                  };
			sop.SyntaxList.Add(TransferSyntax.ExplicitVrLittleEndian);
            sop.SyntaxList.Add(TransferSyntax.ImplicitVrLittleEndian);
            _list.Add(sop);

            sop = new SupportedSop
                  	{
                  		SopClass = SopClass.StudyRootQueryRetrieveInformationModelFind
                  	};
			sop.SyntaxList.Add(TransferSyntax.ExplicitVrLittleEndian);
            sop.SyntaxList.Add(TransferSyntax.ImplicitVrLittleEndian);
            _list.Add(sop);
            
		}

        #endregion

        #region Private Method

        private void OnReceivePatientQuery(DicomServer server, byte presentationId, DicomMessage message)
        {
            var tagList = message.DataSet.Select(attrib => attrib.Tag).ToList();

            ISopQuery query = IoC.Get<ISopQuery>();

            int resultCount = 0;

            try
            {
                query.OnPatientQuery(message, delegate(PatientData row)
                {
                    if (CancelReceived)
                        throw new DicomException("DICOM C-Cancel Received");

                    resultCount++;
                    if (_maxQueryResponses != -1
                        && _maxQueryResponses < resultCount)
                    {
                        SendBufferedResponses(server, presentationId, message);
                        throw new DicomException("Maximum Configured Query Responses Exceeded: " + resultCount);
                    }

                    var response = new DicomMessage();
                    PopulatePatient(response, tagList, row);
                    _responseQueue.Enqueue(response);

                    if (_responseQueue.Count >= _bufferedQueryResponses)
                        SendBufferedResponses(server, presentationId, message);
                });
            }
            catch (Exception)
            {
                if (CancelReceived)
                {
                    var errorResponse = new DicomMessage();
                    server.SendCFindResponse(presentationId, message.MessageId, errorResponse,
                                             DicomStatuses.Cancel);
                }
                else if (_maxQueryResponses != -1
                      && _maxQueryResponses < resultCount)
                {
                    Log.Logger.Warn("Maximum Configured Query Responses Exceeded: {0} on query from {1}", 
                        resultCount, server.AssociationParams.CallingAE);
                    var errorResponse = new DicomMessage();
                    server.SendCFindResponse(presentationId, message.MessageId, errorResponse,
                                             DicomStatuses.Success);
                   
                }
                else
                {
                    Log.Logger.Error("Unexpected exception when processing FIND request.");
                    var errorResponse = new DicomMessage();
                    server.SendCFindResponse(presentationId, message.MessageId, errorResponse,
                                             DicomStatuses.QueryRetrieveUnableToProcess);

                }
                return;
            }

            var finalResponse = new DicomMessage();
            server.SendCFindResponse(presentationId, message.MessageId, finalResponse, DicomStatuses.Success);

            return;
        }

        private void OnReceiveStudyLevelQuery(DicomServer server, byte presentationId, DicomMessage message)
        {
            var tagList = message.DataSet.Select(attrib => attrib.Tag).ToList();

            ISopQuery query = IoC.Get<ISopQuery>();

            int resultCount = 0;

            try
            {
                query.OnStudyQuery(message, delegate (StudyData row)
                {
                    if (CancelReceived)
                        throw new DicomException("DICOM C-Cancel Received");

                    resultCount++;
                    if (_maxQueryResponses != -1
                        && _maxQueryResponses < resultCount)
                    {
                        SendBufferedResponses(server, presentationId, message);
                        throw new DicomException("Maximum Configured Query Responses Exceeded: " + resultCount);
                    }

                    var response = new DicomMessage();
                    PopulateStudy(response, tagList, row);
                    _responseQueue.Enqueue(response);

                    if (_responseQueue.Count >= _bufferedQueryResponses)
                        SendBufferedResponses(server, presentationId, message);
                });
            }
            catch (Exception)
            {
                if (CancelReceived)
                {
                    var errorResponse = new DicomMessage();
                    server.SendCFindResponse(presentationId, message.MessageId, errorResponse,
                                             DicomStatuses.Cancel);
                }
                else if (_maxQueryResponses != -1
                      && _maxQueryResponses < resultCount)
                {
                    Log.Logger.Warn("Maximum Configured Query Responses Exceeded: {0} on query from {1}",
                        resultCount, server.AssociationParams.CallingAE);
                    var errorResponse = new DicomMessage();
                    server.SendCFindResponse(presentationId, message.MessageId, errorResponse,
                                             DicomStatuses.Success);

                }
                else
                {
                    Log.Logger.Error("Unexpected exception when processing FIND request.");
                    var errorResponse = new DicomMessage();
                    server.SendCFindResponse(presentationId, message.MessageId, errorResponse,
                                             DicomStatuses.QueryRetrieveUnableToProcess);

                }
                return;
            }

            var finalResponse = new DicomMessage();
            server.SendCFindResponse(presentationId, message.MessageId, finalResponse, DicomStatuses.Success);

            return;
        }

        private void OnReceiveSeriesLevelQuery(DicomServer server, byte presentationId, DicomMessage message)
        {
            var tagList = message.DataSet.Select(attrib => attrib.Tag).ToList();

            ISopQuery query = IoC.Get<ISopQuery>();

            int resultCount = 0;

            try
            {
                query.OnSeriesQuery(message, delegate (SeriesData row)
                {
                    if (CancelReceived)
                        throw new DicomException("DICOM C-Cancel Received");

                    resultCount++;
                    if (_maxQueryResponses != -1
                        && _maxQueryResponses < resultCount)
                    {
                        SendBufferedResponses(server, presentationId, message);
                        throw new DicomException("Maximum Configured Query Responses Exceeded: " + resultCount);
                    }

                    var response = new DicomMessage();
                    PopulateSeries(message, response, tagList,  row);
                    _responseQueue.Enqueue(response);

                    if (_responseQueue.Count >= _bufferedQueryResponses)
                        SendBufferedResponses(server, presentationId, message);
                });
            }
            catch (Exception)
            {
                if (CancelReceived)
                {
                    var errorResponse = new DicomMessage();
                    server.SendCFindResponse(presentationId, message.MessageId, errorResponse,
                                             DicomStatuses.Cancel);
                }
                else if (_maxQueryResponses != -1
                      && _maxQueryResponses < resultCount)
                {
                    Log.Logger.Warn("Maximum Configured Query Responses Exceeded: {0} on query from {1}",
                        resultCount, server.AssociationParams.CallingAE);
                    var errorResponse = new DicomMessage();
                    server.SendCFindResponse(presentationId, message.MessageId, errorResponse,
                                             DicomStatuses.Success);

                }
                else
                {
                    Log.Logger.Error("Unexpected exception when processing FIND request.");
                    var errorResponse = new DicomMessage();
                    server.SendCFindResponse(presentationId, message.MessageId, errorResponse,
                                             DicomStatuses.QueryRetrieveUnableToProcess);

                }
                return;
            }

            var finalResponse = new DicomMessage();
            server.SendCFindResponse(presentationId, message.MessageId, finalResponse, DicomStatuses.Success);

            return;
        }

        /*
        private void OnReceiveImageLevelQuery(DicomServer server, byte presentationId, DicomMessage message)
        {
            var tagList = new List<DicomTag>();
            var matchingTagList = new List<uint>();

            DicomDataset data = message.DataSet;
            string studyInstanceUid = data[DicomTags.StudyInstanceUid].GetString(0, String.Empty);
            string seriesInstanceUid = data[DicomTags.SeriesInstanceUid].GetString(0, String.Empty);
        }
        */
        
        private void SendBufferedResponses(DicomServer server, byte presentationId, DicomMessage requestMessage)
        {
            while (_responseQueue.Count > 0)
            {
                DicomMessage response = _responseQueue.Dequeue();
                server.SendCFindResponse(presentationId, requestMessage.MessageId, response,
                         DicomStatuses.Pending);

                if (CancelReceived)
                    throw new DicomException("DICOM C-Cancel Received");

                if (!server.NetworkActive)
                    throw new DicomException("Association is no longer valid.");
            }
        }

        private void PopulatePatient(DicomMessageBase response, IEnumerable<DicomTag> tagList, PatientData row)
        {
            DicomDataset dataSet = response.DataSet;

            dataSet[DicomTags.RetrieveAeTitle].SetStringValue(Partition.AeTitle);

            var characterSet = GetPreferredCharacterSet();
            if (!string.IsNullOrEmpty(characterSet))
            {
                dataSet[DicomTags.SpecificCharacterSet].SetStringValue(characterSet);
                dataSet.SpecificCharacterSet = characterSet;
            }

            IList<StudyData> relatedStudies = row.LoadRelatedStudies();
            StudyData studyData = null;
            if (relatedStudies.Count > 0)
                studyData = relatedStudies.First();

            foreach (DicomTag tag in tagList)
            {
                try
                {
                    switch (tag.TagValue)
                    {
                        case DicomTags.PatientsName:
                            dataSet[DicomTags.PatientsName].SetStringValue(row.PatientsName);
                            break;
                        case DicomTags.PatientId:
                            dataSet[DicomTags.PatientId].SetStringValue(row.PatientId);
                            break;
                        case DicomTags.IssuerOfPatientId:
                            dataSet[DicomTags.IssuerOfPatientId].SetStringValue(row.IssuerOfPatientId);
                            break;
                        case DicomTags.NumberOfPatientRelatedStudies:
                            dataSet[DicomTags.NumberOfPatientRelatedStudies].AppendInt32(row.NumberOfPatientRelatedStudies);
                            break;
                        case DicomTags.NumberOfPatientRelatedSeries:
                            dataSet[DicomTags.NumberOfPatientRelatedSeries].AppendInt32(row.NumberOfPatientRelatedSeries);
                            break;
                        case DicomTags.NumberOfPatientRelatedInstances:
                            dataSet[DicomTags.NumberOfPatientRelatedInstances].AppendInt32(
                                row.NumberOfPatientRelatedInstances);
                            break;
                        case DicomTags.QueryRetrieveLevel:
                            dataSet[DicomTags.QueryRetrieveLevel].SetStringValue("PATIENT");
                            break;
                        case DicomTags.PatientsSex:
                            if (studyData == null)
                                dataSet[DicomTags.PatientsSex].SetNullValue();
                            else
                                dataSet[DicomTags.PatientsSex].SetStringValue(studyData.PatientsSex);
                            break;
                        case DicomTags.PatientsBirthDate:
                            if (studyData == null)
                                dataSet[DicomTags.PatientsBirthDate].SetNullValue();
                            else
                                dataSet[DicomTags.PatientsBirthDate].SetStringValue(studyData.PatientsBirthDate);
                            break;

                        // Meta tags that should have not been in the RQ, but we've already set
                        case DicomTags.RetrieveAeTitle:
                        case DicomTags.InstanceAvailability:
                        case DicomTags.SpecificCharacterSet:
                            break;

                        default:
                            if (!tag.IsPrivate)
                                dataSet[tag].SetNullValue();

                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.Warn("Unexpected error setting tag {0} in C-FIND-RSP",
                                 dataSet[tag].Tag.ToString());
                    Log.Logger.TraceException(e);
                    if (!tag.IsPrivate)
                        dataSet[tag].SetNullValue();
                }
            }

        }

        private void PopulateStudy(DicomMessageBase response, IEnumerable<DicomTag> tagList, StudyData row)
        {
            DicomDataset dataSet = response.DataSet;

            dataSet[DicomTags.RetrieveAeTitle].SetStringValue(Partition.AeTitle);

            dataSet[DicomTags.InstanceAvailability].SetStringValue("ONLINE");

            var characterSet = GetPreferredCharacterSet();
            if (!string.IsNullOrEmpty(characterSet))
            {
                dataSet[DicomTags.SpecificCharacterSet].SetStringValue(characterSet);
                dataSet.SpecificCharacterSet = characterSet;
            }
            
            foreach (DicomTag tag in tagList)
            {
                try
                {
                    switch (tag.TagValue)
                    {
                        case DicomTags.StudyInstanceUid:
                            dataSet[DicomTags.StudyInstanceUid].SetStringValue(row.StudyInstanceUid);
                            break;
                        case DicomTags.PatientsName:
                            dataSet[DicomTags.PatientsName].SetStringValue(row.PatientsName);
                            break;
                        case DicomTags.PatientId:
                            dataSet[DicomTags.PatientId].SetStringValue(row.PatientId);
                            break;
                        case DicomTags.PatientsBirthDate:
                            dataSet[DicomTags.PatientsBirthDate].SetStringValue(row.PatientsBirthDate);
                            break;
                        case DicomTags.PatientsAge:
                            dataSet[DicomTags.PatientsAge].SetStringValue(row.PatientsAge);
                            break;
                        case DicomTags.PatientsSex:
                            dataSet[DicomTags.PatientsSex].SetStringValue(row.PatientsSex);
                            break;
                        case DicomTags.StudyDate:
                            dataSet[DicomTags.StudyDate].SetStringValue(row.StudyDate);
                            break;
                        case DicomTags.StudyTime:
                            dataSet[DicomTags.StudyTime].SetStringValue(row.StudyTime);
                            break;
                        case DicomTags.AccessionNumber:
                            dataSet[DicomTags.AccessionNumber].SetStringValue(row.AccessionNumber);
                            break;
                        case DicomTags.StudyId:
                            dataSet[DicomTags.StudyId].SetStringValue(row.StudyId);
                            break;
                        case DicomTags.StudyDescription:
                            dataSet[DicomTags.StudyDescription].SetStringValue(row.StudyDescription);
                            break;
                        case DicomTags.ReferringPhysiciansName:
                            dataSet[DicomTags.ReferringPhysiciansName].SetStringValue(row.ReferringPhysiciansName);
                            break;
                        case DicomTags.NumberOfStudyRelatedSeries:
                            dataSet[DicomTags.NumberOfStudyRelatedSeries].AppendInt32(row.NumberOfStudyRelatedSeries);
                            break;
                        case DicomTags.NumberOfStudyRelatedInstances:
                            dataSet[DicomTags.NumberOfStudyRelatedInstances].AppendInt32(
                                row.NumberOfStudyRelatedInstances);
                            break;
                        case DicomTags.ModalitiesInStudy:
                            //LoadModalitiesInStudy(read, response, row.Key);
                            break;
                        case DicomTags.QueryRetrieveLevel:
                            dataSet[DicomTags.QueryRetrieveLevel].SetStringValue("STUDY");
                            break;
                        // Meta tags that should have not been in the RQ, but we've already set
                        case DicomTags.RetrieveAeTitle:
                        case DicomTags.InstanceAvailability:
                        case DicomTags.SpecificCharacterSet:
                            break;
                        default:
                            if (!tag.IsPrivate)
                                dataSet[tag].SetNullValue();

                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.Warn("Unexpected error setting tag {0} in C-FIND-RSP",
                                 dataSet[tag].Tag.ToString());
                    if (!tag.IsPrivate)
                        dataSet[tag].SetNullValue();
                }
            }
        }

        private void PopulateSeries(DicomMessageBase request, DicomMessageBase response, IEnumerable<DicomTag> tagList,
                                    SeriesData row)
        {
            DicomDataset dataSet = response.DataSet;

            dataSet[DicomTags.RetrieveAeTitle].SetStringValue(Partition.AeTitle);
            dataSet[DicomTags.InstanceAvailability].SetStringValue("ONLINE");

            var characterSet = GetPreferredCharacterSet();
            if (!string.IsNullOrEmpty(characterSet))
            {
                dataSet[DicomTags.SpecificCharacterSet].SetStringValue(characterSet);
                dataSet.SpecificCharacterSet = characterSet;
            }

            foreach (DicomTag tag in tagList)
            {
                try
                {
                    switch (tag.TagValue)
                    {
                        case DicomTags.PatientId:
                            dataSet[DicomTags.PatientId].SetStringValue(request.DataSet[DicomTags.PatientId].ToString());
                            break;
                        case DicomTags.StudyInstanceUid:
                            dataSet[DicomTags.StudyInstanceUid].SetStringValue(
                                request.DataSet[DicomTags.StudyInstanceUid].ToString());
                            break;
                        case DicomTags.SeriesInstanceUid:
                            dataSet[DicomTags.SeriesInstanceUid].SetStringValue(row.SeriesInstanceUid);
                            break;
                        case DicomTags.Modality:
                            dataSet[DicomTags.Modality].SetStringValue(row.Modality);
                            break;
                        case DicomTags.SeriesNumber:
                            dataSet[DicomTags.SeriesNumber].SetStringValue(row.SeriesNumber);
                            break;
                        case DicomTags.SeriesDescription:
                            dataSet[DicomTags.SeriesDescription].SetStringValue(row.SeriesDescription);
                            break;
                        case DicomTags.PerformedProcedureStepStartDate:
                            dataSet[DicomTags.PerformedProcedureStepStartDate].SetStringValue(
                                row.PerformedProcedureStepStartDate);
                            break;
                        case DicomTags.PerformedProcedureStepStartTime:
                            dataSet[DicomTags.PerformedProcedureStepStartTime].SetStringValue(
                                row.PerformedProcedureStepStartTime);
                            break;
                        case DicomTags.NumberOfSeriesRelatedInstances:
                            dataSet[DicomTags.NumberOfSeriesRelatedInstances].AppendInt32(row.NumberOfSeriesRelatedInstances);
                            break;
                        case DicomTags.RequestAttributesSequence:
                            //LoadRequestAttributes(read, response, row);
                            break;
                        case DicomTags.QueryRetrieveLevel:
                            dataSet[DicomTags.QueryRetrieveLevel].SetStringValue("SERIES");
                            break;
                        // Meta tags that should have not been in the RQ, but we've already set
                        case DicomTags.RetrieveAeTitle:
                        case DicomTags.InstanceAvailability:
                        case DicomTags.SpecificCharacterSet:
                            break;
                        default:
                            if (!tag.IsPrivate)
                                dataSet[tag].SetNullValue();


                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.Warn("Unexpected error setting tag {0} in C-FIND-RSP",
                                 dataSet[tag].Tag.ToString());
                    if (!tag.IsPrivate)
                        dataSet[tag].SetNullValue();
                }
            }
        }

        /*
        private void PopulateInstance(DicomMessageBase request, DicomMessageBase response, IEnumerable<DicomTag> tagList,
                                      InstanceXml theInstanceStream)
        {
            DicomDataset dataSet = response.DataSet;

            dataSet[DicomTags.RetrieveAeTitle].SetStringValue(Partition.AeTitle);
            dataSet[DicomTags.InstanceAvailability].SetStringValue("ONLINE");

            DicomDataset sourceDataSet = theInstanceStream.Collection;

            var characterSet = GetPreferredCharacterSet();
            if (!string.IsNullOrEmpty(characterSet))
            {
                dataSet[DicomTags.SpecificCharacterSet].SetStringValue(characterSet);
                dataSet.SpecificCharacterSet = characterSet;
            }
            else if (sourceDataSet.Contains(DicomTags.SpecificCharacterSet))
            {
                dataSet[DicomTags.SpecificCharacterSet].SetStringValue(sourceDataSet[DicomTags.SpecificCharacterSet].ToString());
                dataSet.SpecificCharacterSet = sourceDataSet[DicomTags.SpecificCharacterSet].ToString(); // this will ensure the data is encoded using the specified character set
            }

            foreach (DicomTag tag in tagList)
            {
                try
                {
                    switch (tag.TagValue)
                    {
                        case DicomTags.PatientId:
                            dataSet[DicomTags.PatientId].SetStringValue(request.DataSet[DicomTags.PatientId].ToString());
                            break;
                        case DicomTags.StudyInstanceUid:
                            dataSet[DicomTags.StudyInstanceUid].SetStringValue(
                                request.DataSet[DicomTags.StudyInstanceUid].ToString());
                            break;
                        case DicomTags.SeriesInstanceUid:
                            dataSet[DicomTags.SeriesInstanceUid].SetStringValue(
                                request.DataSet[DicomTags.SeriesInstanceUid].ToString());
                            break;
                        case DicomTags.QueryRetrieveLevel:
                            dataSet[DicomTags.QueryRetrieveLevel].SetStringValue("IMAGE");
                            break;
                        default:
                            if (sourceDataSet.Contains(tag))
                                dataSet[tag] = sourceDataSet[tag].Copy();
                            else if (!tag.IsPrivate)
                                dataSet[tag].SetNullValue();
                            break;
                        // Meta tags that should have not been in the RQ, but we've already set
                        case DicomTags.RetrieveAeTitle:
                        case DicomTags.InstanceAvailability:
                        case DicomTags.SpecificCharacterSet:
                            break;
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.Warn("Unexpected error setting tag {0} in C-FIND-RSP",
                                 dataSet[tag].Tag.ToString());
                    Log.Logger.TraceException(e);
                    if (!tag.IsPrivate)
                        dataSet[tag].SetNullValue();
                }
            }
        }

        */

        /// <summary>
        /// Get the prefered character set for the C-FIND-RSP.
        /// </summary>
        /// <returns></returns>
        private string GetPreferredCharacterSet()
        {
            if (_cfindRspAlwaysInUnicode)
                return "ISO_IR 192";
            return null;
        }


        #endregion


        #region IDicomScp Members

        /// <summary>
        /// Extension method called when a new DICOM Request message has been called that the 
        /// extension will process.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="association"></param>
        /// <param name="presentationId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool OnReceiveRequest(DicomServer server, ServerAssociationParameters association,
                                              byte presentationId, DicomMessage message)
        {
            String level = message.DataSet[DicomTags.QueryRetrieveLevel].GetString(0, string.Empty);

            if (message.CommandField == DicomCommandField.CCancelRequest)
            {
                Log.Logger.Info("Received C-FIND-CANCEL-RQ message.");
                CancelReceived = true;
                return true;
            }

            CancelReceived = false;

            if (message.AffectedSopClassUid.Equals(SopClass.StudyRootQueryRetrieveInformationModelFindUid))
            {
                if (level.Equals("STUDY"))
                {
                    // We use the ThreadPool to process the thread requests. This is so that we return back
                    // to the main message loop, and continue to look for cancel request messages coming
                    // in.  There's a small chance this may cause delays in responding to query requests if
                    // the .NET Thread pool fills up.
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        try
                        {
                            OnReceiveStudyLevelQuery(server, presentationId, message);
                        }
                        catch (Exception x)
                        {
                            Log.Logger.Error("Unexpected exception in OnReceiveStudyLevelQuery.", x);
                        }
                    });
                    return true;
                }
                if (level.Equals("SERIES"))
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        try
                        {
                            OnReceiveSeriesLevelQuery(server, presentationId, message);
                        }
                        catch (Exception x)
                        {
                            Log.Logger.Error("Unexpected exception in OnReceiveSeriesLevelQuery.", x);
                        }
                    });
                    return true;
                }

                /*
                if (level.Equals("IMAGE"))
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        try
                        {
                            OnReceiveImageLevelQuery(server, presentationId, message);
                        }
                        catch (Exception x)
                        {
                            Log.Logger.Error("Unexpected exception in OnReceiveImageLevelQuery.", x);
                        }
                    });
                    return true;
                }
                */

                Log.Logger.Error("Unexpected Study Root Query/Retrieve level: {0}", level);

                server.SendCFindResponse(presentationId, message.MessageId, new DicomMessage(),
                                         DicomStatuses.QueryRetrieveIdentifierDoesNotMatchSOPClass);
                return true;
            }

            if (message.AffectedSopClassUid.Equals(SopClass.PatientRootQueryRetrieveInformationModelFindUid))
            {
                if (level.Equals("PATIENT"))
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        try
                        {
                            OnReceivePatientQuery(server, presentationId, message);
                        }
                        catch (Exception x)
                        {
                            Log.Logger.Error("Unexpected exception in OnReceivePatientQuery.", x);
                            
                        }
                    });

                    return true;
                }
                if (level.Equals("STUDY"))
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        try
                        {
                            OnReceiveStudyLevelQuery(server, presentationId, message);
                        }
                        catch (Exception x)
                        {
                            Log.Logger.Error("Unexpected exception in OnReceiveStudyLevelQuery.", x);
                            
                        }
                    });
                    return true;
                }
                if (level.Equals("SERIES"))
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        try
                        {
                            OnReceiveSeriesLevelQuery(server, presentationId, message);
                        }
                        catch (Exception x)
                        {
                            Log.Logger.Error("Unexpected exception in OnReceiveSeriesLevelQuery.", x);
                            }
                    });
                    return true;
                }

                /*
                if (level.Equals("IMAGE"))
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        try
                        {
                            OnReceiveImageLevelQuery(server, presentationId, message);
                        }
                        catch (Exception x)
                        {
                            Log.Logger.Error("Unexpected exception in OnReceiveImageLevelQuery.", x);
                        }
                    });
                    return true;
                } */

                Log.Logger.Error("Unexpected PatientData Root Query/Retrieve level: {0}", level);

                server.SendCFindResponse(presentationId, message.MessageId, new DicomMessage(),
                                         DicomStatuses.QueryRetrieveIdentifierDoesNotMatchSOPClass);
                return true;
            }

            // Not supported message type, send a failure status.
            server.SendCFindResponse(presentationId, message.MessageId, new DicomMessage(),
                                     DicomStatuses.QueryRetrieveIdentifierDoesNotMatchSOPClass);
            return true;
        }

        

        /// <summary>
        /// Extension method called to get a list of the SOP Classes and transfer syntaxes supported by the extension.
        /// </summary>
        /// <returns></returns>
        public override IList<SupportedSop> GetSupportedSopClasses()
        {
            return _list;
        }

        #endregion

        #region Overridden BaseSCP methods

        protected override DicomPresContextResult OnVerifyAssociation(AssociationParameters association, byte pcid)
        {
            if (!Device.AllowQuery)
            {
                return DicomPresContextResult.RejectUser;
            }

            return DicomPresContextResult.Accept;
        }

        #endregion 
    }
}