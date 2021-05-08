// Copyright © 2019-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace A2v10.Data
{
	internal class CrossItem
	{
		readonly Dictionary<Object, ExpandoObject> _items = new();
		readonly Dictionary<String, Int32> _keys = new();
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
			var id = target.Get<Object>("Id");
			if (!_items.ContainsKey(id))
				_items.Add(id, target);
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
			foreach (var (_, eo) in _items)
			{
				var arr = CreateArray(_keyCount);
				ExpandoObject targetVal = eo.Get<ExpandoObject>(TargetProp);
				if (targetVal == null)
					continue; // already array?
				foreach (var (key, index) in _keys)
				{
					arr[index] = targetVal.Get<ExpandoObject>(key);
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
