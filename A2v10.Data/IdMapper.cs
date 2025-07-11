﻿// Copyright © 2012-2024 Oleksandr  Kukhtin. All rights reserved.



namespace A2v10.Data;

using A2v10.Data.Core.Extensions.Dynamic;
using System.Linq;

internal class IdMapper : Dictionary<Tuple<String, Object?>, ExpandoObject>
{
	public ExpandoObject Add(String typeName, Object? id, ExpandoObject value)
	{
		var key = Tuple.Create(typeName, id);
		if (!TryGetValue(key, out ExpandoObject? valObj))
		{
			Add(key, value);
			valObj = value;
		}
		return valObj;
	}

	public Type GetRefType(String typeName)
	{
		var x = this.FirstOrDefault(x => x.Key.Item1 == typeName);
		return x.Key.Item2?.GetType()
			?? throw new InvalidOperationException($"Key for {typeName} not found");
	}

	public Object? StringToType(Type type, String value)
	{
        if (type == typeof(Int64))
            return Int64.Parse(value);
        else if (type == typeof(String))
			return value;
        else if (type == typeof(Guid))
            return Guid.Parse(value);
        else if (type == typeof(Int32))
            return Int32.Parse(value);
     	else
            throw new InvalidOperationException($"Unsupported type for Id ({type.GetType})");
    }
}

internal record RefMapperItem
{
	public List<ExpandoObject>? List;
	public ExpandoObject? Source;

	public RefMapperItem()
	{
	}

	public List<ExpandoObject> AddToList(ExpandoObject eo)
	{
		List ??= [];
		List.Add(eo);
		return List;
	}
}

internal class RefMapper : Dictionary<Tuple<String, Object?>, RefMapperItem>
{
	public void Add(String typeName, Object? id, ExpandoObject value)
	{
		var key = Tuple.Create<String, Object?>(typeName, id);
		if (!TryGetValue(key, out RefMapperItem? item))
		{
			item = new RefMapperItem();
			Add(key, item);
		}
		var itemList = item.AddToList(value);
		if (item.Source != null)
		{
			foreach (var target in itemList)
				target.CopyFrom(item.Source);
		}
	}

	public void MergeObject(String typeName, Object? id, ExpandoObject? source)
	{
		var key = Tuple.Create<String, Object?>(typeName, id);
		if (TryGetValue(key, out RefMapperItem? item))
		{
			if (item != null)
			{
				if ((item.Source == null) && (source != null))
					item.Source = source;
				if (item.List != null)
					foreach (var target in item.List)
						target.CopyFrom(source);
			}
		}
		else
		{
			// forward definition
			item = new RefMapperItem
			{
				Source = source
			};
			Add(key, item);
		}
	}
	public void Correct(Tuple<String, Object?> key)
	{
		if (!TryGetValue(key, out RefMapperItem? item))
			return;
		if (item == null)
			return;
		if (item.List == null)
			return;
		foreach (var target in item.List)
			target.CopyFromUnconditional(item.Source);
	}
}

