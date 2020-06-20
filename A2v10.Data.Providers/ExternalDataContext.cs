// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Text;

using A2v10.Data.Interfaces;
using A2v10.Data.Providers.Csv;
using A2v10.Data.Providers.Dbf;
using A2v10.Data.Providers.Xml;

namespace A2v10.Data.Providers
{
	public class ExternalDataContext : IExternalDataProvider
	{
		#region IExternalDataProvider

		public IExternalDataReader GetReader(String format, Encoding enc, String fileName)
		{
			if (format == null)
				format = "auto";
			else
				format = format.ToLowerInvariant();
			if (format == "auto")
			{
				fileName = fileName.ToLowerInvariant();
				if (fileName.EndsWith(".dbf"))
					format = "dbf";
				else if (fileName.EndsWith(".csv"))
					format = "csv";
				else if (fileName.EndsWith(".xml"))
					format = "xml";
			}
			switch (format)
			{
				case "dbf":
					var dataFileDbf = new DataFile()
					{
						Encoding = enc
					};
					return new DbfReader(dataFileDbf);
				case "csv":
					var dataFileCsv = new DataFile()
					{
						Encoding = enc
					};
					return new CsvReader(dataFileCsv);
				case "xml":
					var dataFileXml = new DataFile();
					return new XmlReader(dataFileXml);
			}
			return null;
		}
		#endregion
	}
}
