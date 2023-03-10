// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.


namespace A2v10.Data;
internal class ObjectBuilder
{
	static Object CreateObjectSimple(ExpandoObject source, Signature sign, String path)
	{
		if (source is null)
			throw new DataDynamicException($"Invalid dynamic object. {sign}");
		var type = ClassFactory.CreateClass(sign.Properties);
		var target = System.Activator.CreateInstance(type) 
			?? throw new DataDynamicException($"Couldn't create type {type}");
        SetProperties(source, target, path);
		return target;
	}

	static Object? CreateObject(Object? source, String path)
	{
		if (source == null)
			return null;
		if (source is ExpandoObject expSource)
		{
			var sign = new Signature(expSource);
			return CreateObjectSimple(expSource, sign, path);
		}
		else if (source is IList<ExpandoObject> explist)
		{
			var retList = new List<Object>();
			Signature? arraySign = null;
			foreach (var listItem in explist)
			{
				if (listItem is ExpandoObject expListItem)
				{
					arraySign ??= new Signature(expListItem);
					retList.Add(CreateObjectSimple(expListItem, arraySign, path));
				}
				else
				{
					retList.Add(listItem);
				}
			}
			return retList;
		}
		return source;
	}

	static void SetProperties(ExpandoObject source, Object target, String path)
	{
		var props = target.GetType().GetProperties();
		var dict = source as IDictionary<String, Object>;
		foreach (var prop in props)
		{
			if (dict.TryGetValue(prop.Name, out var val))
				prop.SetValue(target, CreateObject(val, $"{path}.{prop.Name}"));
		}
	}

	public static Dictionary<String, Object> BuildObject(ExpandoObject root)
	{
		var list = new Dictionary<String, Object>();
		foreach (var (k, v) in root)
		{
			var o = CreateObject(v, k);
			if (o != null)
				list.Add(k, o);
		}
		return list;
	}
}

