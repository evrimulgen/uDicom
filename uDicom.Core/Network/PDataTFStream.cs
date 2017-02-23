/////////////////////////////////////////////////////////////////////////
//// Copyright, (c) Shanghai United Imaging Healthcare Inc
//// All rights reserved. 
//// 
//// author: qiuyang.cao@united-imaging.com
////
//// File: PDataTFStream.cs
////
//// Summary:
////
////
//// Date: 2014/08/19
//////////////////////////////////////////////////////////////////////////
#region License

// Copyright (c) 2011 - 2013, United-Imaging Inc.
// All rights reserved.
// http://www.united-imaging.com

#endregion

using System;
using System.IO;

namespace UIH.Dicom.Network
{
    internal class PDataTFStream : Stream
    {
        public delegate void TickDelegate();

		#region Private Members
		private bool _command;
		private readonly uint _max;
		private readonly byte _pcid;
		private PDataTFWrite _pdu;
		private readonly NetworkBase _networkBase;
		private readonly bool _combineCommandData;
		#endregion

		#region Public Constructors
		public PDataTFStream(NetworkBase networkBase, byte pcid, uint max, bool combineCommandData)
		{
			_command = true;
			_pcid = pcid;
			_max = max;
			_pdu = new PDataTFWrite(max);
			_pdu.CreatePDV(pcid);

			_networkBase = networkBase;
			_combineCommandData = combineCommandData;
			BytesWritten = 0;
		}
		#endregion

		#region Public Properties
		public TickDelegate OnTick;

		public long BytesWritten { get; set; }

		public bool IsCommand
		{
			get { return _command; }
			set
			{
				if (!_combineCommandData)
				{
					WritePDU(true);
					_command = value;
					_pdu.CreatePDV(_pcid);
				}
				else
				{
					_pdu.CompletePDV(true, _command);
					_command = value;
					_pdu.CreatePDV(_pcid);
				}
			}
		}
		#endregion

        #region Public Members
        public void Flush(bool last)
        {
            WritePDU(last);
            //_network.Flush();
        }
        #endregion

		#region Private Members

		private void WritePDU(bool last)
		{
			_pdu.CompletePDV(last, _command);

			RawPDU raw = _pdu.Write();

			_networkBase.EnqueuePdu(raw);
			if (OnTick != null)
				OnTick();

			_pdu = new PDataTFWrite(_max);

		}

		private void AppendBuffer(byte[] buffer, int index, int count)
		{
			_pdu.AppendPdv(buffer, index, count);
		}

		#endregion

		#region Stream Members
		public override bool CanRead
		{
			get { return false; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            //_network.Flush();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

		public override void Write(byte[] buffer, int offset, int count)
		{
			BytesWritten += count;

			int off = offset;
			int c = count;

			while (c > 0)
			{
				int bytesToWrite = Math.Min(c, (int)_pdu.GetRemaining());
				AppendBuffer(buffer, off, bytesToWrite);
				c -= bytesToWrite;
				off += bytesToWrite;

				if (_pdu.GetRemaining() == 0)
				{
					WritePDU(false);
					_pdu.CreatePDV(_pcid);
				}
			}
		}

		public void Write(Stream readStream)
		{
			var buffer = new byte[64 * 1024];
			int read;
			while ((read = readStream.Read(buffer, 0, buffer.Length)) > 0)
			{
				Write(buffer, 0, read);
			}
		}

		#endregion
	}
}
