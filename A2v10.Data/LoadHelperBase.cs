// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

namespace A2v10.Data
{
	public abstract class LoadHelperBase<T> where T : class
	{
		private readonly Type _type;
		readonly PropertyInfo[] _props;
		Dictionary<String, Int32> _keyMap;

		public LoadHelperBase()
		{
			_type = typeof(T);
			_props = _type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		}

		public void ProcessMetadata(IDataReader rdr)
		{
			_keyMap = new Dictionary<String, Int32>();
			for (Int32 c = 0; c < rdr.FieldCount; c++)
			{
				_keyMap.Add(rdr.GetName(c), c);
			}
		}

		public T CreateInstance(IDataReader rdr)
		{
			T result = System.Activator.CreateInstance(_type) as T;
			foreach (var p in _props)
			{
				if (_keyMap.TryGetValue(p.Name, out Int32 fieldIndex))
				{
					var dbVal = rdr.GetValue(fieldIndex);
					if (dbVal == DBNull.Value)
						continue;
					var pt = p.PropertyType.GetNonNullableType();
					if (pt.IsEnum)
						p.SetValue(result, Enum.Parse(pt, dbVal.ToString()));
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
}
