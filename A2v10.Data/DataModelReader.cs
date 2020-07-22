// Copyright © 2015-2019 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Reflection;

namespace A2v10.Data
{
	/*
	 * TODO: Map with keys: Metadata
	 */

	public class DataModelReader
	{

		const String ROOT = "TRoot";
		const String SYSTEM_TYPE = "$System";
		const String ALIASES_TYPE = "$Aliases";

		private IDataModel _dataModel;
		private readonly IDataLocalizer _localizer;
		private readonly IdMapper _idMap = new IdMapper();
		private readonly RefMapper _refMap = new RefMapper();
		private readonly CrossMapper _crossMap = new CrossMapper();
		private readonly ExpandoObject _root = new ExpandoObject();
		private readonly IDictionary<String, Object> _sys = new ExpandoObject() as IDictionary<String, Object>;
		FieldInfo? mainElement;

		public DataModelReader(IDataLocalizer localizer)
		{
			_localizer = localizer;
			if (localizer == null)
				throw new ArgumentNullException(nameof(localizer));
		}

#pragma warning disable CA1822 // Mark members as static
		public void SetParameters(SqlParameterCollection prms, Object values)
#pragma warning restore CA1822 // Mark members as static
		{
			if (values == null)
				return;
			if (values is ExpandoObject)
			{
				foreach (var e in values as IDictionary<String, Object>)
				{
					var val = e.Value;
					if (val != null)
					{
						prms.AddWithValue($"@{e.Key}", val);
					}
				}
			}
			else
			{
				var props = values.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
				foreach (var prop in props)
				{
					var val = prop.GetValue(values);
					if (val != null)
						prms.AddWithValue($"@{prop.Name}", val);
				}
			}
		}

		Dictionary<String, String> _aliases;

		void AddAliasesFromReader(IDataReader rdr)
		{
			_aliases = new Dictionary<String, String>();
			// 1-based
			for (Int32 i = 1; i < rdr.FieldCount; i++)
			{
				_aliases.Add(rdr.GetName(i), null);
			}
		}

		public void ProcessMetadataAliases(IDataReader rdr)
		{
			if (rdr.FieldCount == 0)
				return;
			var objectDef = new FieldInfo(GetAlias(rdr.GetName(0)));
			if (objectDef.TypeName == ALIASES_TYPE)
				AddAliasesFromReader(rdr);
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
			DataElementInfo dei = new DataElementInfo();
			if (mainElement == null)
				return dei;
			if (!_metadata.TryGetValue(mainElement.Value.TypeName, out IDataMetadata meta))
				return dei;
			if (String.IsNullOrEmpty(meta.Id))
				return dei;
			dei.Metadata = meta;
			var elem = _root.Get<ExpandoObject>(mainElement.Value.PropertyName);
			dei.Element = elem;
			dei.Id = elem.Get<Object>(meta.Id);
			return dei;
		}

		String GetAlias(String name)
		{
			if (_aliases == null)
				return name;
			if (_aliases.TryGetValue(name, out String outName))
				return outName;
			return name;
		}

		void ProcessJsonRecord(FieldInfo fi, IDataReader rdr)
		{
			var val = rdr.GetString(0);
			val = _localizer.Localize(val);
			_root.Add(fi.PropertyName, JsonConvert.DeserializeObject<ExpandoObject>(val));

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
						Int32 pageSize = (Int32)dataVal;
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
						String dir = dataVal.ToString();
						_createModelInfo(fi.TypeName).Set("SortDir", dir);
						break;
					case SpecType.SortOrder:
						if (String.IsNullOrEmpty(fi.TypeName))
							throw new DataLoaderException("For the SortOrder modifier, the field name must be specified");
						String order = dataVal.ToString();
						_createModelInfo(fi.TypeName).Set("SortOrder", order);
						break;
					case SpecType.GroupBy:
						if (String.IsNullOrEmpty(fi.TypeName))
							throw new DataLoaderException("For the Group modifier, the field name must be specified");
						String group = dataVal.ToString();
						_createModelInfo(fi.TypeName).Set("GroupBy", group);
						break;
					case SpecType.Filter:
						if (String.IsNullOrEmpty(fi.TypeName))
							throw new DataLoaderException("For the Filter modifier, the field name must be specified");
						Object filter = dataVal;
						var xs = fi.TypeName.Split('.');
						if (xs.Length < 2)
							throw new DataLoaderException("For the Filter modifier, the field name must be as ItemProperty.FilterProperty");
						var fmi = _createModelInfo(xs[0]).GetOrCreate<ExpandoObject>("Filter");
						if (filter is DateTime)
							filter = DataHelpers.DateTime2StringWrap(filter);
						else if (filter is String)
							filter = _localizer.Localize(filter.ToString());
						for (Int32 ii = 1; ii < xs.Length; ii++)
						{
							if (ii == xs.Length - 1)
								fmi.Set(xs[ii], filter);
							else
								fmi = fmi.GetOrCreate<ExpandoObject>(xs[ii]);
						}
						break;
					case SpecType.ReadOnly:
						_sys.Add("ReadOnly", DataHelpers.SqlToBoolean(dataVal));
						break;
					case SpecType.Copy:
						_sys.Add("Copy", DataHelpers.SqlToBoolean(dataVal));
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
			if (rootFI.IsJson)
			{
				ProcessJsonRecord(rootFI, rdr);
				return;
			}
			rootFI.CheckValid();
			var currentRecord = new ExpandoObject();
			Boolean bAdded = false;
			Boolean bAddMap = false;
			Object id = null;
			Object key = null;
			Int32 rowCount = 0;
			Boolean bHasRowCount = false;
			List<Boolean> groupKeys = null;
			String mapPropName = null;
			// from 1!
			for (Int32 i = 1; i < rdr.FieldCount; i++)
			{
				var dataVal = rdr.GetValue(i);
				if (dataVal == DBNull.Value)
					dataVal = null;
				var fn = GetAlias(rdr.GetName(i));
				FieldInfo fi = new FieldInfo(fn);
				if (fi.IsGroupMarker)
				{
					if (groupKeys == null)
						groupKeys = new List<Boolean>();
					Boolean bVal = (dataVal != null) && (dataVal.ToString() == "1");
					groupKeys.Add(bVal);
					continue;
				}
				else if (fi.IsJson)
				{
					if (dataVal != null)
						AddValueToRecord(currentRecord, fi, JsonConvert.DeserializeObject<ExpandoObject>(_localizer.Localize(dataVal.ToString())));
					continue;
				}
				else if (fi.IsPermissions)
				{
					fi.CheckPermissionsName();
				}
				AddValueToRecord(currentRecord, fi, dataVal);
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
					key = dataVal;
				}
				if (fi.IsParentId)
				{
					if (rootFI.IsArray)
					{
						AddRecordToArray(fi.TypeName, dataVal, currentRecord, rootFI.TypeName);
						if (!rootFI.IsVisible)
							bAdded = true;
					}
					else if (rootFI.IsTree)
					{
						if (dataVal == null)
							_root.AddToArray(rootFI.PropertyName, currentRecord);
						else
							AddRecordToArray(fi.TypeName, dataVal, currentRecord);
						bAdded = true;

					}
					else if (rootFI.IsObject)
					{
						// nested object
						AddObjectToRecord(fi.TypeName, dataVal, currentRecord);
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
					else if (rootFI.IsCrossArray || rootFI.IsCrossObject)
					{
						AddRecordToCross(fi.TypeName, dataVal, currentRecord, key, rootFI);
					}
				}
			}
			if (bAddMap)
			{
				AddMapToRecord(mapPropName, id, key, currentRecord);
			}
			if (!bAdded)
			{
				if (rootFI.IsGroup)
					AddRecordToGroup(currentRecord, rootFI, groupKeys);
				else
					AddRecordToModel(currentRecord, rootFI, id, key);
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
			var schemaTable = rdr.GetSchemaTable();
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
			if (objectDef.FieldType == FieldType.Scalar)
			{
				throw new DataLoaderException($"Invalid element type: '{firstFieldName}'");
			}
			var rootMetadata = GetOrCreateMetadata(ROOT);
			rootMetadata.AddField(objectDef, DataType.Undefined);
			// other fields = object fields
			var typeMetadata = GetOrCreateMetadata(objectDef.TypeName);
			if (objectDef.IsMain && mainElement == null)
			{
				mainElement = objectDef;
				rootMetadata.MainObject = objectDef.PropertyName;
			}
			if (objectDef.IsArray || objectDef.IsTree || objectDef.IsMap)
				typeMetadata.IsArrayType = true;
			if (objectDef.IsGroup)
				typeMetadata.IsGroup = true;
			if (objectDef.IsArray && objectDef.IsVisible)
				_root.AddToArray(objectDef.PropertyName, null); // empty record
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
					typeMetadata.AddField(fieldDef, DataType.Number, 0);
					continue;
				}
				if (fieldDef.IsCrossArray)
				{
					typeMetadata.AddCross(fieldDef.PropertyName, null);
				}
				if (!fieldDef.IsVisible)
					continue;
				DataType dt = rdr.GetFieldType(i).Name.TypeName2DataType();

				Int32 fieldLength = 0;
				if (dt == DataType.String)
					fieldLength = (Int32)schemaTable.Rows[i]["ColumnSize"];

				if (fieldDef.IsComplexField)
					ProcessComplexMetadata(fieldDef, typeMetadata, dt, fieldLength);
				else if (fieldDef.IsMapObject)
					ProcessMapObjectMetadata(fieldDef, typeMetadata);
				else
				{
					var fm = typeMetadata.AddField(fieldDef, dt, fieldLength);
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
						fm.IsJson = true;
				}
			}
			if (hasRowCount)
				_root.AddChecked($"{objectDef.PropertyName}.$RowCount", 0);
		}

		IDictionary<String, GroupMetadata> _groupMetadata;
		IDictionary<String, IDataMetadata> _metadata;


		ElementMetadata GetMetadata(String typeName)
		{
			if (_metadata != null && _metadata.TryGetValue(typeName, out IDataMetadata elemMeta))
				return elemMeta as ElementMetadata;
			return null;
		}

		ElementMetadata GetOrCreateMetadata(String typeName)
		{
			if (_metadata == null)
				_metadata = new Dictionary<String, IDataMetadata>();
			if (_metadata.TryGetValue(typeName, out IDataMetadata elemMeta))
				return elemMeta as ElementMetadata;
			var newMeta = new ElementMetadata();
			_metadata.Add(typeName, newMeta);
			return newMeta;
		}

		GroupMetadata GetOrCreateGroupMetadata(String typeName)
		{
			if (_groupMetadata == null)
				_groupMetadata = new Dictionary<String, GroupMetadata>();
			if (_groupMetadata.TryGetValue(typeName, out GroupMetadata groupMeta))
				return groupMeta;
			groupMeta = new GroupMetadata();
			_groupMetadata.Add(typeName, groupMeta);
			return groupMeta;
		}

		void AddValueToRecord(IDictionary<String, Object> record, FieldInfo field, Object value)
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
				if (value is String)
					innerObj.Add(propNames[1], _localizer.Localize(value?.ToString()));
				else
					innerObj.Add(propNames[1], value);
			}
			else if (field.IsRefId)
			{
				var refValue = new ExpandoObject();
				_refMap.Add(field.TypeName, value, refValue);
				record.Add(field.PropertyName, refValue);
			}
			else if (value is String)
				record.Add(field.PropertyName, _localizer.Localize(value?.ToString()));
			else if (field.IsUtc && value is DateTime dt)
				record.Add(field.PropertyName, DateTime.SpecifyKind(dt.ToLocalTime(), DateTimeKind.Unspecified));
			else
				record.Add(field.PropertyName, value);
		}

		void AddRecordToGroup(ExpandoObject currentRecord, FieldInfo field, List<Boolean> groupKeys)
		{
			if (groupKeys == null)
				throw new DataLoaderException($"There is no groups property for '{field.TypeName}");
			ElementMetadata elemMeta = GetOrCreateMetadata(field.TypeName);
			if (String.IsNullOrEmpty(elemMeta.Items))
				throw new DataLoaderException($"There is no 'Items' property for '{field.TypeName}");
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

		void AddRecordToCross(String propName, Object id, ExpandoObject currentRecord, Object keyProp, FieldInfo rootFI)
		{
			if (keyProp == null)
				throw new DataLoaderException("Key not found in cross object");
			var pxa = propName.Split('.'); // <Type>.PropName
			/*0-key, 1-Property (optional)*/
			var key = Tuple.Create(pxa[0], id);
			if (!_idMap.TryGetValue(key, out ExpandoObject mapObj))
				throw new DataLoaderException($"Property '{propName}'. Object {pxa[0]} (Id={id}) not found");
			mapObj.AddToCross(pxa[1], currentRecord, keyProp?.ToString());
			_crossMap.Add(propName, pxa[1], mapObj, keyProp.ToString(), rootFI);
		}


		void AddRecordToArray(String propName, Object id, ExpandoObject currentRecord, String rootTypeName = null)
		{
			var pxa = propName.Split('.'); // <Type>.PropName
			/*0-key, 1-Property (optional)*/
			var key = Tuple.Create(pxa[0], id);
			if (!_idMap.TryGetValue(key, out ExpandoObject mapObj))
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
				String fieldName = elemData.FindPropertyByType(rootTypeName);
				if (String.IsNullOrEmpty(fieldName))
					throw new DataLoaderException($"AddRecordToArray. Field for type '{rootTypeName}' not found in '{propName}' object");
				mapObj.AddToArray(fieldName, currentRecord);
			}
			else if (pxa.Length != 2)
				throw new DataLoaderException($"Invalid field name '{propName}' for array. 'TypeName.PropertyName' expected");
			else
				mapObj.AddToArray(pxa[1], currentRecord);
		}

		void AddMapToRecord(String propName, Object id, Object key, ExpandoObject currentRecord)
		{
			if (key == null)
				throw new DataLoaderException($"There is no 'Key' property for field '{propName}'");
			var pxa = propName.Split('.'); // <Type>.PropName
			if (pxa.Length != 2)
				throw new DataLoaderException($"Invalid field name '{propName}' for array. 'TypeName.PropertyName' expected");
			/*0-key, 1-Property*/
			var srcKey = Tuple.Create(pxa[0], id);
			if (!_idMap.TryGetValue(srcKey, out ExpandoObject mapObj))
				throw new DataLoaderException($"Property '{propName}'. Object {pxa[0]} (Id={id}) not found");
			var innerObject = mapObj.Get<ExpandoObject>(pxa[1]);
			if (innerObject == null)
			{
				innerObject = new ExpandoObject();
				mapObj.Set(pxa[1], innerObject);
			}
			innerObject.Set(key.ToString(), currentRecord);
		}

		void AddObjectToRecord(String propName, Object id, ExpandoObject currentRecord)
		{
			var pxa = propName.Split('.'); // <Type>.PropName
			if (pxa.Length != 2)
				throw new DataLoaderException($"Invalid field name '{propName}' for array. 'TypeName.PropertyName' expected");
			/*0-key, 1-Property*/
			var key = Tuple.Create(pxa[0], id);
			if (!_idMap.TryGetValue(key, out ExpandoObject mapObj))
				throw new DataLoaderException($"Property '{propName}'. Object {pxa[0]} (Id={id}) not found");
			mapObj.Set(pxa[1], currentRecord);
		}

		void AddRecordToModel(ExpandoObject currentRecord, FieldInfo field, Object id, Object key)
		{
			if (field.IsArray)
			{
				_refMap.MergeObject(field.TypeName, id, currentRecord);
				_root.AddToArray(field.PropertyName, currentRecord);
			}
			else if (field.IsTree)
				_root.AddToArray(field.PropertyName, currentRecord);
			else if (field.IsObject)
				_root.Add(field.PropertyName, currentRecord);
			else if (field.IsMap)
			{
				_refMap.MergeObject(field.TypeName, id, currentRecord);
				if (field.IsVisible)
				{
					if (key != null)
						_root.AddToMap(field.PropertyName, currentRecord, key.ToString());
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

		void CheckRecordRef(ExpandoObject currentRecord, FieldInfo field, Object id)
		{
			if (field.IsArray || field.IsMap)
				_refMap.MergeObject(field.TypeName, id, currentRecord);
		}

		void ProcessComplexMetadata(FieldInfo fieldInfo, ElementMetadata elem, DataType dt, Int32 fieldLen)
		{
			// create metadata for nested type
			var innerElem = GetOrCreateMetadata(fieldInfo.TypeName);
			var fna = fieldInfo.PropertyName.Split('.');
			if (fna.Length != 2)
				throw new DataLoaderException($"Invalid complex name {fieldInfo.PropertyName}");
			elem.AddField(new FieldInfo($"{fna[0]}!{fieldInfo.TypeName}"), DataType.Undefined);
			innerElem.AddField(new FieldInfo(fieldInfo, fna[1]), dt, fieldLen);
		}

		void ProcessMapObjectMetadata(FieldInfo fieldInfo, ElementMetadata elem)
		{
			var mapType = fieldInfo.TypeName + "Map";
			var innerElem = GetOrCreateMetadata(mapType);
			innerElem.MapItemType = fieldInfo.TypeName;
			foreach (var f in fieldInfo.MapFields)
				innerElem.AddField(new FieldInfo($"{f}!{fieldInfo.TypeName}"), DataType.Undefined);
			elem.AddField(new FieldInfo($"{fieldInfo.PropertyName}!{mapType}"), DataType.Undefined);
		}

		public void PostProcess()
		{
			_crossMap.Transform();
			foreach (var (k, v) in _crossMap)
			{
				Int32 pos = k.IndexOf('.');
				String typeName = k.Substring(0, pos);
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
						var fi = new FieldInfo(key, v.CrossType);
						crossMeta.AddField(fi, DataType.String);
					}
					typeMeta.SetCrossObject(prop, crossObjType);
					typeMeta.AddCross(prop, crossKeys);
				}
			}
		}
	}
}
