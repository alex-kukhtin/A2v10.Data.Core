// Copyright © 2012-2024 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Data;

public class ElementMetadata : IDataMetadata
{
	private readonly Dictionary<String, IDataFieldMetadata> _fields = [];

	public IDictionary<String, IList<String?>?>? _cross = null;

	public String? Id { get; private set; }
	public Int32 IdIndex { get; private set; } = -1;
	public Int32 ParentIdIndex { get; private set; } = -1;	
	public String? Key { get; private set; }
	public String? Prop { get; private set; }
	public String? Name { get; private set; }
	public String? RowNumber { get; private set; }
	public String? HasChildren { get; private set; }
	public String? Permissions { get; set; }
	public String? Items { get; set; }
	public String? Expanded { get; set; }
	public String? MapItemType { get; set; }
	public String? MainObject { get; set; }
	public String? Token { get; set; }
    public String? ParentIdTargetType { get; set; }
	public String? ParentIdTargetProp { get; set; }

    public Boolean IsArrayType { get; set; }
	public Boolean IsRowCount { get; set; }
	public Boolean IsGroup { get; set; }
	public Boolean HasCross => _cross != null;

	public SortedList<String, Tuple<Int32, String>>? Groups { get; private set; }

	public IDictionary<String, IDataFieldMetadata> Fields => _fields;
	public IDictionary<String, IList<String?>?>? Cross => _cross;

	public String? FindPropertyByType(String typeName)
	{
		foreach (var f in Fields)
			if (f.Value.RefObject == typeName)
				return f.Key;
		return null;
	}

	public FieldMetadata? AddField(Int32 index, FieldInfo field, DataType type, SqlDataType sqlType, Int32 fieldLen = 0)
	{
		if (!field.IsVisible)
			return null;
		if (IsFieldExists(field.PropertyName, type, out FieldMetadata? fm))
			return fm;
		if (index == -1)
			index = _fields.Count - 1;
		fm = new FieldMetadata(index, field, type, sqlType, fieldLen);
		_fields.Add(field.PropertyName, fm);
		switch (field.SpecType)
		{
			case SpecType.Id:
				Id = field.PropertyName;
				IdIndex = index;
				break;
			case SpecType.Key:
				Key = field.PropertyName;
				break;
			case SpecType.Prop:
				Prop = field.PropertyName;
				break;
			case SpecType.Name:
				Name = field.PropertyName;
				break;
			case SpecType.RowNumber:
				RowNumber = field.PropertyName;
				break;
			case SpecType.RowCount:
				IsRowCount = true;
				break;
			case SpecType.HasChildren:
				HasChildren = field.PropertyName;
				break;
			case SpecType.Permissions:
				Permissions = field.PropertyName;
				break;
			case SpecType.Items:
				Items = field.PropertyName;
				break;
			case SpecType.Expanded:
				Expanded = field.PropertyName;
				break;
			case SpecType.Token:
				Token = field.PropertyName;
				break;
		}
		return fm;
	}

	public void SetCrossObject(String key, String typeName)
	{
		if (_fields.TryGetValue(key, out IDataFieldMetadata? iFM))
		{
			if (iFM is FieldMetadata fm)
				fm.SetType(typeName);
		}
	}

	public void AddCross(String key, IList<String?>? cross)
	{
		_cross ??= new Dictionary<String, IList<String?>?>();
		if (_cross.ContainsKey(key))
			_cross[key] = cross;
		else
			_cross.Add(key, cross);
	}

	public Int32 FieldCount { get { return _fields.Count; } }

	public Boolean ContainsField(String field)
	{
		return _fields.ContainsKey(field);
	}

	Boolean IsFieldExists(String name, DataType dataType, out FieldMetadata? fm)
	{
		fm = null;
		if (_fields.TryGetValue(name, out IDataFieldMetadata? ifm))
		{
			if (ifm is FieldMetadata fm2)
			{
				fm = fm2;
				if (fm2.DataType != dataType)
					throw new DataLoaderException($"Invalid property '{name}'. Type mismatch. ({fm2.DataType} <> {dataType})");
				return true;
			}
			else
            {
				throw new InvalidProgramException("Invalid metadata. variable is not FieldMetadata");
            }
		}
		return false;
	}

	public FieldMetadata GetField(String name)
	{
		if (_fields.TryGetValue(name, out IDataFieldMetadata? fm))
		{
			return (fm as FieldMetadata)!;
		}
		throw new DataLoaderException($"Field '{name}' not found.");
	}

	public void SetParentId(Int32 index, String key)
	{
		ParentIdIndex = index;
		var s = key.Split('.');
		if (s.Length < 2)
			throw new InvalidOperationException($"Invalid ParentId key ({key})");
		ParentIdTargetType = s[0];
		ParentIdTargetProp = s[1];
	}
}

