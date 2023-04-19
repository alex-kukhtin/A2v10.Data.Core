// Copyright © 2012-2023 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data;

internal class TypedIdMapItem
{
    private Dictionary<Object, Object> _map = new();
    public void Add(Object id, Object data)
    {
        if (_map.ContainsKey(id))
            throw new InvalidOperationException($"Element with id = {id} is already used");
        _map.Add(id, data);
    }
    public Object Get(Object id) 
    {
        if (_map.TryGetValue(id, out Object? val))
            return val;
        throw new InvalidOperationException($"Element with Id = {id} not found");
    }
}

internal class TypedIdMap
{
    private readonly Dictionary<String, TypedIdMapItem> _map = new();

    public void Add(String typeName, Object id, Object data)
    {
        if (!_map.TryGetValue(typeName, out TypedIdMapItem? item))
        {
            item = new TypedIdMapItem();
            _map.Add(typeName, item);
        }
        item.Add(id, data);
    }

    public Object? GetElement(String typeName, Object id)
    {
        if (_map.TryGetValue(typeName, out TypedIdMapItem? item))
            return item.Get(id);
        return null;
    }
}
