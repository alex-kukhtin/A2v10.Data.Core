// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using Newtonsoft.Json;


namespace A2v10.Data;

[JsonDictionary]
public class TType : Dictionary<String, String>
{
	// self: fieldName:type
	// _id, name: fieldName

	[JsonIgnore]
	public String? Name { get; private set; }

	[JsonIgnore]
	private static readonly HashSet<String> _specialPropNames =
        [
            "_id",
		"_name"
	];

	[JsonIgnore]
	private readonly Dictionary<String, String> _specialFieldNames = [];

	[JsonIgnore]
	private Boolean _parsed;

	public void Parse(String name, TypeDictionary sharedTypes)
	{
		if (_parsed)
			return;
		Name = name;
		List<String> toRemoveFields = [];
		TType? extends = null;

		foreach (var m in this)
		{
			if (_specialPropNames.Contains(m.Key))
			{
				toRemoveFields.Add(m.Key);
				if (_specialFieldNames.ContainsKey(m.Key))
					throw new DataValidationException($"Load. Special key '{m.Key}' already exists in '{Name}' type");
				_specialFieldNames.Add(m.Key, m.Value);
			}
			else if (m.Key == "_extends")
			{
				extends = sharedTypes[m.Value];
				toRemoveFields.Add(m.Key);
			}
		}
		foreach (var r in toRemoveFields)
			this.Remove(r);
		if (extends != null)
			CopyFrom(extends);
		_parsed = true;
	}

	public void CopyFrom(TType baseType)
	{
		foreach (var s in baseType._specialFieldNames)
		{
			if (_specialFieldNames.ContainsKey(s.Key))
				throw new DataValidationException($"Load. Special key '{s.Key}' already exists in '{Name}' type");
			_specialFieldNames.Add(s.Key, s.Value);
		}
		foreach (var f in baseType)
		{
			if (this.ContainsKey(f.Key))
				throw new DataValidationException($"Load. Field '{f.Key}' already exists in '{Name}' type");
			this.Add(f.Key, f.Value);
		}
	}

	private static void CheckSingleFieldType(String name, TypeDictionary types, TypeDictionary sharedTypes)
	{
		if (name.EndsWith("[]"))
			name = name[0..^2];
		if (!types.ContainsKey(name) && !sharedTypes.ContainsKey(name))
			throw new DataValidationException($"Load. Type '{name}' not found");
	}

	public void CheckFieldTypes(TypeDictionary types, TypeDictionary sharedTypes)
	{
		// fields
		foreach (var f in this)
		{
			switch (f.Value)
			{
				case "String":
				case "Number":
				case "Date":
					break;
				default:
					CheckSingleFieldType(f.Value, types, sharedTypes);
					break;
			}
		}
	}
	public void ValidateType(IDataMetadata typeMetadata)
	{
		foreach (var f in typeMetadata.Fields)
		{
			if (!this.ContainsKey(f.Key))
				throw new DataValidationException($"Validate. Field {f.Key} not found in type {Name}");
			IDataFieldMetadata fm = f.Value;
			if (fm.TypeForValidate != this[f.Key])
				throw new DataValidationException($"Validate. Type mismatch for '{Name}.{f.Key}'. Database value:'{fm.TypeForValidate}', Validator value:'{this[f.Key]}'");
		}
	}

}

[JsonDictionary]
public class TypeDictionary : Dictionary<String, TType>, IDataModelValidator
{
	[JsonIgnore]
	public String? Name { get; private set; }

	public void Parse(String name, TypeDictionary sharedTypes)
	{
		Name = name;
		foreach (var t in this)
		{
			t.Value.Parse(t.Key, sharedTypes);
		}
		CheckFieldTypes(sharedTypes);
	}

	void CheckFieldTypes(TypeDictionary sharedTypes)
	{
		foreach (var t in this)
		{
			t.Value.CheckFieldTypes(this, sharedTypes);
		}
	}

	#region IDataModelValidator
	public void ValidateType(String name, IDataMetadata typeMetadata)
	{
		if (!this.ContainsKey(name))
		{
			throw new DataValidationException($"Validate. Type '{name}' not found");
		}
		this[name].ValidateType(typeMetadata);
	}
	#endregion
}

[JsonDictionary]
public class AllModels : Dictionary<String, TypeDictionary>
{
	TypeDictionary? _shared;

	public void Parse()
	{
		Boolean bShared = false;
		foreach (var m in this)
		{
			if (m.Key == "_shared")
			{
				_shared = m.Value;
				bShared = true;
			}
		}
		if (_shared != null)
		{
			foreach (var m in this)
			{
				m.Value.Parse(m.Key, _shared);
			}
		}
		if (bShared)
			this.Remove("_shared");
	}
}
