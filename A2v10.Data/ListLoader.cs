using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.Data
{
	public class ListLoader<T> where T : class
	{
		private readonly Type _retType;
		private readonly PropertyInfo[] _props;
		Dictionary<String, Int32> _keyMap;

		public List<T> Result;

		public ListLoader()
		{
			_retType = typeof(T);
			_props = _retType.GetProperties();
			Result = new List<T>();
		}

		public void ProcessFields(IDataReader rdr)
		{
			_keyMap = new Dictionary<String, Int32>();
			for (Int32 c = 0; c < rdr.FieldCount; c++)
			{
				_keyMap.Add(rdr.GetName(c), c);
			}
		}

		public void ProcessRecord(IDataReader rdr)
		{
			T item = System.Activator.CreateInstance(_retType) as T;
			foreach (var p in _props)
			{
				if (_keyMap.TryGetValue(p.Name, out Int32 fieldIndex))
				{
					var dbVal = rdr.GetValue(fieldIndex);
					if (dbVal == DBNull.Value)
						dbVal = null;
					p.SetValue(item, dbVal);
				}
			}
			Result.Add(item);
		}
	}
}
