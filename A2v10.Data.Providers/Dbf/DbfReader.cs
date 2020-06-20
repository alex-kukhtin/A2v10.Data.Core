// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Text;

using A2v10.Data.Interfaces;

namespace A2v10.Data.Providers.Dbf
{
	public class DbfReader : IExternalDataReader
	{
		private readonly DataFile _file;

		const String ErrorIncorrectFormat = "DbfReader. Incorrect file format";

		public DbfReader(DataFile file)
		{
			_file = file;
		}

		public IExternalDataFile Read(Stream stream)
		{
			using (var rdr = new BinaryReader(stream))
			{
				Read(rdr);
			}
			return _file;
		}

		public ExpandoObject ParseFile(Stream stream, ITableDescription table)
		{
			// extension
			return this.ParseFlatFile(stream, table);
		}

		public ExpandoObject CreateDataModel(Stream stream)
		{
			// extension
			return this.CreateExpandoObject(stream);
		}


		public void Read(BinaryReader rdr)
		{
			Char c = (Char)rdr.ReadByte();
			Byte y = rdr.ReadByte(); // modified date
			Byte m = rdr.ReadByte();
			Byte d = rdr.ReadByte();
			_file.LastModifedDate = new DateTime(y + 1900, m, d);
			Int32 numRecords = rdr.ReadInt32();
			Int16 headerSize = rdr.ReadInt16();
			Int16 recordSize = rdr.ReadInt16();
			Byte[] rsvd = rdr.ReadBytes(3 + 13 + 4); // reserved;
			Int32 numFields = (headerSize - 1 - 32) / 32;
			for (Int32 i = 0; i < numFields; i++)
			{
				ReadHeader(rdr);
			}
			_file.MapFields();
			if (rdr.BaseStream.CanRead)
			{
				// if possible
				Byte term = rdr.ReadByte();
				if (term != 13)
				{
					throw new FormatException(ErrorIncorrectFormat);
				}
			}
			// skip tail (may be present)
			while (rdr.BaseStream.Position < (Int64)headerSize)
				rdr.ReadByte();

			for (Int32 i = 0; i < numRecords; i++)
			{
				ReadRecord(rdr, recordSize);
			}

			if (rdr.BaseStream.Position < rdr.BaseStream.Length)
			{
				// read tail if possible
				Byte eof = rdr.ReadByte();
				if (eof != 0x1a && eof != 0)
					throw new FormatException(ErrorIncorrectFormat);
			}
		}

		FieldType Char2FieldType(Char ch)
		{
			switch (ch)
			{
				case 'N': return FieldType.Numeric;
				case 'D': return FieldType.Date;
				case 'C': return FieldType.Char;
				case 'M': return FieldType.Memo;
				case 'L': return FieldType.Boolean;
				case 'F': return FieldType.Float;
				default:
					throw new FormatException($"Invalid field type: '{ch}'");
			}
		}

		void ReadHeader(BinaryReader rdr)
		{
			Byte[] name = rdr.ReadBytes(11); // max length
			StringBuilder sb = new StringBuilder();
			for (Int32 i = 0; i < 11; i++)
			{
				if (name[i] == 0)
					break;
				sb.Append((Char)name[i]);
			}
			Field f = _file.CreateField();
			f.Name = sb.ToString();
			Char ft = (Char)rdr.ReadByte();
			f.Type = Char2FieldType(ft);
			Int32 addr = rdr.ReadInt32();
			f.Size = (Int32)rdr.ReadByte(); // fieldSize;
			f.Decimal = (Int32) rdr.ReadByte();
			rdr.ReadInt16();  // reserved
			rdr.ReadByte(); // workarea
			rdr.ReadInt16(); // reserved
			Byte flag = rdr.ReadByte();
			rdr.ReadBytes(8); // tail
		}

		void ReadRecord(BinaryReader rdr, Int16 recordSize)
		{
			Byte del = rdr.ReadByte();
			Byte[] dat = rdr.ReadBytes(recordSize - 1);
			if (del == 0x2a) // deleted record (*)
				return;
			ParseRecord(dat);
			//_data.Add(rec);
		}

		Record ParseRecord(Byte[] dat)
		{
			Int32 iIndex = 0;

			Record rec = _file.CreateRecord();
			for (Int32 i = 0; i < _file.FieldCount; i++)
			{
				Field f = _file.GetField(i);
				var fd = new FieldData();
				rec.DataFields.Add(fd);
				fd.FieldType = f.Type;
				switch (f.Type)
				{
					case FieldType.Boolean:
						{
							Byte bVal = dat[iIndex];
							fd.BooleanValue = bVal == (Char) 'T';
						}
						break;
					case FieldType.Char:
						{
							Byte[] xb = new Byte[f.Size];
							for (Int32 j = 0; j < f.Size; j++)
								xb[j] = dat[iIndex + j];
							Decoder dec = _file.FindDecoding(xb).GetDecoder();
							Char[] chs = new Char[xb.Length];
							dec.GetChars(xb, 0, xb.Length, chs, 0);
							StringBuilder sb = new StringBuilder();
							sb.Append(chs);
							String str = sb.ToString().Trim();
							fd.StringValue = str;
						}
						break;
					case FieldType.Numeric:
						{
							StringBuilder sb = new StringBuilder();
							for (Int32 j = 0; j < f.Size; j++)
								sb.Append((Char)dat[iIndex + j]);
							String x = sb.ToString().Trim();
							Decimal.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out Decimal dv);
							fd.DecimalValue = dv;
						}
						break;
					case FieldType.Date:
						// 8 bytes YYYYMMDD (char)
						Int32 year = (dat[iIndex] - '0') * 1000;
						if (year < 0)
						{
							fd.DateValue = DateTime.MinValue; // no date
							break;
						}
						year += (dat[iIndex + 1] - '0') * 100;
						year += (dat[iIndex + 2] - '0') * 10;
						year += (dat[iIndex + 3] - '0');
						Int32 month = (dat[iIndex + 4] - '0') * 10 + (dat[iIndex + 5] - '0');
						Int32 day = (dat[iIndex + 6] - '0') * 10 + (dat[iIndex + 7] - '0');
						try
						{
							DateTime dt = new DateTime(year, month, day);
							fd.DateValue = dt;
						}
						catch (Exception)
						{
							fd.DateValue = DateTime.MinValue;
						}
						break;
				}
				iIndex += f.Size;
			}
			return rec;
		}
	}
}
