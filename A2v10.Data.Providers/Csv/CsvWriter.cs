﻿
// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using System.IO;
using System.Text;

namespace A2v10.Data.Providers.Csv;
public class CsvWriter(DataFile file) : IExternalDataWriter
{
	private readonly DataFile _file = file;

	public void SetDelimiter(Char delimiter)
	{
		_file.Delimiter = delimiter;
	}

	public void Write(Stream stream)
	{
		using var sw = new StreamWriter(stream);
		Write(sw);
	}

	public void Write(StreamWriter wr)
	{

		wr.Write(GetHeader());
		for (var r = 0; r < _file.NumRecords; r++)
		{
			wr.WriteLine();
			wr.Write(GetRecord(_file.GetRecord(r)));
		}
	}

	String GetHeader()
	{
		var sb = new StringBuilder();
		foreach (var f in _file.Fields)
		{
			if (sb.Length > 0)
				sb.Append(_file.Delimiter);
			sb.Append(EscapeString(f.Name));
		}
		return sb.ToString();
	}

	String GetRecord(Record record)
	{
		var sb = new StringBuilder();
		foreach (var df in record.DataFields)
		{
			if (sb.Length > 0)
				sb.Append(_file.Delimiter);
			sb.Append(EscapeString(df?.StringValue));
		}
		return sb.ToString();
	}

	String? EscapeString(String? str)
	{
		if (str == null)
			return null;
		if (str.IndexOfAny([_file.Delimiter, '"', '\n', '\r']) != -1)
		{
			return $"\"{str.Replace("\"", "\"\"")}\"";
		}
		return str;
	}
}

