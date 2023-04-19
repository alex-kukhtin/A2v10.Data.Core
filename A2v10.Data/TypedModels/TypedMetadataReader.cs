// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System.Data;

namespace A2v10.Data;

internal class TypedMetadataReader
{
    private const String ROOT = "TRoot";
    private readonly IDictionary<String, IDataMetadata> _metadata = new Dictionary<String, IDataMetadata>();
    private FieldInfo? _mainElement;
    public void ProcessOneMetadata(IDataReader rdr)
    {
        if (rdr.FieldCount == 0)
            return;
        // first field = self object
        var schemaTable = rdr.GetSchemaTable()
            ?? throw new DataLoaderException($"Invalid schema table");
        var firstFieldName = rdr.GetName(0);
        var objectDef = new FieldInfo(firstFieldName);
        objectDef.CheckTypeName(); // for first field only
        if (objectDef.FieldType == FieldType.Scalar)
        {
            throw new DataLoaderException($"Invalid element type: '{firstFieldName}'");
        }
        var rootMetadata = GetOrCreateMetadata(ROOT);
        rootMetadata.AddField(0, objectDef, DataType.Undefined, SqlDataType.Unknown);
        // other fields = object fields
        var typeMetadata = GetOrCreateMetadata(objectDef.TypeName);
        if (objectDef.IsMain && _mainElement == null)
        {
            _mainElement = objectDef;
            rootMetadata.MainObject = objectDef.PropertyName;
        }
        if (objectDef.IsArray || objectDef.IsTree || objectDef.IsMap || objectDef.IsLookup)
            typeMetadata.IsArrayType = true;
        if (objectDef.IsGroup)
            typeMetadata.IsGroup = true;
        //if ((objectDef.IsArray || objectDef.IsTree) && objectDef.IsVisible)
            //_root.AddToArray(objectDef.PropertyName, null); // empty record
        Boolean hasRowCount = false;
        for (Int32 i = 1; i < rdr.FieldCount; i++)
        {
            String fieldName = rdr.GetName(i);
            var fieldDef = new FieldInfo(fieldName);
            if (fieldDef.IsRowCount)
                hasRowCount = true;
            if (fieldDef.IsPermissions)
            {
                fieldDef.CheckPermissionsName();
                typeMetadata.AddField(i, fieldDef, DataType.Number, 0);
                continue;
            }
            if (fieldDef.IsCrossArray)
                typeMetadata.AddCross(fieldDef.PropertyName, null);
            if (fieldDef.IsParentId)
                typeMetadata.SetParentId(i, fieldDef.TypeName);

            if (!fieldDef.IsVisible)
                continue;
            DataType dt = rdr.GetFieldType(i).Name.TypeName2DataType();
            SqlDataType sqlDataType = rdr.GetDataTypeName(i).SqlTypeName2SqlDataType();

            Int32 fieldLength = 0;
            if (dt == DataType.String)
                fieldLength = (Int32)schemaTable.Rows[i]["ColumnSize"];

            if (fieldDef.IsComplexField)
                ProcessComplexMetadata(fieldDef, typeMetadata, dt, sqlDataType, fieldLength);
            else if (fieldDef.IsMapObject)
                ProcessMapObjectMetadata(fieldDef, typeMetadata);
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
            AddRowCount(objectDef.PropertyName);

    }

    void AddRowCount(String propertyName)
    {
    }
    
    public ElementMetadata GetMetadata(String typeName)
    {
        if (_metadata.TryGetValue(typeName, out IDataMetadata? elemMeta))
        {
            if (elemMeta is ElementMetadata realElem)
                return realElem;
            throw new DataLoaderException($"Invalid element metadata ({typeName})");
        }
        throw new DataLoaderException($"Metadata not found ({typeName})");
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
}
