// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using A2v10.Data.Providers.Csv;
using A2v10.Data.Providers.Dbf;
using A2v10.Data.Providers.Xml;
using System;
using System.Text;

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

		public IExternalDataWriter GetWriter(IDataModel model, String format, Encoding enc)
		{
			switch (format)
			{
				case "dbf":
					{
						var dataFileDbf = new DataFile()
						{
							Encoding = enc,
							Format = DataFileFormat.dbf
						};
						dataFileDbf.FillModel(model);
						return new DbfWriter(dataFileDbf);
					}
				case "csv":
					{
						var dataFileCsv = new DataFile()
						{
							Encoding = enc,
							Format = DataFileFormat.csv,
							Delimiter = ';'
						};
						dataFileCsv.FillModel(model);
						return new CsvWriter(dataFileCsv);
					}
			}
			return null;
		}

		#endregion
	}
}
