#region License

// 
// Copyright (c) 2011 - 2012, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

namespace UIH.Dicom.PACS.Service
{
    public class QueryHelper
    {
        public static string ReplacsWildcard(string dicomString)
        {
            string sqlString = null;

            if (dicomString.Contains("*") || dicomString.Contains("?"))
            {
                sqlString = dicomString.Replace("%", "[%]").Replace("_", "[_]");
                sqlString = sqlString.Replace('*', '%');
                sqlString = sqlString.Replace('?', '_');
            }

            return sqlString;
        }
    }
}