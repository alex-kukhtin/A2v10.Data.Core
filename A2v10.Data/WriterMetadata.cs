// Copyright © 2015-2022 Alex Kukhtin. All rights reserved.

using System.Data;

namespace A2v10.Data;

internal class WriterMetadata
{
	private readonly IDictionary<String, Tuple<DataTable, String>> _tables = new Dictionary<String, Tuple<DataTable, String>>();
	private readonly IDictionary<String, String> _jsonParams = new Dictionary<String, String>();

	internal IDictionary<String, Tuple<DataTable, String>> Tables => _tables;
	internal IDictionary<String, String> JsonParams => _jsonParams;
	internal void ProcessOneMetadata(IDataReader rdr)
	{
		var rsName = rdr.GetName(0);
		var fi = rsName.Split('!');
		if ((fi.Length < 3) || (fi[2] != "Metadata" && fi[2] != "Json"))
			throw new DataWriterException($"First field name part '{rsName}' is invalid. 'ParamName!DataPath!Metadata' or 'ParamName!DataPath!Json' expected.");
		var paramName = fi[0];
		var tablePath = fi[1];

		if (fi[2] == "Json")
		{
			_jsonParams.Add($"@{paramName}", tablePath);
		}
		else
		{
			var table = new DataTable();
			var schemaTable = rdr.GetSchemaTable();
			if (schemaTable == null)
				return;
			/* starts from 1 */
			for (Int32 c = 1; c < rdr.FieldCount; c++)
			{
				var ftp = rdr.GetFieldType(c);
				var fn = rdr.GetName(c);
				if (fn.Contains("!!"))
				{
					var fx = fn.Split('!');
					if (fx.Length != 3)
						throw new DataWriterException($"Field name '{rsName}' is invalid. 'Name!!Modifier' expected.");
					fn = fx[0];
				}
				var fieldColumn = new DataColumn(fn, ftp);
				if (ftp == typeof(String))
					fieldColumn.MaxLength = Convert.ToInt32(schemaTable.Rows[c]["ColumnSize"]);
				table.Columns.Add(fieldColumn);
			}
			_tables.Add("@" + paramName, new Tuple<DataTable, String>(table, tablePath));
		}
	}

	internal void Clear()
    {
		foreach (var t in _tables)
			t.Value.Item1.Clear();
    }
}
