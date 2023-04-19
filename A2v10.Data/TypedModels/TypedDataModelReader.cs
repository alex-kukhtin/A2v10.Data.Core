// Copyright © 2022-2023 Oleksandr Kukhtin. All rights reserved.

using System.Data;

namespace A2v10.Data.Core;

internal class TypedDataModelReader<T> where T : new()
{
	private readonly IDataLocalizer _localizer;
	private readonly ITokenProvider? _tokenProvider;
	private readonly T _root;
	private readonly TypedMetadataReader _metadataReader = new();
	public TypedDataModelReader(IDataLocalizer localizer, ITokenProvider? tokenProvider) 
	{
		_localizer = localizer;
		_tokenProvider = tokenProvider;
		_root = new T();
		// CreateTargetMetadata();
	}

	public void ProcessOneRecord(IDataReader rdr)
	{
		var rsDef = new FieldInfo(rdr.GetName(0));
		if (rsDef.IsObjectLike)
			ProcessObject(rsDef, rdr);
		else if (rsDef.IsMap)
			ProcessMap(rsDef, rdr);
		else if (rsDef.IsArray)
			ProcessArray(rsDef,	rdr);
	}
    void ProcessObject(FieldInfo fi, IDataReader rdr)
	{
        var elemMeta = _metadataReader.GetMetadata(fi.TypeName);
        var data = GetElement(_root, fi.PropertyName);
        ReadRecord(elemMeta, data, rdr);
    }

    void ProcessMap(FieldInfo fi, IDataReader rdr)
	{

	}
    void ProcessArray(FieldInfo fi, IDataReader rdr)
    {

    }

    public Object? GetElement(Object data, String name)
	{
		var pi = data.GetType().GetProperty(name);
		return pi?.GetValue(data);
	}

	public void ReadRecord(ElementMetadata elementMetadata, Object? element, IDataReader rdr)
	{
		if (element == null)
			return;
		var tp = element.GetType();
		foreach (var (name, field) in elementMetadata.Fields)
		{
			if (rdr.IsDBNull(field.FieldIndex))
				continue;
			if (field.IsRefId)
				continue; // TODO process RefId
			// RefObject => add to cache
			// Array => add to cache
			var pi = tp.GetProperty(name);
			var val = rdr.GetValue(field.FieldIndex);
			pi?.SetValue(element, val, null);
		}
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
