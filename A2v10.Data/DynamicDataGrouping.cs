// Copyright © 2022-2024 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using System.Linq;

namespace A2v10.Data;

using A2v10.Data.Core.Extensions.Dynamic;

internal class KeyComparer : IEqualityComparer<Object>
{
	private const String Id = "Id";
	public new Boolean Equals(Object? x, object? y)
	{
		if (x == null && y == null)
			return true;
		if (x == null || y == null) return false;
		if (x is ExpandoObject eox && y is ExpandoObject eoy)
			return eox.Get<Object>(Id) == eoy.Get<Object>(Id);
		return x.Equals(y);
	}

	public int GetHashCode(Object obj)
	{
		if (obj == null)
			return 0;
		if (obj is ExpandoObject eo)
			return eo.Get<Object>(Id)?.GetHashCode() ?? 0;
		return obj.GetHashCode();
	}
}

internal class DynamicGroupItem
{
	private readonly Dictionary<Object, DynamicGroupItem> _children = new(new KeyComparer());
	private ExpandoObject _data = [];
	private readonly List<ExpandoObject> _leafs = [];
	public DynamicGroupItem(Object? key = null, String? elem = null)
	{
		if (elem == null) return;
		_data.Set(elem, key);
	}

	public ExpandoObject? ToExpando(String? propertyName)
	{
		var e = _data;
		if (propertyName == null)
			return e;
		var coll = new List<ExpandoObject?>();
		foreach (var c in _children.Values)
			coll.Add(c.ToExpando(propertyName));
		e.Set(propertyName, coll);
		return e;
	}
	public DynamicGroupItem GetOrCreate(Object? key, String elem)
	{
		if (_children.TryGetValue(key ?? 0, out var item))
			return item;
		var newElem = new DynamicGroupItem(key, elem);
		_children.Add(key ?? 0, newElem);
		return newElem;
	}

	public void SetData(ExpandoObject data, String? groupProp)
	{
		_data = [];
		if (groupProp != null)
			_data.Set(groupProp, data.Get<Object>(groupProp));	
		_leafs.Add(data);
	}
	public void CalculateLeafs<T>(String propName, Func<T?[], T> calc)
	{
		if (_leafs.Count == 0)
			return;
		if (_children.Count > 0)
			return;
		T? result;
		T?[] values = new T[_leafs.Count];
		for (int i = 0; i < _leafs.Count; i++)
			values[i] = _leafs[i].Get<T>(propName);
		result = calc(values);
		_data.Set(propName, result);
	}

	public void CalculateCrossPhase2(CrossMapper crossMap)
	{
		if (crossMap.Count == 0)
			return;
		if (_children.Count == 0)
			return;
		foreach (var (_, ch) in _children)
			ch.CalculateCrossPhase2(crossMap);
		foreach (var (_, crossItem) in crossMap)
		{
			var targetCross = _data.Get<List<ExpandoObject>>(crossItem.TargetProp);
			if (targetCross == null)
				continue;
			var chList = new List<ExpandoObject>();
			foreach (var (_, ch) in _children)
				chList.Add(ch._data);
			CalcAggregates(crossItem, targetCross, chList);
		}
	}

	public void CalculateCross(CrossMapper crossMap, IDictionary<String, IDataMetadata> metadata)
	{
		if (crossMap.Count == 0)
			return;
		foreach (var (_, ch) in _children)
			ch.CalculateCross(crossMap, metadata);
		foreach (var (_, crossItem) in crossMap)
		{
			var md = metadata[crossItem.CrossType] ??
				throw new DataDynamicException("Invalid Cross metadata");
			var targetCross = _data.Get<List<ExpandoObject>>(crossItem.TargetProp);
			if (targetCross == null)
			{
				targetCross = [];
				foreach (var key in crossItem.Keys)
				{
					var elem = new ExpandoObject() { { crossItem.KeyName, key } };
					foreach (var f in md.Fields.Where(f => f.Key != crossItem.KeyName))
						elem.Add(f.Key, InternalHelpers.SqlDataTypeDefault(f.Value.SqlDataType));
					targetCross.Add(elem);
				}
				_data.Set(crossItem.TargetProp, targetCross);
			}
			if (_children.Count == 0)
				CalcAggregates(crossItem, targetCross, _leafs);
		}
	}

	public void CalcCrossLeafs(CrossMapper crossMap)
	{
		if (crossMap.Count == 0)
			return;
		foreach (var (_, ch) in _children)
			ch.CalcCrossLeafs(crossMap);
	}

	static void CalcAggregates(CrossItem crossItem, List<ExpandoObject> targetCross, List<ExpandoObject> leafs)
	{
		foreach (var crossKey in crossItem.Keys)
		{
			var targetDict = GetTypedResult(targetCross, crossKey, crossItem.KeyName);
			if (targetDict == null)
				continue;
			var targetItem = targetCross.FirstOrDefault(x => x.Get<String>(crossItem.KeyName) == crossKey);
			foreach (var leaf in leafs)
			{
				var leafDict = (leaf as IDictionary<String, Object?>)!;
				var crossArray = (leafDict[crossItem.TargetProp] as List<ExpandoObject>)!;
				var elem = crossArray.FirstOrDefault(x => x.Get<String>(crossItem.KeyName) == crossKey);
				if (elem == null)
					continue;
				var elemDict = (elem as IDictionary<String, Object?>);
				foreach (var xk in elemDict.Keys)
				{
					if (xk == crossItem.KeyName)
						continue;
					var crossValue = elemDict[xk];
					targetDict[xk] = AddTyped(targetDict[xk], crossValue);
				}
			}
			foreach (var xk in targetDict.Keys)
				targetItem?.Set(xk, targetDict[xk]);
		}
	}
	static IDictionary<String, Object?>? GetTypedResult(List<ExpandoObject> target, String key, String keyName)
	{
		var trgObject = target.FirstOrDefault(itm => itm.Get<String>(keyName) == key);
		if (trgObject == null)
			return null;
		var trgDict = (trgObject as IDictionary<String, Object?>)!;
		var eo = new ExpandoObject();
		IDictionary<String, Object?> resultDict = eo;
		foreach (var trgKey in trgDict.Keys)
		{
			if (trgKey == keyName)
				continue;
			resultDict[trgKey] = trgDict[trgKey] switch
			{
				Double => (Double)0,
				Decimal => (Decimal)0,
				Int32 => (Int32)0,
				Int64 => (Int64)0,
				_ => null
			};
		}
		return resultDict;
	}
	private static Object? AddTyped(Object? v1, Object? v2)
	{
		if (v1 == null || v2 == null)
			return null;
		if (v1 is Double dblVal1 && v2 is Double dblVal2)
			return dblVal1 + dblVal2;
		else if (v1 is Decimal decVal1 && v2 is Decimal decVal2)
			return decVal1 + decVal2;
		else if (v1 is Int32 intVal1 && v2 is Int32 intVal2)
			return intVal1 + intVal2;
		return null;
	}

	public void Calculate<T>(String propName, Func<T?[], T> calc)
	{
		if (_children.Count == 0)
		{
			CalculateLeafs<T>(propName, calc);
			return;
		}
		T? result = default;
		T?[] values = new T[_children.Count];
		var i = 0;
		foreach (var item in _children.Values)
		{
			if (item?._children.Count > 0)
				item?.Calculate<T>(propName, calc);
			else
				item?.CalculateLeafs<T>(propName, calc);
			if (item != null)
				values[i] = item._data.Get<T>(propName);
			++i;
		}
		result = calc(values);
		_data.Set(propName, result);
	}
}

internal enum AggregateType
{
	None,
	Sum,
	Avg,
	Count,
	First,
	Last
}

record AggregateDescriptor(String Property, AggregateType Type);

internal class RecordsetDescriptor
{
	public List<String> Groups = [];
	public List<AggregateDescriptor> Aggregates = [];
	public void AddGroup(String prop)
	{
		Groups.Add(prop);
	}
	public void AddAggregate(String prop, AggregateType type)
	{
		Aggregates.Add(new AggregateDescriptor(prop, type));
	}
}

internal class DynamicDataGrouping(ExpandoObject root, IDictionary<String, IDataMetadata> metadata, DataModelReader modelReader)
{
	private readonly ExpandoObject _root = root;

	private readonly IDictionary<String, IDataMetadata> _metadata = metadata;
	private readonly Dictionary<String, RecordsetDescriptor> _recordsets = [];
	private readonly DataModelReader _modelReader = modelReader;

    private RecordsetDescriptor GetOrCreateRSDescriptor(String name)
	{
		if (_recordsets.TryGetValue(name, out var descr))
			return descr;
		var d = new RecordsetDescriptor();
		_recordsets.Add(name, d);
		return d;
	}

	public void AddGrouping(IDataReader rdr)
	{
		var itemName = rdr.GetName(0);
		var fi = new FieldInfo(itemName);
		var rsDescr = GetOrCreateRSDescriptor(fi.PropertyName);
		String? funcName = null;
		String? propName = null;
		for (var i = 1; i < rdr.FieldCount; i++)
		{
			var fn = rdr.GetName(i);
			switch (fn)
			{
				case "Property":
					propName = rdr.GetString(i);
					break;
				case "Func":
					funcName = rdr.GetString(i);
					break;
				default:
					throw new InvalidOperationException($"Invalid Grouping function: '{fn}'");
			}
		}
		if (propName == null)
			return;
		switch (funcName)
		{
			case "Group":
				rsDescr.AddGroup(propName);
				break;
			case "Sum":
				rsDescr.AddAggregate(propName, AggregateType.Sum);
				break;
			case "Avg":
				rsDescr.AddAggregate(propName, AggregateType.Avg);
				break;
			case "Count":
				rsDescr.AddAggregate(propName, AggregateType.Count);
				break;
			case "First":
				rsDescr.AddAggregate(propName, AggregateType.First);
				break;
			case "Last":
				rsDescr.AddAggregate(propName, AggregateType.Last);
				break;
			case "None":
				break;
			case "Cross":
				break;
			default:
				throw new InvalidOperationException($"Invalid Function for grouping: {funcName}");
		}
	}

	void ProcessRecordset(RecordsetDescriptor descr, IDataMetadata itemMeta, GroupMetadata groupMeta,
		DynamicGroupItem dynaroot, List<ExpandoObject> items, CrossMapper crossMapper)
	{
		for (var i = 0; i < descr.Groups.Count; i++)
		{
			var gr = descr.Groups[i];
			groupMeta.AddMarkerMetadata(gr);
		}
		foreach (var dat in items)
		{
			Object? elem = null;
			DynamicGroupItem group = dynaroot;
			String? groupProp = null;
			for (var i = 0; i < descr.Groups.Count; i++)
			{
				var gr = descr.Groups[i];
				elem = dat.Eval<Object>(gr);
				group = group.GetOrCreate(elem, gr);
				groupProp = gr;
			}
			group?.SetData(dat, groupProp);
		}
		foreach (var v in descr.Aggregates)
		{
			if (!itemMeta.Fields.TryGetValue(v.Property, out var dataMeta))
				throw new InvalidOperationException($"Field Metadata {v.Property} not found");
			switch (v.Type)
			{
				case AggregateType.Sum:
					switch (dataMeta.SqlDataType)
					{
						case SqlDataType.Float:
							dynaroot.Calculate<Double>(v.Property, (values) =>
								Sum((a, b) => a + b, values));
							break;
						case SqlDataType.Currency:
							dynaroot.Calculate<Decimal>(v.Property, (values) =>
								Sum((a, b) => a + b, values));
							break;
						default:
							throw new InvalidOperationException($"Sum for {dataMeta.SqlDataType} not yet implemented");
					}
					break;
				case AggregateType.Avg:
					switch (dataMeta.SqlDataType)
					{
						case SqlDataType.Float:
							dynaroot.Calculate<Double>(v.Property, (values) =>
								Average((a, b) => a + b, (a, b) => a / b, values));
							break;
						case SqlDataType.Currency:
							dynaroot.Calculate<Decimal>(v.Property, (values) =>
								Average((a, b) => a + b, (a, b) => a / b, values));
							break;
						default:
							throw new InvalidOperationException($"Avg for {dataMeta.SqlDataType} not yet implemented");
					}
					break;
				case AggregateType.Count:
					dynaroot.Calculate<Int32>(v.Property, (values) =>
						Count(values));
					break;
			}
		}

		if (crossMapper.Count > 0)
		{
			dynaroot.CalculateCross(crossMapper, _metadata);
			dynaroot.CalculateCrossPhase2(crossMapper);
		}
	}

	public void Process(CrossMapper crossMapper)
	{
		var rootMd = _metadata["TRoot"];
		foreach (var pd in _recordsets)
		{
			if (!rootMd.Fields.TryGetValue(pd.Key, out var fieldMeta))
				throw new InvalidOperationException($"Metadata {pd.Key} not found");
			if (!_metadata.TryGetValue(fieldMeta.RefObject, out var itemMeta))
				throw new InvalidOperationException($"Metadata {fieldMeta.RefObject} not found");
			var gm = _modelReader.GetOrCreateGroupMetadata(fieldMeta.RefObject);
			var list = _root.Get<List<ExpandoObject>>(pd.Key);
			if (list == null)
				continue;
			var dr = new DynamicGroupItem();
			ProcessRecordset(pd.Value, itemMeta, gm, dr, list, crossMapper);
			var result = dr.ToExpando(itemMeta.Items);
			_root.Set(pd.Key, result);
			itemMeta.IsGroup = true;
			fieldMeta.ToDynamicGroup();
		}
	}

	static T Sum<T>(Func<T, T, T> add, T[] values) where T : struct
	{
		T result = default;
		for (var i = 0; i < values.Length; i++)
			result = add(result, values[i]);
		return result;
	}

	static T Average<T>(Func<T, T, T> add, Func<T, Int32, T> div, T[] values) where T : struct
	{
		T result = default;
		for (var i = 0; i < values.Length; i++)
			result = add(result, values[i]);
		return div(result, values.Length);
	}
	static Int32 Count<T>(T[] values) where T : struct
	{
		return values.Length;
	}
}
