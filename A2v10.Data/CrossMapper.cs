// Copyright © 2019-2024 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data;

internal class CrossItem(String targetProp, Boolean isArray, String crossType, String keyName)
{
	readonly Dictionary<Object, ExpandoObject> _items = [];
	readonly Dictionary<String, Int32> _keys = [];

    public String TargetProp { get; } = targetProp;
    public Boolean IsArray { get; } = isArray;
    public String CrossType { get; } = crossType;

    public String KeyName { get; } = keyName;

    public IEnumerable<String> Keys => _keys.Keys;

    public void Add(String propName, ExpandoObject target, Object id)
	{
		_items.TryAdd(id, target);
		_keys.TryAdd(propName, _keys.Count);
	}

	public List<String?> GetCross()
	{
		var l = new List<String?>();
		for (Int32 i = 0; i < _keys.Count; i++)
			l.Add(null);
		foreach (var x in _keys)
			l[x.Value] = x.Key;
		return l;
	}

	public void Transform()
	{
		if (!IsArray)
			return;
		Int32 _keyCount = _keys.Count;
		foreach (var (_, eo) in _items)
		{
			var arr = CreateArray(_keyCount);
			ExpandoObject? targetVal = eo.Get<ExpandoObject>(TargetProp);
			if (targetVal == null)
				continue; // already array?
			foreach (var (key, index) in _keys)
			{
				var val = targetVal.Get<ExpandoObject>(key);
				val ??= new ExpandoObject() { 
					{ KeyName, key}
				};
				arr[index] = val;
			}
			eo.Set(TargetProp, arr);
		}
	}

	static List<ExpandoObject?> CreateArray(Int32 cnt)
	{
		var l = new List<ExpandoObject?>();
		for (Int32 i = 0; i < cnt; i++)
			l.Add(null);
		return l;
	}

	static void SetIntoArray(List<ExpandoObject?> array, ExpandoObject obj, int ix)
	{
		while (array.Count <= ix)
			array.Add(null);
		array[ix] = obj;
	}

	public List<ExpandoObject?> GetEmptyArray()
	{
		var arr = new List<ExpandoObject?>();
		foreach (var key in _keys)
		{
			SetIntoArray(arr, new ExpandoObject() {
					{ KeyName, key.Key}}, key.Value
			);

		}
		return arr;
	}
}

internal class CrossMapper : Dictionary<String, CrossItem>
{
	private readonly Dictionary<Tuple<String, String>, List<ExpandoObject?>> _parentRecords = [];
	public void AddParentRecord(String typeName, String propName, ExpandoObject record)
	{
		var key = Tuple.Create<String, String>(typeName, propName);
		if (_parentRecords.TryGetValue(key, out List<ExpandoObject?>? list))
			list.Add(record);
		else
			_parentRecords.Add(key, [record]);
	}

	public void Add(String key, String targetProp, ExpandoObject target, String propName, String keyName, FieldInfo rootFI, Object id)
	{
		if (!TryGetValue(key, out CrossItem? crossItem))
		{
			crossItem = new CrossItem(targetProp, rootFI.IsCrossArray, rootFI.TypeName, keyName);
			Add(key, crossItem);
		}
		// all source elements
		crossItem.Add(propName, target, id);
	}

	public void Transform()
	{
		foreach (var x in this)
			x.Value.Transform();
		TransformParentRecords();
	}

	void TransformParentRecords()
	{
		foreach (var kv in _parentRecords)
		{
			foreach (var row in kv.Value)
			{
				if (row == null)
					continue;
				var arr = row.Get<List<ExpandoObject?>>(kv.Key.Item2);
				if (arr == null)
				{
					var crossKey = $"{kv.Key.Item1}.{kv.Key.Item2}";
					if (this.TryGetValue(crossKey, out CrossItem? crossItem))
						row.Set(kv.Key.Item2, crossItem.GetEmptyArray());
				}
			}
		}
	}
}

