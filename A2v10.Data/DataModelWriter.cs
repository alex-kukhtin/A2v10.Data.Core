// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using System.Data.SqlClient;
using System.Linq;

using Newtonsoft.Json;

namespace A2v10.Data;

internal class DataModelWriter
{
	private readonly WriterMetadata _writerMetadata;
	private readonly Boolean _allowEmptyStrings;

	public DataModelWriter(WriterMetadata writerMetadata, Boolean allowEmptyStrings)
    {
		_writerMetadata = writerMetadata;
		_allowEmptyStrings	= allowEmptyStrings;
	}

	internal void SetTableParameters(SqlCommand cmd, ExpandoObject data, Object? prms)
	{
		IDictionary<String, Object?>? scalarParams = SqlExtensions.GetParametersDictionary(prms);
		for (Int32 i = 0; i < cmd.Parameters.Count; i++)
		{
			SqlParameter prm = cmd.Parameters[i];
			var simpleParamName = prm.ParameterName[1..]; /*skip @*/
			if (prm.SqlDbType == SqlDbType.Structured)
			{
				if (_writerMetadata.Tables.TryGetValue(prm.ParameterName, out DataTablePatternTuple? table))
				{
					// table parameters (binging by object name)
					var dt = table.Table.ToDataTable();
					FillDataTable(dt, GetDataForSave(data, table.Path /*path*/));
					prm.Value = dt;
					prm.RemoveDbName(); // remove first segment (database name)
				}
				else
				{
					throw new DataWriterException($"Parameter {simpleParamName} not found");
				}
			}
			else if (prm.SqlDbType == SqlDbType.VarBinary)
			{
				throw new NotImplementedException(nameof(SqlDbType.VarBinary));
			}
			else if (_writerMetadata.JsonParams.TryGetValue(prm.ParameterName, out String? tablePath))
			{
				var dataForSave = data.Eval<ExpandoObject>(tablePath);
				prm.Value = JsonConvert.SerializeObject(dataForSave);
			}
			else
			{
				// simple parameter
				if (scalarParams != null && scalarParams.TryGetValue(simpleParamName, out Object? paramVal))
					prm.Value = SqlExtensions.Value2SqlValue(paramVal);
			}
		}
	}

	void FillDataTable(DataTable table, IEnumerable<ExpandoObject> data)
	{
		foreach (var d in data)
		{
			ProcessOneDataElem(table, d);
		}
	}

	static Object CheckId(String colName, Object dbVal, Type dataType)
	{
		if (dbVal == DBNull.Value)
			return dbVal;
		if (colName.EndsWith("Id") && (dataType == typeof(Int64) || dataType == typeof(Int32)))
		{
			if (dbVal.ToString() == "0")
				return DBNull.Value;
		}
		return dbVal;
	}

    static void CheckStringLength(DataColumn col, Object value, Int32 rowIndex)
    {
        if (col.DataType != typeof(String) || value is not String valString)
            return;
        if (value != null && valString.Length > col.MaxLength)
            throw new InvalidOperationException($"Line {rowIndex + 1}. The string '{valString}' is too long for the '{col.ColumnName}' field. The max length is {col.MaxLength}.");
    }

    void ProcessOneDataElem(DataTable table, ExpandoObject data)
	{
		var row = table.NewRow();
		var dataD = data as IDictionary<String, Object>;
		for (Int32 c = 0; c < table.Columns.Count; c++)
		{
			var col = table.Columns[c];
			if (col.ColumnName.Contains('.'))
			{
				// complex value
				if (GetComplexValue(data, col.ColumnName, out Object? rowVal1))
				{
					var dbVal = SqlExtensions.ConvertTo(rowVal1, col.DataType, _allowEmptyStrings);
					dbVal = CheckId(col.ColumnName, dbVal, col.DataType);
                    CheckStringLength(col, dbVal, table.Rows.Count);
                    row[col.ColumnName] = dbVal;
					continue;
				}
			}
			if (dataD.TryGetValue(col.ColumnName, out Object? rowVal2))
			{
				var dbVal = SqlExtensions.ConvertTo(rowVal2, col.DataType, _allowEmptyStrings);
				dbVal = CheckId(col.ColumnName, dbVal, col.DataType);
				row[col.ColumnName] = dbVal;
			}
		}
		table.Rows.Add(row);
	}

#pragma warning disable CA1822 // Mark members as static
	Boolean GetComplexValue(ExpandoObject data, String expr, out Object? rowVal)
#pragma warning restore CA1822 // Mark members as static
	{
		rowVal = null;
		var ev = data.Eval<Object>(expr);
		if (ev != null)
		{
			rowVal = ev;
			return true;
		}
		return false;
	}

	IEnumerable<ExpandoObject> GetDataForSave(ExpandoObject data, String path, Int32? parentIndex = null, Object? parentKey = null, Guid? parentGuid = null)
	{
		if (String.IsNullOrEmpty(path))
			yield return data;
		var x = path.Split('.');
		if (data is not IDictionary<String, Object> currentData)
			throw new DataWriterException("There is no current data");
		var currentId = data.Get<Object>("Id");
		Guid? currentGuid = null;
		for (Int32 i = 0; i < x.Length; i++)
		{
			Boolean bLast = (i == (x.Length - 1));
			String prop = x[i];
			// RowNumber is 1-based!
			Boolean isMap = false;
			if (prop.EndsWith("*"))
			{
				prop = prop[0..^1];
				isMap = true;
			}
			if (currentData.TryGetValue(prop, out Object? propValue))
			{
				if (propValue is IList<ExpandoObject> expList)
				{
					for (Int32 j = 0; j < expList.Count; j++)
					{
						var currVal = expList[j];
						currVal.Set(Const.Fileds.RowNumber, j + 1);
						currVal.SetNotNull(Const.Fileds.ParentId, currentId);
						currVal.SetNotNull(Const.Fileds.ParentKey, parentKey);
						var rowGuid = currVal.GetOrCreate<Guid>(Const.Fileds.Guid, () => Guid.NewGuid());
						if (parentIndex != null)
							currVal.Set(Const.Fileds.ParentRowNumber, parentIndex.Value + 1);
						currVal.SetNotNull(Const.Fileds.ParentGuid, parentGuid);
						if (bLast)
							yield return currVal;
						else
						{
							String newPath = String.Empty;
							for (Int32 k = i + 1; k < x.Length; k++)
								newPath = newPath.AppendDot(x[k]);
							foreach (var dx in GetDataForSave(currVal, newPath, parentIndex: j, parentKey: null, parentGuid: rowGuid))
								yield return dx;
						}
					}
				}
				else if (propValue is IList<Object> listObj)
				{
					for (Int32 j = 0; j < listObj.Count; j++)
					{
						if (listObj[j] is not ExpandoObject currVal)
							continue;
						currVal.Set(Const.Fileds.RowNumber, j + 1);
						currVal.SetNotNull(Const.Fileds.ParentId, currentId);
						currVal.SetNotNull(Const.Fileds.ParentKey, parentKey);
						var rowGuid = currVal.GetOrCreate<Guid>(Const.Fileds.Guid, () => Guid.NewGuid());
						if (parentIndex != null)
							currVal.Set(Const.Fileds.ParentRowNumber, parentIndex.Value + 1);
						currVal.SetNotNull(Const.Fileds.ParentGuid, parentGuid);
						if (bLast)
							yield return currVal;
						else
						{
							String newPath = String.Empty;
							for (Int32 k = i + 1; k < x.Length; k++)
								newPath = newPath.AppendDot(x[k]);
							foreach (var dx in GetDataForSave(currVal, newPath, parentIndex: j, parentKey: null, parentGuid: rowGuid))
								yield return dx;
						}
					}
				}
				else if (propValue is ExpandoObject propValEO)
				{
					if (propValue is not ExpandoObject currVal)
						continue;
					if (bLast)
					{
						if (isMap)
						{
							var propValD = propValEO as IDictionary<String, Object>;
							foreach (var (k, v) in propValD)
							{
								var mapItem = (v as ExpandoObject)!;
								mapItem.Set(Const.Fileds.CurrentKey, k);
								if (parentIndex != null)
									mapItem.Set(Const.Fileds.ParentRowNumber, parentIndex.Value + 1);
								mapItem.SetNotNull(Const.Fileds.ParentGuid, parentGuid);
								yield return mapItem;
							}
						}
						else
						{
							currVal.SetNotNull(Const.Fileds.ParentId, currentId);
							currVal.SetNotNull(Const.Fileds.ParentKey, parentKey);
							if (parentIndex != null)
								currVal.Set(Const.Fileds.ParentRowNumber, parentIndex.Value + 1);
							currentGuid = currVal.GetOrCreate<Guid>(Const.Fileds.Guid, () => Guid.NewGuid());
							currVal.SetNotNull(Const.Fileds.ParentGuid, parentGuid);
							yield return currVal;
						}
					}
					else
					{
						String newPath = String.Empty;
						for (Int32 k = i + 1; k < x.Length; k++)
							newPath = newPath.AppendDot(x[k]);
						if (isMap)
						{
							var currValD = currVal as IDictionary<String, Object>;
							foreach (var kv in currValD)
							{
								if (kv.Value is ExpandoObject mapItem)
								{
									foreach (var dx in GetDataForSave(mapItem, newPath, parentIndex: parentIndex, parentKey: kv.Key, parentGuid: currentGuid))
										yield return dx;
								}
								else
									throw new InvalidProgramException("Map item is not an Expando");
							}
						}
						else
						{
							currentGuid = currVal.GetOrCreate<Guid>(Const.Fileds.Guid, () => Guid.NewGuid());
							foreach (var dx in GetDataForSave(currVal, newPath, parentIndex: 0, parentKey: null, parentGuid: currentGuid))
								yield return dx;
							yield break;
						}
					}
				}
			}
		}
	}

	internal ITableDescription GetTableDescription()
	{
		var tables = _writerMetadata.Tables;
		if (tables.Count != 1)
			throw new DataWriterException("Invalid tables for GetTableDescription");
		return new TableDescription(tables.First().Value.Table);
	}
}
