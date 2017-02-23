/////////////////////////////////////////////////////////////////////////
//// Copyright, (c) Shanghai United Imaging Healthcare Inc
//// All rights reserved. 
//// 
//// author: qiuyang.cao@united-imaging.com
////
//// File: StreamWriter.cs
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

using System.IO;

namespace UIH.Dicom.IO
{
    internal enum DicomWriteStatus
    {
        Success,
        UnknownError
    }

	internal class DicomStreamWriter
	{
		#region Private Members

		private const uint _undefinedLength = 0xFFFFFFFF;

		private readonly Stream _stream;
		private BinaryWriter _writer;
		private TransferSyntax _syntax;
		private Endian _endian;

        private ushort _group = 0xffff;
        #endregion

		#region Public Constructors

		public DicomStreamWriter(Stream stream)
		{
			_stream = stream;
		}

		#endregion

        #region Public Properties
        public TransferSyntax TransferSyntax
        {
            get { return _syntax; }
            set
            {
                _syntax = value;
                if (_endian != _syntax.Endian || _writer == null)
                {
                    _endian = _syntax.Endian;
                    _writer = EndianBinaryWriter.Create(_stream, _endian);
                }
            }
        }

		#endregion

		public DicomWriteStatus Write(TransferSyntax syntax, DicomDataset dataset, DicomWriteOptions options)
		{
			TransferSyntax = syntax;

			foreach (var item in dataset)
			{
				if (item.IsEmpty)
					continue;

				if (item.Tag.Element == 0x0000)
					continue;

				if (Flags.IsSet(options, DicomWriteOptions.CalculateGroupLengths)
				    && item.Tag.Group != _group)
				{
					_group = item.Tag.Group;
					_writer.Write((ushort) _group);
					_writer.Write((ushort) 0x0000);
					if (_syntax.ExplicitVr)
					{
						_writer.Write((byte) 'U');
						_writer.Write((byte) 'L');
						_writer.Write((ushort) 4);
					}
					else
					{
						_writer.Write((uint) 4);
					}
					_writer.Write((uint) dataset.CalculateGroupWriteLength(_group, _syntax, options));
				}

				_writer.Write((ushort) item.Tag.Group);
				_writer.Write((ushort) item.Tag.Element);

				if (_syntax.ExplicitVr)
				{
					_writer.Write((byte) item.Tag.VR.Name[0]);
					_writer.Write((byte) item.Tag.VR.Name[1]);
				}

				if (item is DicomElementSq)
				{
					var sq = item as DicomElementSq;

					if (_syntax.ExplicitVr)
						_writer.Write((ushort) 0x0000);

					if (Flags.IsSet(options, DicomWriteOptions.ExplicitLengthSequence))
					{
						int hl = _syntax.ExplicitVr ? 12 : 8;
						_writer.Write((uint) sq.CalculateWriteLength(_syntax, options & ~DicomWriteOptions.CalculateGroupLengths) - (uint) hl);
					}
					else
					{
						_writer.Write((uint) _undefinedLength);
					}

					foreach (DicomSequenceItem ids in (DicomSequenceItem[]) sq.Values)
					{
						_writer.Write((ushort) DicomTag.Item.Group);
						_writer.Write((ushort) DicomTag.Item.Element);

						if (Flags.IsSet(options, DicomWriteOptions.ExplicitLengthSequenceItem))
						{
							_writer.Write((uint) ids.CalculateWriteLength(_syntax, options & ~DicomWriteOptions.CalculateGroupLengths));
						}
						else
						{
							_writer.Write((uint) _undefinedLength);
						}

                        Write(this.TransferSyntax, ids, options & ~DicomWriteOptions.CalculateGroupLengths);

                        if (!Flags.IsSet(options, DicomWriteOptions.ExplicitLengthSequenceItem))
                        {
                            _writer.Write((ushort)DicomTag.ItemDelimitationItem.Group);
                            _writer.Write((ushort)DicomTag.ItemDelimitationItem.Element);
                            _writer.Write((uint)0x00000000);
                        }
                    }

                    if (!Flags.IsSet(options, DicomWriteOptions.ExplicitLengthSequence))
                    {
                        _writer.Write((ushort)DicomTag.SequenceDelimitationItem.Group);
                        _writer.Write((ushort)DicomTag.SequenceDelimitationItem.Element);
                        _writer.Write((uint)0x00000000);
                    }
                }

                else if (item is DicomFragmentSequence)
                {
                    DicomFragmentSequence fs = item as DicomFragmentSequence;

					if (_syntax.ExplicitVr)
						_writer.Write((ushort) 0x0000);
					_writer.Write((uint) _undefinedLength);

                    _writer.Write((ushort)DicomTag.Item.Group);
                    _writer.Write((ushort)DicomTag.Item.Element);

                    if (Flags.IsSet(options, DicomWriteOptions.WriteFragmentOffsetTable) && fs.HasOffsetTable)
                    {
                        _writer.Write((uint)fs.OffsetTableBuffer.Length);
                        fs.OffsetTableBuffer.CopyTo(_writer);
                    }
                    else
                    {
                        _writer.Write((uint)0x00000000);
                    }

                    foreach (DicomFragment bb in fs.Fragments)
                    {
                        _writer.Write((ushort)DicomTag.Item.Group);
                        _writer.Write((ushort)DicomTag.Item.Element);
                        _writer.Write((uint)bb.Length);
                        bb.GetByteBuffer(_syntax).CopyTo(_writer);
                    }

					_writer.Write((ushort) DicomTag.SequenceDelimitationItem.Group);
					_writer.Write((ushort) DicomTag.SequenceDelimitationItem.Element);
					_writer.Write((uint) 0x00000000);
				}
				else
				{
					var de = item;
					ByteBuffer theData = de.GetByteBuffer(_syntax, dataset.SpecificCharacterSet);
					if (_syntax.ExplicitVr)
					{
						if (de.Tag.VR.Is16BitLengthField)
						{
							// #10890: Can't encode the value length if the length of the data exceeds max value for a 16-bit field
							if (theData.Length > ushort.MaxValue - 1 /* must be even length so max allowed = 65534 */)
								throw new DicomDataException(string.Format(
									"Value for {0} exceeds maximum stream length allowed for a {1} VR attribute encoded using {2}",
									de.Tag, de.Tag.VR, _syntax));
							_writer.Write((ushort) theData.Length);
						}
						else
						{
							_writer.Write((ushort) 0x0000);
							_writer.Write((uint) theData.Length);
						}
					}
					else
					{
						_writer.Write((uint) theData.Length);
					}

					if (theData.Length > 0)
						theData.CopyTo(_writer);
                }
            }

            return DicomWriteStatus.Success;
        }
    }
}
