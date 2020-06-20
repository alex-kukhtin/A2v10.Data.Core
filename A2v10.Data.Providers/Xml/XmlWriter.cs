
// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Xml;

namespace A2v10.Data.Providers.Csv
{
	public class XmlWriter
	{
		private readonly DataFile _file;

		public String RootElement { get; set; }
		public String RowElement { get; set; }

		public XmlWriter(DataFile file)
		{
			_file = file;
		}

		public void Write(Stream stream)
		{
			using (var sw = System.Xml.XmlWriter.Create(stream, new XmlWriterSettings() { Encoding = _file.Encoding, Indent = true, IndentChars = "\t"}))
			{
				sw.WriteStartDocument();
				sw.WriteStartElement(RootElement);
				Write(sw);
				sw.WriteEndElement();
				sw.WriteEndDocument();
			}
		}

		public void Write(System.Xml.XmlWriter wr)
		{
			for (Int32 i = 0; i < _file.NumRecords; i++)
			{
				wr.WriteStartElement(RowElement);
				WriteAttributes(_file.GetRecord(i), wr);
				wr.WriteEndElement();
			}
		}

		public void WriteAttributes(Record record, System.Xml.XmlWriter wr)
		{
			foreach(var f in _file.Fields)
			{
				Int32 ix = _file.GetOrCreateField(f.Name);
				String val = record.StringFieldValueByIndex(ix);
				if (val != null)
					wr.WriteAttributeString(f.Name, val);
			}
		}
	}
}
