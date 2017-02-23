/////////////////////////////////////////////////////////////////////////
//// Copyright, (c) Shanghai United Imaging Healthcare Inc
//// All rights reserved. 
//// 
//// author: qiuyang.cao@united-imaging.com
////
//// File: FileReference.cs
////
//// Summary:
////
////
//// Date: 2014/08/18
//////////////////////////////////////////////////////////////////////////
#region License

// Copyright (c) 2011 - 2013, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using UIH.Dicom.IO;

namespace UIH.Dicom
{
	internal class FileReference
	{
		#region Private Members

		private readonly long _length;

		#endregion

		#region Public Properties

		internal DicomStreamOpener StreamOpener { get; private set; }

		internal long Offset { get; private set; }
		internal Endian Endian { get; private set; }
		internal DicomVr Vr { get; private set; }

		public uint Length
		{
			get { return (uint) _length; }
		}

		#endregion

		#region Constructors

		internal FileReference(DicomStreamOpener streamOpener, long offset, long length, Endian endian, DicomVr vr)
		{
			StreamOpener = streamOpener;
			Offset = offset;
			_length = length;
			Endian = endian;
			Vr = vr;
		}

		#endregion
	}
}
