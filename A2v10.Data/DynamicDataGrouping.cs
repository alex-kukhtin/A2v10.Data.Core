// Copyright © 2022-2023 Oleksandr Kukhtin. All rights reserved.

using System.Data;

namespace A2v10.Data;

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
    private ExpandoObject _data = new();
    private readonly List<ExpandoObject> _leafs = new();
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

    public void SetData(ExpandoObject data)
    {
		_data = data;
		_leafs.Add(data);
	}

	public void CalculateLeafs<T>(String propName, Func<T?[], T> calc)
    {
        if (_leafs.Count == 0) 
            return;
		T? result;
		T?[] values = new T[_leafs.Count];
        for (int i = 0; i < _leafs.Count; i++)
            values[i] = _leafs[i].Get<T>(propName);
		result = calc(values);
		_data.Set(propName, result);
	}

	public void Calculate<T>(String propName, Func<T?[], T> calc)
    {
        if (_children.Count == 0)
            return;
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
    public List<String> Groups = new();
    public List<AggregateDescriptor> Aggregates = new();
    public void AddGroup(String prop)
    {
        Groups.Add(prop);
    }
    public void AddAggregate(String prop, AggregateType type)
    {
        Aggregates.Add(new AggregateDescriptor(prop, type));
    }
}

internal class DynamicDataGrouping
{
    private readonly ExpandoObject _root;

    private readonly IDictionary<String, IDataMetadata> _metadata;
    private readonly Dictionary<String, RecordsetDescriptor> _recordsets = new();
    private readonly DataModelReader _modelReader;
    public DynamicDataGrouping(ExpandoObject root, IDictionary<String, IDataMetadata> metadata, DataModelReader modelReader)
    {
        _root = root;
        _metadata = metadata;
        _modelReader = modelReader;
    }

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

    static void ProcessRecordset(RecordsetDescriptor descr, IDataMetadata itemMeta, GroupMetadata groupMeta,
        DynamicGroupItem dynaroot, List<ExpandoObject> items)
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
            for (var i = 0; i < descr.Groups.Count; i++)
            {
                var gr = descr.Groups[i];
                elem = dat.Eval<Object>(gr);
                group = group.GetOrCreate(elem, gr);
            }
            group?.SetData(dat);
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
	}

    public void Process()
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
            ProcessRecordset(pd.Value, itemMeta, gm, dr, list);
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
