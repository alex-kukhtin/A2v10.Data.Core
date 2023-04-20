// Copyright © 2012-2023 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data;

internal record TypedRefMapPair(TargetFieldMetadata Field, Object Target);

internal class TypedRefMapItem
{
    private readonly Dictionary<Object, List<TypedRefMapPair>> _map = new();
    public void Add(Object id, TypedRefMapPair pair)
    {
        if (!_map.TryGetValue(id, out var list))
        {
            list = new List<TypedRefMapPair>();
            _map.Add(id, list);
        }
        list.Add(pair);
    }
    public Object? CreateObject(Object obj)
    {
        if (!_map.TryGetValue(obj, out var list))
            return null;
        if (list.Count == 0)
            return null;
        var newelem = list[0].Field.CreateObject();
        foreach (var li in list)
            li.Field.SetValue(li.Target, newelem);
        return newelem;
    }
}

internal class TypedRefMap
{
    private readonly Dictionary<String, TypedRefMapItem> _map = new();
    public void Add(String typeName, Object value, TypedRefMapPair mapPair)
    {
        if (!_map.TryGetValue(typeName, out var mapItem))
        {
            mapItem = new TypedRefMapItem();
            _map.Add(typeName, mapItem);
        }
        mapItem.Add(value, mapPair);
    }
    public TypedRefMapItem? Get(String typeName)
    {
        if (_map.TryGetValue(typeName, out var item))
            return item;
        return null;
    }
}
