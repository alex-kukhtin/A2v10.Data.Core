﻿// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System.Text;

using A2v10.Data.Providers.Csv;
using A2v10.Data.Providers.Dbf;
using A2v10.Data.Providers.Xml;

namespace A2v10.Data.Providers;
public class ExternalDataContext : IExternalDataProvider
{
	#region IExternalDataProvider

	public IExternalDataReader GetReader(String format, Encoding? enc, String? fileName)
	{
		if (format == null)
			format = "auto";
		else
			format = format.ToLowerInvariant();
		if (format == "auto")
		{
			if (fileName == null)
				throw new InvalidOperationException("For 'auto' format, the file name is required");
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
				var dataFileDbf = new DataFile(enc);
				return new DbfReader(dataFileDbf);
			case "csv":
				var dataFileCsv = new DataFile(enc);
				return new CsvReader(dataFileCsv);
			case "xml":
				var dataFileXml = new DataFile();
				return new XmlReader(dataFileXml);
			default:
				throw new ExternalDataException($"Format '{format}'. Reader not found.");
		}
	}

	public IExternalDataWriter GetWriter(IDataModel model, String format, Encoding enc)
	{
		switch (format)
		{
			case "dbf":
				{
					var dataFileDbf = new DataFile(enc)
					{
						Format = DataFileFormat.dbf
					};
					dataFileDbf.FillModel(model);
					return new DbfWriter(dataFileDbf);
				}
			case "csv":
				{
					var dataFileCsv = new DataFile(enc)
					{
						Format = DataFileFormat.csv,
						Delimiter = ';'
					};
					dataFileCsv.FillModel(model);
					return new CsvWriter(dataFileCsv);
				}
			default:
				throw new ExternalDataException($"Format '{format}'. Writer not found.");
		}
	}
	#endregion
}

