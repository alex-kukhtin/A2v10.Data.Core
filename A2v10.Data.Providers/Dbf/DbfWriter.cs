
// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using System.Globalization;
using System.IO;
using System.Text;

namespace A2v10.Data.Providers.Dbf;
public class DbfWriter(DataFile file) : IExternalDataWriter
{
	private readonly DataFile _file = file;

	public void SetDelimiter(Char delimiter)
	{
		_file.Delimiter = delimiter;
	}

    public void SetSkipHeader(Boolean skipHeader)
	{
        _file.SkipHeader = skipHeader;
    }

    public void Write(Stream stream)
	{
		using var bw = new BinaryWriter(stream);
		Write(bw);
	}

	public void Write(BinaryWriter wr)
	{
		wr.Write((Byte)0x03); // no dbt
		DateTime dt = _file.LastModifedDate;
		wr.Write((Byte)(dt.Year - 1900));
		wr.Write((Byte)dt.Month);
		wr.Write((Byte)dt.Day);

		wr.Write((Int32)_file.NumRecords);
		wr.Write((Int16)(_file.FieldCount * 32 + 32 + 1)); // header size

		Int16 recordSize = (Int16)(GetRecordSize() + 1); // + 1 byte (delete marker '*')
		wr.Write(recordSize);

		for (Int32 i = 0; i < 3 + 13 + 4; i++)
			wr.Write((Byte)0); // reserved

		for (Int32 i = 0; i < _file.FieldCount; i++)
		{
			WriteHeader(wr, _file.GetField(i), i);
		}
		wr.Write((Byte)13); // header term

		for (Int32 i = 0; i < _file.NumRecords; i++)
		{
			WriteRecord(wr, _file.GetRecord(i));
		}

		wr.Write((Byte)0x1a); // eof
	}

	private Int32 GetRecordSize()
	{
		Int32 sz = 0;
		foreach (var f in _file.Fields)
		{
			switch (f.Type)
			{
				case FieldType.Char:
					sz += f.Size;
					break;
				case FieldType.Date:
					sz += 8;
					break;
				case FieldType.Numeric:
					sz += f.Size;
					break;
				case FieldType.Boolean:
					sz += 1;
					break;
			}
		}
		return sz;
	}

	private static void WriteHeader(BinaryWriter wr, Field f, Int32 _)
	{
		// 32 bytes
		// 11 bytes name
		String fn = f.Name + new String(' ', 11);
		for (Int32 i = 0; i < 11; i++)
		{
			Char c = fn[i];
			wr.Write(c == ' ' ? (Byte)0 : (Byte)c);
		}
		// 12 byte - field type
		Byte fieldSize = 0;
		switch (f.Type)
		{
			case FieldType.Char:
				wr.Write((Byte)'C');
				fieldSize = (Byte)f.Size;
				break;
			case FieldType.Date:
				wr.Write((Byte)'D');
				fieldSize = 8;
				break;
			case FieldType.Numeric:
				wr.Write((Byte)'N');
				fieldSize = (Byte)f.Size;
				break;
			case FieldType.Boolean:
				wr.Write((Byte)'L');
				fieldSize = 1;
				break;
		}
		wr.Write((Int32)0); // addr
		wr.Write((Byte)fieldSize); // size 
		wr.Write((Byte)f.Decimal); // decimal count
		wr.Write((Int16)0); // reserved
		wr.Write((Byte)0); // workarea
		wr.Write((Int16)0); // reserved
		wr.Write((Byte)0); // set flag
		for (Int32 i = 0; i < 8; i++)
			wr.Write((Byte)0); // reserved
	}

	void WriteRecord(BinaryWriter wr, Record rec)
	{
		wr.Write((Byte)0x20); // not deleted
		String sVal;

		// simply write
		for (Int32 i = 0; i < rec.DataFields.Count; i++)
		{
			FieldData xd = rec.DataFields[i];
			Field fd = _file.GetField(i);
			switch (xd.FieldType)
			{
				case FieldType.Numeric:
					// fd.Size bytes (left padding spaces)
					sVal = xd.DecimalValue.ToString(CultureInfo.InvariantCulture);
					if (sVal.Length < fd.Size)
					{
						String x = new(' ', fd.Size - sVal.Length);
						sVal = x + sVal;
					}
					if (sVal.Length != fd.Size)
					{
						throw new InvalidOperationException($"Numeric: Invalid field size ({fd.Size})");
					}
					break;
				case FieldType.Date:
					// 8 bytes
					if (xd.DateValue == DateTime.MinValue)
						sVal = new String(' ', 8);
					else
						sVal = String.Format("{0:0000}{1:00}{2:00}", xd.DateValue.Year, xd.DateValue.Month, xd.DateValue.Day);
					break;
				case FieldType.Char:
					sVal = xd.StringValue ?? String.Empty;
					if (sVal.Length < fd.Size)
					{
						var x = new String(' ', fd.Size - sVal.Length);
						sVal += x;
					}
					if (sVal.Length != fd.Size)
                        throw new InvalidOperationException($"Char: Invalid field size ({fd.Size})");
                    break;
				case FieldType.Boolean:
					sVal = xd.BooleanValue ? "T" : "F";
					break;
				default:
                    throw new InvalidOperationException($"Unknown field type ({xd.FieldType})");
            }
            Encoder enc = _file.Encoding.GetEncoder();
			Char[] chs = sVal.ToCharArray();
			Byte[] buff = new Byte[sVal.Length];
			enc.GetBytes(chs, 0, chs.Length, buff, 0, true);
			wr.Write(buff);
		}
	}
}

