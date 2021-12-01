// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System.Data;
using System.IO;
using System.Reflection;

namespace A2v10.Data;
public abstract class LoadHelperBase<T> where T : class
{
	private readonly Type _type;
	private readonly PropertyInfo[] _props;
	private readonly Dictionary<String, Int32> _keyMap;

	public LoadHelperBase()
	{
		_type = typeof(T);
		_props = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		_keyMap = new Dictionary<String, Int32>();
	}

	public void ProcessMetadata(IDataReader rdr)
	{
		for (Int32 c = 0; c < rdr.FieldCount; c++)
		{
			_keyMap.Add(rdr.GetName(c), c);
		}
	}

	public T CreateInstance(IDataReader rdr)
	{
		if (System.Activator.CreateInstance(_type) is not T result)
			throw new DataLoaderException($"Couldn't create the instance of '{typeof(T)}'");
		foreach (var p in _props)
		{
			if (_keyMap.TryGetValue(p.Name, out Int32 fieldIndex))
			{
				var dbVal = rdr.GetValue(fieldIndex);
				if (dbVal == DBNull.Value)
					continue;
				var pt = p.PropertyType.GetNonNullableType();
				if (pt.IsEnum)
					p.SetValue(result, Enum.Parse(pt, dbVal.ToString()!));
				else if (pt == typeof(ExpandoObject))
					p.SetValue(result, DataHelpers.DeserializeJson(dbVal.ToString()));
				else if (pt.IsAssignableFrom(typeof(MemoryStream)))
				{
					if (dbVal is Byte[] dbByteArray)
						p.SetValue(result, new MemoryStream(dbByteArray));
					else
						throw new DataLoaderException($"Property '{p.Name}' (Stream). Source must be varbianry");
				}
				else
					p.SetValue(result, dbVal);
			}
		}
		return result;
	}
}

