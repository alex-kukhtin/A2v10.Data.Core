// Copyright © 2022-2023 Oleksandr Kukhtin. All rights reserved.

using System.Data;

namespace A2v10.Data;

internal class TypedDataModelReader<T> where T : new()
{
	private readonly IDataLocalizer _localizer;
	private readonly ITokenProvider? _tokenProvider;
	private readonly T _root;
	private readonly TypedMetadataReader _metadataReader = new();
	private readonly TargetTypeMetadata _targetMetadata = new();
	private readonly TypedRefMap _refMap = new();
	private readonly TypedIdMap _idMap = new();
    public TypedDataModelReader(IDataLocalizer localizer, ITokenProvider? tokenProvider) 
	{
		_localizer = localizer;
		_tokenProvider = tokenProvider;
		_root = new T();
		if (_root == null)
			throw new InvalidOperationException("Root is null");
		_targetMetadata.Build(typeof(T));
	}

	public void ProcessOneRecord(IDataReader rdr)
	{
		var rsDef = new FieldInfo(rdr.GetName(0));
		if (rsDef.IsObject)
			ProcessObject(rsDef, rdr);
		else if (rsDef.IsMap)
			ProcessMap(rsDef, rdr);
		else if (rsDef.IsArray)
			ProcessArray(rsDef,	rdr);
	}

    void AddToObjectMap(String typeName, Object? id, Object? data)
    {
        if (id == null || data == null)
            return;
		_idMap.Add(typeName, id, data);
    }

    void ProcessObject(FieldInfo fi, IDataReader rdr)
	{
        var elemMeta = _metadataReader.GetMetadata(fi.TypeName);
        var data = GetElement(_root!, fi.PropertyName);
        var elemId = ReadRecord(elemMeta, data, rdr);
		AddToObjectMap(fi.TypeName, elemId, data);
    }

    void ProcessMap(FieldInfo fi, IDataReader rdr)
	{
		var refItem = _refMap.Get(fi.TypeName);
		if (refItem == null)
			return;
        var elemMeta = _metadataReader.GetMetadata(fi.TypeName);
		if (elemMeta == null) 
			return;
		if (rdr.IsDBNull(elemMeta.IdIndex))
			return;
		var id = rdr.GetValue(elemMeta.IdIndex);
		var newelem = refItem.CreateObject(id);
		if (newelem == null)
			return;
		var idElem = ReadRecord(elemMeta, newelem, rdr);
		AddToObjectMap(fi.TypeName, idElem, newelem);
	}
    void ProcessArray(FieldInfo fi, IDataReader rdr)
    {
        var elemMeta = _metadataReader.GetMetadata(fi.TypeName);
        if (elemMeta == null)
            return;
		if (elemMeta.ParentIdIndex == -1)
			return;
		var parentId = rdr.GetValue(elemMeta.ParentIdIndex);
		if (elemMeta.ParentIdTargetType == null || elemMeta.ParentIdTargetProp == null)
			throw new InvalidOperationException("ParentIdTargetType is null");
		var parentElement = _idMap.GetElement(elemMeta.ParentIdTargetType, parentId);
		if (parentElement == null)
			return;
		var targetMeta = _targetMetadata.GetFieldMetadata(parentElement.GetType(), elemMeta.ParentIdTargetProp);
		if (targetMeta == null)	
			return;
		var targetObj = targetMeta.CreateObject();
		var array = targetMeta.GetValue(parentElement);
		if (array == null)
			throw new InvalidOperationException($"Array '{elemMeta.ParentIdTargetProp}' for element (id = {parentId}) is null");
		targetMeta.AddToArray(array, targetObj);
		var newElemId = ReadRecord(elemMeta, targetObj, rdr);
		AddToObjectMap(fi.TypeName, newElemId, targetObj);
    }

    public Object? GetElement(Object data, String name)
	{
		return _targetMetadata.GetOrCreateElement(data, name);
	}

	public Object? ReadRecord(ElementMetadata elementMetadata, Object? element, IDataReader rdr)
	{
		if (element == null)
			return null;
		var tp = element.GetType();
		var objmeta = _targetMetadata.GetObjectMetadata(tp);
		foreach (var (name, field) in elementMetadata.Fields)
		{
			if (rdr.IsDBNull(field.FieldIndex))
				continue;
			var fieldMeta = objmeta.GetFieldMetadata(name);
			if (fieldMeta == null)
				continue;
			if (field.IsRefId)
			{
				var mapVal = rdr.GetValue(field.FieldIndex);
                _refMap.Add(field.RefObject, mapVal, new TypedRefMapPair(fieldMeta, element));
				continue;
			}
			// Array => add to cache
			if (fieldMeta.IsString)
                fieldMeta.SetValue(element, _localizer.Localize(rdr.GetString(field.FieldIndex)));
            else
                fieldMeta.SetValue(element, rdr.GetValue(field.FieldIndex));
		}
        if (elementMetadata.IdIndex != -1)
            return rdr.GetValue(elementMetadata.IdIndex);
		return null;
	}
	public void ProcessOneMetadata(IDataReader rdr)
	{
		_metadataReader.ProcessOneMetadata(rdr);
	}

	public void PostProcess()
	{
	}

	public T DataModel => _root;
}
