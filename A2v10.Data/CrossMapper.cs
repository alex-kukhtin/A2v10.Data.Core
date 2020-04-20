// Copyright © 2019-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace A2v10.Data
{
	internal class CrossItem
	{
		private readonly List<ExpandoObject> _list = new List<ExpandoObject>();
		private readonly Dictionary<String, Int32> _keys = new Dictionary<String, Int32>();
		public String TargetProp { get; }
		public Boolean IsArray { get; }
		public String CrossType { get; }


		public CrossItem(String targetProp, Boolean isArray, String crossType)
		{
			TargetProp = targetProp;
			IsArray = isArray;
			CrossType = crossType;
		}

		public void Add(String propName, ExpandoObject target)
		{
			_list.Add(target);
			if (!_keys.ContainsKey(propName))
			{
				_keys.Add(propName, _keys.Count);
			}
		}

		public List<String> GetCross()
		{
			var l = new List<String>();
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
			foreach (var eo in _list)
			{
				var arr = CreateArray(_keyCount);
				ExpandoObject targetVal = eo.Get<ExpandoObject>(TargetProp);
				foreach (var key in _keys)
				{
					Int32 index = key.Value;
					arr[index] = targetVal.Get<ExpandoObject>(key.Key);
				}
				eo.Set(TargetProp, arr);
			}
		}

		static List<ExpandoObject> CreateArray(Int32 cnt)
		{
			var l = new List<ExpandoObject>();
			for (Int32 i = 0; i < cnt; i++)
			{
				l.Add(new ExpandoObject());
			}
			return l;
		}

	}

	internal class CrossMapper : Dictionary<String, CrossItem>
	{
		public void Add(String key, String targetProp, ExpandoObject target, String propName, FieldInfo rootFI)
		{
			if (!TryGetValue(key, out CrossItem crossItem))
			{
				crossItem = new CrossItem(targetProp, rootFI.IsCrossArray, rootFI.TypeName);
				Add(key, crossItem);
			}
			// all source elements
			crossItem.Add(propName, target);
		}

		public void Transform()
		{
			foreach (var x in this)
				x.Value.Transform();
		}
	}
}
