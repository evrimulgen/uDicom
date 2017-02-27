using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using App.PACS.Model;
using UIH.Dicom;
using UIH.Dicom.Log;
using UIH.Dicom.PACS.Service;
using UIH.Dicom.PACS.Service.Interface;
using UIH.Dicom.PACS.Service.Model;

namespace App.PACS.Logic
{
    [Export(typeof(ISopQuery)),
        PartCreationPolicy(CreationPolicy.NonShared)]
    internal class DicomQuery : ISopQuery
    {
        public static ILog Logger = LogManager.GetLog(typeof(DicomQuery).ToString());

        public void OnPatientQuery(DicomMessage message, SelectCallback<IPatientData> callback)
        {
            // filter condition: 
            // PatientsName, PatientsId, PatientsSex, PatientsBirthDate, PatientsBirthTime

            DicomDataset data = message.DataSet;

            using (var ctx = new PacsContext())
            {
                IQueryable<Patient> results = from patient in ctx.Patients select patient;
                
                foreach (var attrib in data)
                {
                    if (!attrib.IsNull)
                    {
                        #region Build Query 

                        switch (attrib.Tag.TagValue)
                        {
                            case DicomTags.PatientsName:
                                {
                                    string patientName = data[DicomTags.PatientsName].GetString(0,
                                                                                                string.Empty);
                                    if (patientName.Length == 0)
                                        break;

                                    var replaced = QueryHelper.ReplacsWildcard(patientName);
                                    if (replaced == null)
                                    {
                                        results = from row in results
                                                  where row.PatientName.Equals(patientName)
                                                  select row;
                                    }
                                    else
                                    {
                                        results = from row in results
                                                  where row.PatientName.Contains(replaced)
                                                  select row;
                                    }

                                    break;
                                }

                            case DicomTags.PatientId:
                                {
                                    string patientId = data[DicomTags.PatientId].GetString(0, string.Empty);
                                    if (patientId.Length == 0)
                                        break;

                                    var replaced = QueryHelper.ReplacsWildcard(patientId);
                                    if (replaced == null)
                                    {
                                        results = from row in results
                                            where row.PatientId.Equals(patientId)
                                            select row;
                                    }
                                    else
                                    {
                                        results = from row in results
                                            where row.PatientId.Contains(replaced)
                                            select row;
                                    }
                                    
                                    break;
                                }
                            case DicomTags.PatientsBirthDate:
                                {
                                    var values = (string[])data[DicomTags.PatientsBirthDate].Values;
                                    if (values == null || values.Length == 0)
                                        break;

                                    results = from study in results
                                              where (values.Length == 1
                                                         ? study.PatientBirthDate.Equals(values[0])
                                                         : values.Contains(study.PatientBirthDate))
                                              select study;
                                    break;
                                }
                            default:
                                break;
                        }

                        #endregion
                    }
                }

                foreach (Patient source in results.ToList())
                {
                    callback(source);
                }
            }
        }

        public void OnStudyQuery(DicomMessage message, SelectCallback<IStudyData> callback)
        {
            // Supported Query Condition includes: 
            // PatientsName, PatientsId, PatientsSex, PatientsBirthDate, PatientsBirthTime
            // StudyInstanceUid, StudyId, StudyDescription, AccessionNumber, ModalitiesInStudy
            // ReferringPhysiciansName, StudyDate, StudyTime

            var data = message.DataSet;

            using (var ctx = new PacsContext())
            {
                #region Build Query 

                //Linq query is lazy query mechanism 
                IQueryable<Study> results = from study in ctx.Studies select study;
                foreach (var attrib in message.DataSet)
                {
                    if (!attrib.IsNull)
                    {
                        switch (attrib.Tag.TagValue)
                        {
                            case DicomTags.PatientsName:
                                {
                                    string patientName = data[DicomTags.PatientsName].GetString(0,
                                        string.Empty);
                                    if (patientName.Length == 0)
                                        break;

                                    var replaced = QueryHelper.ReplacsWildcard(patientName);
                                    if (replaced == null)
                                    {
                                        results = from study in results
                                                  where study.PatientName.Equals(patientName)
                                                  select study;
                                    }
                                    else
                                    {
                                        results = from study in results
                                                  where study.PatientName.Contains(replaced)
                                                  select study;
                                    }

                                    break;
                                }

                            case DicomTags.PatientId:
                                {
                                    string patientId = data[DicomTags.PatientId].GetString(0, string.Empty);
                                    if (patientId.Length == 0)
                                        break;

                                    var replaced = QueryHelper.ReplacsWildcard(patientId);
                                    if (replaced == null)
                                    {
                                        results = from row in results
                                                  where row.PatientId.Equals(patientId)
                                                  select row;
                                    }
                                    else
                                    {
                                        results = from row in results
                                                  where row.PatientId.Contains(replaced)
                                                  select row;
                                    }

                                    break;
                                }

                            case DicomTags.PatientsSex:
                                {
                                    var values = (string[])data[DicomTags.PatientsSex].Values;
                                    if (values == null || values.Length == 0)
                                        break;

                                    results = from study in results
                                              where (values.Length == 1
                                                  ? study.PatientSex.Equals(values[0])
                                                  : values.Contains(study.PatientSex))
                                              select study;
                                    break;
                                }
                            case DicomTags.PatientsBirthDate:
                                {
                                    var values = (string[])data[DicomTags.PatientsBirthDate].Values;
                                    if (values == null || values.Length == 0)
                                        break;

                                    results = from study in results
                                              where (values.Length == 1
                                                  ? study.PatientBirthday.Equals(values[0])
                                                  : values.Contains(study.PatientBirthday))
                                              select study;
                                    break;
                                }
                        }
                    }

                }

                #endregion

                Logger.Warn(results.ToString());

                foreach (var source in results.ToList())
                {
                    callback(source);
                }

            }
        }

        public void OnSeriesQuery(DicomMessage message, SelectCallback<ISeriesData> callback)
        {
            // Support query conditions :
            // StudyInstanceUid, SeriesInstanceUid, Modality, SeriesDescription
            // SeriesNumber

            var data = message.DataSet;

            using (var ctx = new PacsContext())
            {
                #region Build Query

                //Linq query is lazy query mechanism 
                var results = from row in ctx.Series select row;
                foreach (var attrib in message.DataSet)
                {
                    if (!attrib.IsNull)
                    {
                        switch (attrib.Tag.TagValue)
                        {
                            case DicomTags.StudyInstanceUid:
                                var studyUid = data[DicomTags.StudyInstanceUid].GetString(0,
                                        string.Empty);

                                results = from row in results
                                    where row.Study.StudyUid.Equals(studyUid)
                                    select row;

                                break;
                            case DicomTags.SeriesInstanceUid:
                                var uid = data[DicomTags.SeriesInstanceUid].GetString(0, string.Empty);
                                results = from row in results
                                    where row.SeriesUid.Equals(uid)
                                    select row;

                                break;
                            case DicomTags.Modality:
                                var modality = data[DicomTags.Modality].GetString(0, string.Empty);
                                results = from row in results
                                    where row.Modality.Equals(modality)
                                    select row;
                                break;
                            case DicomTags.SeriesDescription:
                                var description = data[DicomTags.SeriesDescription].GetString(0, string.Empty);
                                var replaced = QueryHelper.ReplacsWildcard(description);

                                if (replaced == null)
                                {
                                    results = from row in results
                                        where row.SeriesDescription.Equals(description)
                                        select row;
                                }
                                else
                                {
                                    results = from row in results
                                        where row.SeriesDescription.Contains(description)
                                        select row;
                                }

                                break;
                            case DicomTags.SeriesNumber:
                                var number = data[DicomTags.SeriesNumber].GetUInt32(0, 0);
                                results = from row in results
                                    where row.SeriesNumber.Equals(number.ToString())
                                    select row;
                  
                                break;
                        }
                    }
                }

                foreach (var source in results.ToList())
                {
                    callback(source);
                }

                #endregion
            }
        }


        // Instance Level 
        // StudyInstanceUid, SeriesInstanceUid, SopInstanceUid, InstanceNumber, SopClassUid

    }
}
