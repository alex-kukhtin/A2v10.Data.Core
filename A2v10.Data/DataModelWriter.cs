// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;

using Newtonsoft.Json;

using A2v10.Data.Interfaces;

namespace A2v10.Data
{

	internal class DataModelWriter
	{
		private readonly IDictionary<String, Tuple<DataTable, String>> _tables = new Dictionary<String, Tuple<DataTable, String>>();
		private readonly IDictionary<String, String> _jsonParams = new Dictionary<String, String>();

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

		internal void SetTableParameters(SqlCommand cmd, ExpandoObject data, Object prms)
		{
			IDictionary<String, Object> scalarParams = SqlExtensions.GetParametersDictionary(prms);
			for (Int32 i = 0; i < cmd.Parameters.Count; i++)
			{
				SqlParameter prm = cmd.Parameters[i];
				var simpleParamName = prm.ParameterName[1..]; /*skip @*/
				if (prm.SqlDbType == SqlDbType.Structured)
				{
					if (_tables.TryGetValue(prm.ParameterName, out Tuple<DataTable, String> table))
					{
						// table parameters (binging by object name)
						FillDataTable(table.Item1, GetDataForSave(data, table.Item2 /*path*/));
						prm.Value = table.Item1;
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
				else if (_jsonParams.TryGetValue(prm.ParameterName, out String tablePath))
				{
					var dataForSave = data.Eval<ExpandoObject>(tablePath);
					prm.Value = JsonConvert.SerializeObject(dataForSave);
				}
				else
				{
					// simple parameter
					if (scalarParams != null && scalarParams.TryGetValue(simpleParamName, out Object paramVal))
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
			if (colName.EndsWith("Id") && dataType == typeof(Int64))
			{
				if (dbVal.ToString() == "0")
					return DBNull.Value;
			}
			return dbVal;
		}

		void ProcessOneDataElem(DataTable table, ExpandoObject data)
		{
			var row = table.NewRow();
			var dataD = data as IDictionary<String, Object>;
			for (Int32 c = 0; c < table.Columns.Count; c++)
			{
				Object rowVal;
				var col = table.Columns[c];
				if (col.ColumnName.Contains("."))
				{
					// complex value
					if (GetComplexValue(data, col.ColumnName, out rowVal))
					{
						var dbVal = SqlExtensions.ConvertTo(rowVal, col.DataType);
						dbVal = CheckId(col.ColumnName, dbVal, col.DataType);
						row[col.ColumnName] = dbVal;
						break;
					}
				}
				if (dataD.TryGetValue(col.ColumnName, out rowVal))
				{
					var dbVal = SqlExtensions.ConvertTo(rowVal, col.DataType);
					dbVal = CheckId(col.ColumnName, dbVal, col.DataType);
					row[col.ColumnName] = dbVal;
				}
			}
			table.Rows.Add(row);
		}

#pragma warning disable CA1822 // Mark members as static
		Boolean GetComplexValue(ExpandoObject data, String expr, out Object rowVal)
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

		IEnumerable<ExpandoObject> GetDataForSave(ExpandoObject data, String path, Int32? parentIndex = null, Object parentKey = null, Guid? parentGuid = null)
		{
			if (String.IsNullOrEmpty(path))
				yield return data;
			var x = path.Split('.');
			if (!(data is IDictionary<String, Object> currentData))
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
				if (currentData.TryGetValue(prop, out Object propValue))
				{
					if (propValue is IList<ExpandoObject> expList)
					{
						for (Int32 j = 0; j < expList.Count; j++)
						{
							var currVal = expList[j];
							currVal.Set("RowNumber", j + 1);
							currVal.SetNotNull("ParentId", currentId);
							currVal.SetNotNull("ParentKey", parentKey);
							var rowGuid = currVal.GetOrCreate<Guid>("GUID", () => Guid.NewGuid());
							if (parentIndex != null)
								currVal.Set("ParentRowNumber", parentIndex.Value + 1);
							currVal.SetNotNull("ParentGUID", parentGuid);
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
					else if (propValue is IList<Object>)
					{
						var list = propValue as IList<Object>;
						for (Int32 j = 0; j < list.Count; j++)
						{
							var currVal = list[j] as ExpandoObject;
							currVal.Set("RowNumber", j + 1);
							currVal.SetNotNull("ParentId", currentId);
							currVal.SetNotNull("ParentKey", parentKey);
							var rowGuid = currVal.GetOrCreate<Guid>("GUID", () => Guid.NewGuid());
							if (parentIndex != null)
								currVal.Set("ParentRowNumber", parentIndex.Value + 1);
							currVal.SetNotNull("ParentGUID", parentGuid);
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
					else if (propValue is ExpandoObject)
					{
						var currVal = propValue as ExpandoObject;
						if (bLast)
						{
							var propValEO = propValue as ExpandoObject;
							if (isMap)
							{
								var propValD = propValEO as IDictionary<String, Object>;
								foreach (var (k, v) in propValD)
								{
									var mapItem = v as ExpandoObject;
									mapItem.Set("CurrentKey", k);
									if (parentIndex != null)
										mapItem.Set("ParentRowNumber", parentIndex.Value + 1);
									mapItem.SetNotNull("ParentGUID", parentGuid);
									yield return mapItem;
								}
							}
							else
							{
								currVal.SetNotNull("ParentId", currentId);
								currVal.SetNotNull("ParentKey", parentKey);
								if (parentIndex != null)
									currVal.Set("ParentRowNumber", parentIndex.Value + 1);
								currentGuid = currVal.GetOrCreate<Guid>("GUID", () => Guid.NewGuid());
								currVal.SetNotNull("ParentGUID", parentGuid);
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
									var mapItem = kv.Value as ExpandoObject;
									foreach (var dx in GetDataForSave(mapItem, newPath, parentIndex: parentIndex, parentKey: kv.Key, parentGuid: currentGuid))
										yield return dx;
								}
							}
							else
							{
								currentGuid = currVal.GetOrCreate<Guid>("GUID", () => Guid.NewGuid());
								foreach (var dx in GetDataForSave(currVal, newPath, parentIndex: 0, parentKey: null, parentGuid: currentGuid))
									yield return dx;
							}
						}
					}
				}
			}
		}

		internal ITableDescription GetTableDescription()
		{
			if (_tables.Count != 1)
				throw new DataWriterException("Invalid tables for GetTableDescription");
			return new TableDescription(_tables.First().Value.Item1);
		}
	}
}