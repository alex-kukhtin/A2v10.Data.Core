﻿// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data.Providers;

public class Record : IExternalDataRecord
{
	public List<FieldData> DataFields;
	private readonly IDictionary<String, Int32> _fieldMap;

	public Record(IDictionary<String, Int32> fields)
	{
		DataFields = [];
		_fieldMap = fields ?? throw new ArgumentNullException(nameof(fields));
	}

	public Record(List<FieldData> dat, IDictionary<String, Int32> fields)
	{
		DataFields = dat;
		_fieldMap = fields ?? throw new ArgumentNullException(nameof(fields));
	}

	public Object? FieldValue(String name)
	{
		var xname = FindFieldName(name);
		if (xname == null)
			return null;
		if (_fieldMap.TryGetValue(xname, out Int32 fieldNo))
		{
			if (fieldNo >= 0 && fieldNo < DataFields.Count)
				return DataFields[fieldNo].Value;
			return null; // may be 
		}
		throw new ExternalDataException($"Invalid field name: '{xname}'");
	}

	public Object? FieldValue(Int32 index)
	{
		if (index >= 0 && index < DataFields.Count)
			return DataFields[index].Value;
		return null;
	}

	public String? StringFieldValueByIndex(Int32 index)
	{
		if (index < DataFields.Count)
			return DataFields[index].StringValue;
		return null;
	}

	public void SetFieldValueString(Int32 index, String name, String value)
	{
		while (DataFields.Count <= index)
			DataFields.Add(new FieldData());
		if (!_fieldMap.ContainsKey(name))
			_fieldMap.Add(name, index);
		DataFields[index].StringValue = value;
	}

	public Boolean FieldExists(String name)
	{
		return _fieldMap.ContainsKey(name);
	}

	public String? FindFieldName(String name)
	{
		if (!name.Contains('|'))
			return name;
		String retName = name;
		foreach (var f in name.Split('|'))
		{
			if (String.IsNullOrWhiteSpace(f))
				return null;
			if (_fieldMap.ContainsKey(f))
			{
				retName = f;
				// try to get all variants
				if (!IsFieldEmptyInternal(f))
					return f;
			}
		}
		return retName;
	}

	public Boolean IsFieldEmptyInternal(String name)
	{
		if (_fieldMap.TryGetValue(name, out Int32 fieldNo))
		{
			if (fieldNo >= 0 && fieldNo < DataFields.Count)
				return DataFields[fieldNo].IsEmpty;
		}
		return true;
	}

	public Boolean IsFieldEmpty(String name)
	{
		var xname = FindFieldName(name);
		if (xname == null)
			return true;
		if (_fieldMap.TryGetValue(xname, out Int32 fieldNo))
		{
			if (fieldNo >= 0 && fieldNo < DataFields.Count)
				return DataFields[fieldNo].IsEmpty;
		}
		throw new ExternalDataException($"Invalid field name: {xname}");
	}

	public Boolean IsEmpty
	{
		get
		{
			foreach (var d in DataFields)
				if (!d.IsEmpty)
					return false;
			return true;
		}
	}
}

