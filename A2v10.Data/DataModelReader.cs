// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using Newtonsoft.Json;

namespace A2v10.Data;

internal class DataModelReader(IDataLocalizer localizer, ITokenProvider? tokenProvider = null)
{

	const String ROOT = "TRoot";
	const String SYSTEM_TYPE = "$System";
	const String ALIASES_TYPE = "$Aliases";
    const String GROUPING_TYPE = "$Grouping";

    private readonly IDataLocalizer _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
	private readonly ITokenProvider? _tokenProvider = tokenProvider;

	private IDataModel? _dataModel;

	private readonly IdMapper _idMap = [];
	private readonly RefMapper _refMap = [];
	private readonly CrossMapper _crossMap = [];
	private readonly ExpandoObject _root = [];
	private readonly IDictionary<String, Object?> _sys = new ExpandoObject(); 

	private FieldInfo? mainElement;
    private DynamicDataGrouping? _dynamicGrouping = null;
    Dictionary<String, String>? _aliases = null;

	void AddAliasesFromReader(IDataReader rdr)
	{
		_aliases = [];
		// 1-based
		for (Int32 i = 1; i < rdr.FieldCount; i++)
			_aliases.Add(rdr.GetName(i), String.Empty);
	}

	public IDataModel DataModel
	{
		get
		{
			if (_dataModel != null)
				return _dataModel;
			if (_groupMetadata != null)
				_sys.Add("Levels", GroupMetadata.GetLevels(_groupMetadata));
			_dataModel = new DynamicDataModel(_metadata, _root, _sys as ExpandoObject)
			{
				MainElement = GetMainElement()
			};
			return _dataModel;
		}
	}

	DataElementInfo GetMainElement()
	{
		DataElementInfo dei = new();
		if (mainElement == null)
			return dei;
		if (!_metadata.TryGetValue(mainElement.Value.TypeName, out IDataMetadata? meta))
			return dei;
		if (String.IsNullOrEmpty(meta.Id))
			return dei;
		dei.Metadata = meta;
		var elem = _root.Get<ExpandoObject>(mainElement.Value.PropertyName);
		if (elem != null)
		{
			dei.Element = elem;
			dei.Id = elem.Get<Object>(meta.Id);
		}
		return dei;
	}

	String GetAlias(String name)
	{
		if (_aliases == null)
			return name;
		if (_aliases.TryGetValue(name, out String? outName))
			return outName!;
		return name;
	}

    static void AddGroupingFromReader(IDataReader _)
    {
    }

    void ProcessJsonRecord(FieldInfo fi, IDataReader rdr)
	{
		var val = rdr.GetString(0);
		val = _localizer.Localize(val);
		if (val != null)
			_root.Add(fi.PropertyName, JsonConvert.DeserializeObject<ExpandoObject>(val));

	}

    void ProcessGroupingRecord(IDataReader rdr)
    {
        _dynamicGrouping ??= new DynamicDataGrouping(_root, _metadata, this);
        _dynamicGrouping.AddGrouping(rdr);
    }

    void ProcessAliasesRecord(IDataReader rdr)
	{
		if (_aliases == null)
			throw new InvalidOperationException();
		// 1-based
		for (Int32 i = 1; i < rdr.FieldCount; i++)
		{
			String name = rdr.GetName(i);
			if (_aliases.ContainsKey(name))
			{
				_aliases[name] = rdr.GetString(i);
			}
		}
	}

	void ProcessSystemRecord(IDataReader rdr)
	{

		ExpandoObject _createModelInfo(String elem)
		{
			return _root.GetOrCreate<ExpandoObject>("$ModelInfo").GetOrCreate<ExpandoObject>(elem);
		}

		// from 1
		for (Int32 i = 1; i < rdr.FieldCount; i++)
		{
			var fn = rdr.GetName(i);
			var fi = new FieldInfo(fn);
			var dataVal = rdr.GetValue(i);
			switch (fi.SpecType)
			{
				case SpecType.Id:
					_sys.Add("Id", dataVal);
					break;
				case SpecType.PageSize:
					Int32 pageSize = (Int32) dataVal;
					if (!String.IsNullOrEmpty(fi.TypeName))
						_createModelInfo(fi.TypeName).Set("PageSize", pageSize);
					else
					{
						// for compatibility with older versions
						_sys.Add("PageSize", pageSize);
					}
					break;
				case SpecType.Offset:
					if (String.IsNullOrEmpty(fi.TypeName))
						throw new DataLoaderException("For the Offset modifier, the field name must be specified");
					Int32 offset = (Int32)dataVal;
					_createModelInfo(fi.TypeName).Set("Offset", offset);
					break;
				case SpecType.HasRows:
					if (String.IsNullOrEmpty(fi.TypeName))
						throw new DataLoaderException("For the HasRows modifier, the field name must be specified");
					if (dataVal is Int32 intHasRows)
						_createModelInfo(fi.TypeName).Set("HasRows", intHasRows != 0);
					else if (dataVal is Boolean boolHasRows)
						_createModelInfo(fi.TypeName).Set("HasRows", boolHasRows);
					else
						throw new DataLoaderException("Invalid data type for the TotalRows modifier. Expected 'int' or 'bit'");
					break;
				case SpecType.SortDir:
					if (String.IsNullOrEmpty(fi.TypeName))
						throw new DataLoaderException("For the SortDir modifier, the field name must be specified");
					String? dir = dataVal?.ToString();
					_createModelInfo(fi.TypeName).Set("SortDir", dir);
					break;
				case SpecType.SortOrder:
					if (String.IsNullOrEmpty(fi.TypeName))
						throw new DataLoaderException("For the SortOrder modifier, the field name must be specified");
					if (dataVal is String strDataOrder)
						_createModelInfo(fi.TypeName).Set("SortOrder", strDataOrder);
					break;
				case SpecType.GroupBy:
					if (String.IsNullOrEmpty(fi.TypeName))
						throw new DataLoaderException("For the Group modifier, the field name must be specified");
					if (dataVal is String strDataGroup)
						_createModelInfo(fi.TypeName).Set("GroupBy", strDataGroup);
					break;
				case SpecType.Filter:
					if (String.IsNullOrEmpty(fi.TypeName))
						throw new DataLoaderException("For the Filter modifier, the field name must be specified");
					Object? filter = dataVal;
					var xs = fi.TypeName.Split('.');
					if (xs.Length < 2)
						throw new DataLoaderException("For the Filter modifier, the field name must be as ItemProperty.FilterProperty");
					var fmi = _createModelInfo(xs[0]).GetOrCreate<ExpandoObject>("Filter");
					if (filter is DateTime)
						filter = DataHelpers.DateTime2StringWrap(filter);
					else if (filter is String strFilter)
						filter = _localizer.Localize(strFilter);
					var lastFilterKey = xs[^1];
					if (lastFilterKey == "RefId")
					{
						if (xs.Length < 4)
                            throw new DataLoaderException($"Invalid Filter for RefId modifier {fi.TypeName}");
						var mapKey = xs[^2];
						var propName = xs[^3];
                        var key = Tuple.Create<String, Object?>(mapKey, filter);
						if (_idMap.TryGetValue(key, out ExpandoObject? filterValue))
							fmi.Set(propName, filterValue);
						else
						{
							fmi.Set(propName, new ExpandoObject()
							{
								{ "Id", null },
                                { "Name", null },
                            });
						}
                    }
                    else
					{
						for (Int32 ii = 1; ii < xs.Length; ii++)
						{
							if (ii == xs.Length - 1)
								fmi.Set(xs[ii], filter);
							else
								fmi = fmi.GetOrCreate<ExpandoObject>(xs[ii]);
						}
					}
					break;
				case SpecType.ReadOnly:
					_sys.Add("ReadOnly", InternalHelpers.SqlToBoolean(dataVal));
					break;
				case SpecType.Copy:
					_sys.Add("Copy", InternalHelpers.SqlToBoolean(dataVal));
					break;
				case SpecType.Permissions:
					if (String.IsNullOrEmpty(fi.TypeName))
						throw new DataLoaderException("For the Permissions modifier, the field name must be specified");
					Object perm = dataVal;
					var xp = fi.TypeName.Split('.');
					if (xp.Length < 1)
						throw new DataLoaderException("For the Permissions modifier, the field name must be as ItemProperty");
					var fmp = _createModelInfo(fi.TypeName);
					fmp.Set("Permissions", perm);
					break;
				default:
					_sys.Add(fn, dataVal);
					break;
			}
		}
	}

	public void ProcessOneRecord(IDataReader rdr)
	{
		var rootFI = new FieldInfo(GetAlias(rdr.GetName(0)));
		if (rootFI.TypeName == SYSTEM_TYPE)
		{
			ProcessSystemRecord(rdr);
			return;
		}
		else if (rootFI.TypeName == ALIASES_TYPE)
		{
			ProcessAliasesRecord(rdr);
			return;
		}
        else if (rootFI.TypeName == GROUPING_TYPE)
        {
            ProcessGroupingRecord(rdr);
        }
        if (rootFI.IsJson)
		{
			ProcessJsonRecord(rootFI, rdr);
			return;
		}
		rootFI.CheckValid();
		ExpandoObject currentRecord = [];
		Boolean bAdded = false;
		Boolean bAddMap = false;
		Boolean bAddRow = false;
		Boolean bAddColumn = false;
		Boolean bAddCell = false;
		Boolean bAddProp = false;
		Object? id = null;
		Object? key = null;
		String? keyName = null;
		Object? propVal = null;
		Object? index = null;
		String? indexName = null;
		Object? column = null;
		String? columnName = null;
		Int32 rowCount = 0;
		Boolean bHasRowCount = false;
		List<Boolean>? groupKeys = null;
		String mapPropName = String.Empty;
		// from 1!
		for (Int32 i = 1; i < rdr.FieldCount; i++)
		{
			var dataVal = rdr.GetValue(i);
			if (dataVal == DBNull.Value)
				dataVal = null;
			var fn = GetAlias(rdr.GetName(i));
			var fi = new FieldInfo(fn);
			if (fi.IsGroupMarker)
			{
				groupKeys ??= [];
				Boolean bVal = (dataVal != null) && (dataVal.ToString() == "1");
				groupKeys.Add(bVal);
				continue;
			}
			else if (fi.IsJson)
			{
				if (dataVal != null)
				{
					var strloc = _localizer.Localize(dataVal.ToString());
					if (strloc != null)
						AddValueToRecord(currentRecord, fi, JsonConvert.DeserializeObject<ExpandoObject>(strloc));
				}
				continue;
			}
			else if (fi.IsPermissions)
			{
				fi.CheckPermissionsName();
			}

			if (fi.IsToken)
			{
				if (_tokenProvider == null)
					dataVal = null;
				if (dataVal is Guid dataGuid)
					dataVal = _tokenProvider!.GenerateToken(dataGuid);
				else if (dataVal != null)
					throw new DataLoaderException("Token must be an guid");
			}


			AddValueToRecord(currentRecord, fi, dataVal);
			if (fi.IsCrossArray)
				_crossMap.AddParentRecord(rootFI.TypeName, fi.PropertyName, currentRecord);
			if (fi.IsRowCount)
			{
				if (dataVal is Int32 int32Val)
					rowCount = int32Val;
				else
					throw new DataLoaderException("Invalid field type for !!RowCount");
				bHasRowCount = true;
			}
			if (fi.IsId)
			{
				if (fi.IsComplexField)
					_idMap.Add(fi.TypeName, dataVal, currentRecord);
				else
				{
					_idMap.Add(rootFI.TypeName, dataVal, currentRecord);
					id = dataVal;
				}
			}
			else if (fi.IsKey)
			{
				keyName = fi.PropertyName;
				key = dataVal;
			}
			else if (fi.IsProp)
			{
				propVal = dataVal;
				bAddProp = true;
			}
			else if (fi.IsIndex)
			{
				indexName = fi.PropertyName;
				index = dataVal;
			}
			else if (fi.IsColumnId)
			{
				columnName = fi.TypeName;
				column = dataVal;
			}
			if (fi.IsParentId)
			{
				if (rootFI.IsArray)
				{
					if (dataVal == null)
						continue;
					AddRecordToArray(fi.TypeName, dataVal, currentRecord, rootFI.TypeName);
					if (!rootFI.IsVisible)
						bAdded = true;
				}
				else if (rootFI.IsTree)
				{
					if (fi.IsParentIdSelf(rootFI))
					{
						if (dataVal == null)
						{
							if (!String.IsNullOrEmpty(rootFI.PropertyName))
								_root.AddToArray(rootFI.PropertyName, currentRecord);
						}
						else
							AddRecordToArray(fi.TypeName, dataVal, currentRecord);
						bAdded = true;
					}
					else
					{
						// Add Record to parent
						if (dataVal != null)
						{
							AddRecordToArray(fi.TypeName, dataVal, currentRecord);
						}
					}
				}
				else if (rootFI.IsObject || rootFI.IsSheet)
				{
					if (bAddProp)
					{
						// nested object
						if (dataVal == null)
							throw new DataLoaderException("NestedObject: dataVal is null");
						if (propVal == null)
							throw new DataLoaderException("NestedObject: propVal is null");
						AddObjectToRecordProperty(fi.TypeName, dataVal, propVal.ToString()!, currentRecord);
					}
					else
					{
						// nested object
						if (dataVal == null)
							throw new DataLoaderException("NestedObject: dataVal is null");
						AddObjectToRecord(fi.TypeName, dataVal, currentRecord);
					}
					if (!rootFI.IsVisible)
						bAdded = true;
				}
				else if (rootFI.IsMapObject)
				{
					if (!rootFI.IsVisible)
					{
						bAdded = true;
						// defer. key needed
						bAddMap = true;
						mapPropName = fi.TypeName;
						id = dataVal;
					}
				}
				else if (rootFI.IsRows)
				{
					if (!rootFI.IsVisible)
					{
						bAdded = true;
						mapPropName = fi.TypeName;
						bAddRow = true;
						id = dataVal;
					}
				}
				else if (rootFI.IsColumns)
				{
					if (!rootFI.IsVisible)
					{
						bAdded = true;
						mapPropName = fi.TypeName;
						bAddColumn = true;
						id = dataVal;
					}
				}
				else if (rootFI.IsCells)
				{
					if (!rootFI.IsVisible)
					{
						bAdded = true;
						mapPropName = fi.TypeName;
						bAddCell = true;	
						id = dataVal;
					}
				}
				else if (rootFI.IsCrossArray || rootFI.IsCrossObject)
				{
					if (key == null || keyName == null)
						throw new DataLoaderException("CrossArray or CrossObject: keyName or key are null");
					if (dataVal != null)
						AddRecordToCross(fi.TypeName, dataVal, currentRecord, key, keyName, rootFI);
				}
			}
		}
		if (bAddMap)
		{
			if (key == null)
				throw new InvalidProgramException("AddToMap: key is null");
			AddMapToRecord(mapPropName, id, key, currentRecord);
		}
		else if (bAddRow)
		{
			if (index == null)
				throw new InvalidProgramException("AddSheetRow: index is null");
			if (id == null)
				throw new InvalidProgramException("AddSheetRow: ParentId is null");
			if (indexName == null)
				throw new InvalidProgramException("AddSheetRow: IndexName is null");
			AddRecordToRows(mapPropName, id, currentRecord, index, indexName);
		}
		else if (bAddColumn)
		{
			if (id == null)
				throw new InvalidProgramException("AddSheetColumn: ParentId is null");
			if (key == null)
				throw new InvalidProgramException("AddSheetColumn: Key is null");
			if (keyName == null)
				throw new InvalidProgramException("AddSheetColumn: KeyName is null");
			AddRecordToColumns(mapPropName, id, currentRecord, key.ToString()!, keyName);
		}
		else if (bAddCell)
		{
			if (id == null)
				throw new InvalidProgramException("AddSheetCell: ParentId is null");
			if (column == null)
				throw new InvalidProgramException("AddSheetCell: Column is null");
			if (columnName == null)
				throw new InvalidProgramException("AddSheetCell: ColumnName is null");
			AddRecordToCells(mapPropName, id, currentRecord, column, columnName);
		}
		if (!bAdded)
		{
			if (rootFI.IsGroup)
			{
				if (groupKeys == null)
					throw new InvalidProgramException("IsGroup: groupKeys is null");
				AddRecordToGroup(currentRecord, rootFI, groupKeys);
			}
			else
			{
				AddRecordToModel(currentRecord, rootFI, id, key);
			}
		}
		else
			CheckRecordRef(currentRecord, rootFI, id);
		if (bHasRowCount)
		{
			AddRowCount(rootFI.PropertyName, rowCount);
		}
	}

	public void ProcessOneMetadata(IDataReader rdr)
	{
		if (rdr.FieldCount == 0)
			return;
		// first field = self object
		var schemaTable = rdr.GetSchemaTable() 
			?? throw new DataLoaderException($"Invalid schema table");
        var firstFieldName = GetAlias(rdr.GetName(0));
		var objectDef = new FieldInfo(firstFieldName);
		objectDef.CheckTypeName(); // for first field only
		if (objectDef.TypeName == SYSTEM_TYPE)
			return; // not needed
		else if (objectDef.TypeName == ALIASES_TYPE)
		{
			AddAliasesFromReader(rdr);
			return;
		}
        else if (objectDef.TypeName == GROUPING_TYPE)
        {
            AddGroupingFromReader(rdr);
            return;
        }
        if (objectDef.FieldType == FieldType.Scalar)
		{
			throw new DataLoaderException($"Invalid element type: '{firstFieldName}'");
		}
		var rootMetadata = GetOrCreateMetadata(ROOT);
		rootMetadata.AddField(0, objectDef, DataType.Undefined, SqlDataType.Unknown);
		// other fields = object fields
		var typeMetadata = GetOrCreateMetadata(objectDef.TypeName);
		if (objectDef.IsMain && mainElement == null)
		{
			mainElement = objectDef;
			rootMetadata.MainObject = objectDef.PropertyName;
		}
		if (objectDef.IsArray || objectDef.IsTree || objectDef.IsMap || objectDef.IsLookup || objectDef.IsRows || objectDef.IsColumns || objectDef.IsCells)
			typeMetadata.IsArrayType = true;
		if (objectDef.IsGroup)
			typeMetadata.IsGroup = true;
		if ((objectDef.IsArray || objectDef.IsTree) && objectDef.IsVisible)
			_root.AddToArray(objectDef.PropertyName, null); // empty record
		if (objectDef.IsLookup)
			ProcessLookupMetadata(objectDef); // for root here
		Boolean hasRowCount = false;
		for (Int32 i = 1; i < rdr.FieldCount; i++)
		{
			String fieldName = GetAlias(rdr.GetName(i));
			var fieldDef = new FieldInfo(fieldName);
			if (fieldDef.IsGroupMarker)
			{
				GetOrCreateGroupMetadata(objectDef.TypeName).AddMarkerMetadata(fieldDef.PropertyName);
				continue;
			}
			if (fieldDef.IsRowCount)
				hasRowCount = true;
			if (fieldDef.IsPermissions)
			{
				fieldDef.CheckPermissionsName();
				typeMetadata.AddField(i, fieldDef, DataType.Number, 0);
				continue;
			}
			if (fieldDef.IsCrossArray)
			{
				typeMetadata.AddCross(fieldDef.PropertyName, null);
			}
			if (!fieldDef.IsVisible)
				continue;
			DataType dt = rdr.GetFieldType(i).Name.TypeName2DataType();
			SqlDataType sqlDataType = rdr.GetDataTypeName(i).SqlTypeName2SqlDataType();

			Int32 fieldLength = 0;
			if (dt == DataType.String)
				fieldLength = (Int32) schemaTable.Rows[i]["ColumnSize"];

			if (fieldDef.IsComplexField)
				ProcessComplexMetadata(fieldDef, typeMetadata, dt, sqlDataType, fieldLength);
			else if (fieldDef.IsMapObject)
				ProcessMapObjectMetadata(fieldDef, typeMetadata);
			else if (fieldDef.IsLookup)
				ProcessLookupMetadata(fieldDef);
			else
			{
				var fm = typeMetadata.AddField(i, fieldDef, dt, sqlDataType, fieldLength);
				if (fieldDef.IsNestedType)
				{
					// create metadata for nested object or array
					String nestedTypeName = fieldDef.TypeName;
					if (String.IsNullOrEmpty(nestedTypeName))
						throw new DataLoaderException($"Type name for '{fieldName}' is required");
					var tm = GetOrCreateMetadata(nestedTypeName);
					if (fieldDef.IsArray || fieldDef.IsCrossArray)
						tm.IsArrayType = true;
				}
				if (fieldDef.IsJson)
				{
					if (fm == null)
						throw new DataLoaderException("Json. Invalid fieldDef");
					fm.IsJson = true;
				}
			}
		}
		if (hasRowCount)
			_root.AddChecked($"{objectDef.PropertyName}.$RowCount", 0);
	}

	Dictionary<String, GroupMetadata>? _groupMetadata;
	private readonly Dictionary<String, IDataMetadata> _metadata = [];


	ElementMetadata? GetMetadata(String typeName)
	{
		if (_metadata.TryGetValue(typeName, out IDataMetadata? elemMeta))
			return elemMeta as ElementMetadata;
		return null;
	}

	ElementMetadata GetOrCreateMetadata(String typeName)
	{
		if (_metadata.TryGetValue(typeName, out IDataMetadata? elemMeta))
		{
			if (elemMeta is ElementMetadata realElem)
				return realElem;
			throw new DataLoaderException("Invalid element metadata");
		}
		var newMeta = new ElementMetadata();
		_metadata.Add(typeName, newMeta);
		return newMeta;
	}

	internal GroupMetadata GetOrCreateGroupMetadata(String typeName)
	{
		_groupMetadata ??= [];
		if (_groupMetadata.TryGetValue(typeName, out GroupMetadata? groupMeta))
			return groupMeta;
		groupMeta = new GroupMetadata();
		_groupMetadata.Add(typeName, groupMeta);
		return groupMeta;
	}

	void AddValueToRecord(IDictionary<String, Object?> record, FieldInfo field, Object? value)
	{
		if (!field.IsVisible)
			return;
		if (field.IsArray)
			record.Add(field.PropertyName, new List<ExpandoObject>());
		else if (field.IsComplexField)
		{
			var propNames = field.PropertyName.Split('.');
			if (propNames.Length != 2)
				throw new DataLoaderException($"Invalid complex name {field.PropertyName}");
			var innerObj = record.GetOrCreate(propNames[0]);
			if (value is String strVal)
				innerObj.Add(propNames[1], _localizer.Localize(strVal));
			else
				innerObj.Add(propNames[1], value);
		}
		else if (field.IsRefId)
		{
			var refValue = new ExpandoObject();
			_refMap.Add(field.TypeName, value, refValue);
			record.Add(field.PropertyName, refValue);
		}
		else if (value is String strVal)
			record.Add(field.PropertyName, _localizer.Localize(strVal));
		else if (field.IsUtc && value is DateTime dt)
			record.Add(field.PropertyName, DateTime.SpecifyKind(dt.ToLocalTime(), DateTimeKind.Unspecified));
		else
			record.Add(field.PropertyName, value);
	}

	void AddRecordToGroup(ExpandoObject currentRecord, FieldInfo field, List<Boolean> groupKeys)
	{
		if (groupKeys == null)
			throw new DataLoaderException($"There is no groups property for '{field.TypeName}'");
		ElementMetadata elemMeta = GetOrCreateMetadata(field.TypeName);
		if (String.IsNullOrEmpty(elemMeta.Items))
			throw new DataLoaderException($"There is no 'Items' property for '{field.TypeName}'");
		GroupMetadata groupMeta = GetOrCreateGroupMetadata(field.TypeName);
		if (groupMeta.IsRoot(groupKeys))
		{
			_root.Add(field.PropertyName, currentRecord);
			groupMeta.CacheElement(GroupMetadata.RootKey, currentRecord); // current
		}
		else
		{
			// item1 - elemKey, item2 -> parentKey
			var keys = groupMeta.GetKeys(groupKeys, currentRecord);
			var parentRec = groupMeta.GetCachedElement(keys.Item2); // parent
			parentRec.AddToArray(elemMeta.Items, currentRecord);
			if (!groupMeta.IsLeaf(groupKeys))
				groupMeta.CacheElement(keys.Item1, currentRecord); // current
		}
	}

	void AddRecordToCross(String propName, Object id, ExpandoObject currentRecord, Object keyProp, String keyName, FieldInfo rootFI)
	{
		if (keyProp == null)
			throw new DataLoaderException("Key not found in cross object");
		var pxa = propName.Split('.'); // <Type>.PropName
		/*0-key, 1-Property (optional) */
		var key = Tuple.Create<String, Object?>(pxa[0], id);
		if (!_idMap.TryGetValue(key, out ExpandoObject? mapObj))
			throw new DataLoaderException($"Property '{propName}'. Object {pxa[0]} (Id={id}) not found");
		mapObj.AddToCross(pxa[1], currentRecord, keyProp.ToString()!);
		_crossMap.Add(propName, pxa[1], mapObj, keyProp.ToString()!, keyName, rootFI, id);
	}

	void AddRecordToArray(String propName, Object id, ExpandoObject currentRecord, String? rootTypeName = null)
	{
		var pxa = propName.Split('.'); // <Type>.PropName
		/*0-key, 1-Property (optional)*/
		var key = Tuple.Create<String, Object?>(pxa[0], id);
		if (!_idMap.TryGetValue(key, out ExpandoObject? mapObj))
			throw new DataLoaderException($"Property '{propName}'. Object {pxa[0]} (Id={id}) not found");
		if (pxa.Length == 1)
		{
			// old syntax. Find property name by rootTypeName
			if (rootTypeName == null)
				throw new DataLoaderException($"AddRecordToArray. Invalid RootTypeName");
			String oldTypeName = pxa[0];
			if (String.IsNullOrEmpty(oldTypeName))
				throw new DataLoaderException($"AddRecordToArray. Type name for {propName} is required");
			var elemData = GetOrCreateMetadata(oldTypeName);
			String? fieldName = elemData.FindPropertyByType(rootTypeName);
			if (String.IsNullOrEmpty(fieldName))
				throw new DataLoaderException($"AddRecordToArray. Field for type '{rootTypeName}' not found in '{propName}' object");
			mapObj.AddToArray(fieldName, currentRecord);
		}
		else if (pxa.Length != 2)
			throw new DataLoaderException($"Invalid field name '{propName}' for array. 'TypeName.PropertyName' expected");
		else
			mapObj.AddToArray(pxa[1], currentRecord);
	}

	void AddRecordToRows(String propName, Object? id, ExpandoObject currentRecord, Object index, String indexName)
	{
		var pxa = propName.Split('.'); // <Type>.PropName
		if (pxa.Length != 2)
			throw new DataLoaderException($"Invalid field name '{propName}' for array. 'TypeName.PropertyName' expected");
		var int32Index = Convert.ToInt32(index);
		if (int32Index < 0)
			throw new DataLoaderException($"Invalid index value ({int32Index}). Must be > 0");
		var srcKey = Tuple.Create(pxa[0], id);
		if (!_idMap.TryGetValue(srcKey, out ExpandoObject? mapObj))
			throw new DataLoaderException($"Property '{propName}'. Object {pxa[0]} (Id={id}) not found");
		mapObj.AddToArrayIndex(pxa[1], currentRecord, int32Index - 1, indexName);
	}

	void AddRecordToColumns(String propName, Object? id, ExpandoObject currentRecord, String key, String keyName)
	{
		var pxa = propName.Split('.'); // <Type>.PropName
		if (pxa.Length != 2)
			throw new DataLoaderException($"Invalid field name '{propName}' for array. 'TypeName.PropertyName' expected");
		var srcKey = Tuple.Create(pxa[0], id);
		if (!_idMap.TryGetValue(srcKey, out ExpandoObject? sheetObj))
			throw new DataLoaderException($"Property '{propName}'. Object {pxa[0]} (Id={id}) not found");
		sheetObj.AddToArrayIndexKey(pxa[1], currentRecord, key, keyName);
	}

	void AddRecordToCells(String propName, Object? id, ExpandoObject currentRecord, Object? column, String columnName)
	{
		var pxa = propName.Split('.'); // <Type>.PropName
		if (pxa.Length != 2)
			throw new DataLoaderException($"Invalid field name '{propName}' for array. 'TypeName.PropertyName' expected");
		var srcKey = Tuple.Create(pxa[0], id);
		if (!_idMap.TryGetValue(srcKey, out ExpandoObject? rowObj))
			throw new DataLoaderException($"Property '{propName}'. Object {pxa[0]} (Id={id}) not found");
		// row was found
		var cxa = columnName.Split(".");
		if (cxa.Length != 2)
			throw new DataLoaderException($"Invalid Column name '{column}' for sheet. 'TypeName.PropertyName' expected");
		var colKey = Tuple.Create(cxa[0], column);
		if (!_idMap.TryGetValue(colKey, out ExpandoObject? colObj))
			throw new DataLoaderException($"Property '{columnName}'. Object {cxa[0]} (Id={column}) not found");
		// column was found
		var columnKey = colObj.Get<String>(cxa[1]) 
			?? throw new DataLoaderException("Column Key is null");
		rowObj.AddToArrayIndexCell(pxa[1], currentRecord, columnKey);
	}

	void AddMapToRecord(String propName, Object? id, Object key, ExpandoObject currentRecord)
	{
		if (key == null)
			throw new DataLoaderException($"There is no 'Key' property for field '{propName}'");
		var pxa = propName.Split('.'); // <Type>.PropName
		if (pxa.Length != 2)
			throw new DataLoaderException($"Invalid field name '{propName}' for array. 'TypeName.PropertyName' expected");
		/*0-key, 1-Property*/
		var srcKey = Tuple.Create(pxa[0], id);
		if (!_idMap.TryGetValue(srcKey, out ExpandoObject? mapObj))
			throw new DataLoaderException($"Property '{propName}'. Object {pxa[0]} (Id={id}) not found");
		var innerObject = mapObj.Get<ExpandoObject>(pxa[1]);
		if (innerObject == null)
		{
			innerObject = [];
			mapObj.Set(pxa[1], innerObject);
		}
		innerObject.Set(key.ToString()!, currentRecord);
	}

	void AddObjectToRecord(String propName, Object id, ExpandoObject currentRecord)
	{
		var pxa = propName.Split('.'); // <Type>.PropName
		if (pxa.Length != 2)
			throw new DataLoaderException($"Invalid field name '{propName}' for array. 'TypeName.PropertyName' expected");
		/*0-key, 1-Property*/
		var key = Tuple.Create<String, Object?>(pxa[0], id);
		if (!_idMap.TryGetValue(key, out ExpandoObject? mapObj))
			throw new DataLoaderException($"Property '{propName}'. Object {pxa[0]} (Id={id}) not found");
		mapObj.Set(pxa[1], currentRecord);
		_refMap.Correct(key);
	}

	void AddObjectToRecordProperty(String propName, Object id, String prop, ExpandoObject currentRecord)
	{
		var pxa = propName.Split('.'); // <Type>.PropName
		if (pxa.Length != 2)
			throw new DataLoaderException($"Invalid field name '{propName}' for array. 'TypeName.PropertyName' expected");
		/*0-key, 1-Property*/
		var key = Tuple.Create<String, Object?>(pxa[0], id);
		if (!_idMap.TryGetValue(key, out ExpandoObject? mapObj))
			throw new DataLoaderException($"Property '{propName}'. Object {pxa[0]} (Id={id}) not found");
		// ensure object
		var resObj = mapObj.EnsureObject(pxa[1]);
		resObj.Set(prop, currentRecord);
	}

	void AddRecordToModel(ExpandoObject currentRecord, FieldInfo field, Object? id, Object? key)
	{
		if (field.IsArray)
		{
			_refMap.MergeObject(field.TypeName, id, currentRecord);
			_root.AddToArray(field.PropertyName, currentRecord);
		}
		else if (field.IsTree)
			_root.AddToArray(field.PropertyName, currentRecord);
		else if (field.IsObject || field.IsSheet)
			_root.Add(field.PropertyName, currentRecord);
		else if (field.IsLookup)
		{
			var leo = _root.CreateOrAddObject(field.PropertyName);
			String fieldKey = key != null ? key.ToString()! :
				throw new InvalidOperationException($"Key for lookup is null");
			leo.Add(fieldKey, currentRecord);
			var lookupMeta = GetOrCreateMetadata($"{field.TypeName}Map");
			lookupMeta.AddField(-1, new FieldInfo($"{fieldKey}!{field.TypeName}"), DataType.Undefined, SqlDataType.Unknown);
		}
		else if (field.IsMap)
		{
			_refMap.MergeObject(field.TypeName, id, currentRecord);
			if (field.IsVisible)
			{
				if (key != null)
					_root.AddToMap(field.PropertyName, currentRecord, key.ToString()!);
				else if (id != null)
					_root.AddToArray(field.PropertyName, currentRecord);
				else
					throw new DataLoaderException("For Map objects, the property 'Key' or 'Id' is required");
			}
		}
	}

	void AddRowCount(String propertyName, Int32 rowCount)
	{
		var pn = $"{propertyName}.$RowCount";
		// added in metadata
		// _root.AddChecked(pn, rowCount);
		_root.Set(pn, rowCount);
	}

	void CheckRecordRef(ExpandoObject currentRecord, FieldInfo field, Object? id)
	{
		if (field.IsArray || field.IsMap)
			_refMap.MergeObject(field.TypeName, id, currentRecord);
	}

	void ProcessComplexMetadata(FieldInfo fieldInfo, ElementMetadata elem, DataType dt, SqlDataType sqlType, Int32 fieldLen)
	{
		// create metadata for nested type
		var innerElem = GetOrCreateMetadata(fieldInfo.TypeName);
		var fna = fieldInfo.PropertyName.Split('.');
		if (fna.Length != 2)
			throw new DataLoaderException($"Invalid complex name {fieldInfo.PropertyName}");
		var fi = new FieldInfo($"{fna[0]}!{fieldInfo.TypeName}");
		fi.CheckTypeName();
		elem.AddField(0, fi, DataType.Undefined, SqlDataType.Unknown);
		innerElem.AddField(0, new FieldInfo(fieldInfo, fna[1]), dt, sqlType, fieldLen);
	}

	void ProcessLookupMetadata(FieldInfo fieldInfo)
	{
		var mapType = fieldInfo.TypeName + "Map";
		var innerElem = GetOrCreateMetadata(mapType);
		innerElem.MapItemType = fieldInfo.TypeName;
	}

	void ProcessMapObjectMetadata(FieldInfo fieldInfo, ElementMetadata elem)
	{
		var mapType = fieldInfo.TypeName + "Map";
		var innerElem = GetOrCreateMetadata(mapType);
		innerElem.MapItemType = fieldInfo.TypeName;
		if (fieldInfo.MapFields != null)
		{
			foreach (var f in fieldInfo.MapFields)
				innerElem.AddField(0, new FieldInfo($"{f}!{fieldInfo.TypeName}"), DataType.Undefined, SqlDataType.Unknown);
		}
		elem.AddField(0, new FieldInfo($"{fieldInfo.PropertyName}!{mapType}"), DataType.Undefined, SqlDataType.Unknown);
	}

	public void PostProcess()
	{
		_crossMap.Transform();
		foreach (var (k, v) in _crossMap)
		{
			Int32 pos = k.IndexOf('.');
			String typeName = k[..pos];
			var typeMeta = GetMetadata(typeName);
			if (typeMeta == null)
				throw new DataLoaderException($"Invalid type name {typeMeta}");
			var crossKeys = v.GetCross();
			var prop = v.TargetProp;

			if (v.IsArray)
			{
				typeMeta.AddCross(prop, crossKeys);
			}
			else
			{
				// CrossObject. TCross.props = Keys with TCross type
				var crossObjType = $"{v.CrossType}Object";
				var crossMeta = GetOrCreateMetadata(crossObjType);
				foreach (var key in crossKeys)
				{
					if (key == null)
						throw new InvalidOperationException("CrossKey not defined");
					var fi = new FieldInfo(key, v.CrossType);
					crossMeta.AddField(0, fi, DataType.String, SqlDataType.String);
				}
				typeMeta.SetCrossObject(prop, crossObjType);
				typeMeta.AddCross(prop, crossKeys);
			}
		}
        _dynamicGrouping?.Process(_crossMap);
    }
}

