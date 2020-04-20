﻿// Copyright © 2012-2017 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace A2v10.Data
{
	internal class IdMapper : Dictionary<Tuple<String, Object>, ExpandoObject>
	{
		public ExpandoObject Add(String typeName, Object id, ExpandoObject value)
		{
			var key = Tuple.Create(typeName, id);
			if (!TryGetValue(key, out ExpandoObject valObj))
			{
				Add(key, value);
				valObj = value;
			}
			return valObj;
		}
	}

	internal class RefMapperItem
	{
		public IList<ExpandoObject> List;
		public ExpandoObject Source;

		public RefMapperItem()
		{
		}

		public void AddToList(ExpandoObject eo)
		{
			if (List == null)
				List = new List<ExpandoObject>();
			List.Add(eo);
		}
	}

	internal class RefMapper : Dictionary<Tuple<String, Object>, RefMapperItem>
	{
		public void Add(String typeName, Object id, ExpandoObject value)
		{
			var key = Tuple.Create(typeName, id);
			if (!TryGetValue(key, out RefMapperItem item))
			{
				item = new RefMapperItem();
				Add(key, item);
			}
			item.AddToList(value);
			if (item.Source != null)
			{
				foreach (var target in item.List)
					target.CopyFrom(item.Source);
			}
		}

		public void MergeObject(String typeName, Object id, ExpandoObject source)
		{
			var key = Tuple.Create(typeName, id);
			if (TryGetValue(key, out RefMapperItem item))
			{
				if (item != null)
				{
					if ((item.Source == null) && (source != null))
						item.Source = source;
					if (item.List != null)
						foreach (var target in item.List)
							target.CopyFrom(source);
				}
			}
			else
			{
				// forward definition
				item = new RefMapperItem
				{
					Source = source
				};
				Add(key, item);
			}
		}
	}
}
