// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.IO;
using A2v10.Data.Interfaces;

namespace A2v10.Data.Providers
{
	public static class ExternalDataExtensions
	{
		public static ExpandoObject ParseFlatFile(this IExternalDataReader rdr, Stream stream, ITableDescription table)
		{
			var file = rdr.Read(stream);
			foreach (var r in file.Records)
			{
				var row = table.NewRow();
				for (Int32 f = 0; f < file.FieldCount; f++)
				{
					var fn = file.FieldName(f);
					table.SetValue(row, fn, r.FieldValue(f));
				}
			}
			return table.ToObject();
		}

		public static ExpandoObject CreateExpandoObject(this IExternalDataReader rdr, Stream stream)
		{
			var table = new FlatTable();
			rdr.ParseFlatFile(stream, table);
			return table.ToObject();
		}
	}
}
