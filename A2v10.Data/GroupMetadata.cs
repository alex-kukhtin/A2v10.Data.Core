// Copyright © 2012-2023 Oleksandr Kukhtin. All rights reserved.

using System.Linq;
using System.Text;

namespace A2v10.Data;
public class GroupMetadata
{
	IList<String>? _fields = null;

	IDictionary<String, ExpandoObject>? _cache = null;

	internal static String RootKey { get { return "[ROOT]\b"; } }


	public static IDictionary<String, IList<String>> GetLevels(IDictionary<String, GroupMetadata> dict)
	{
		var rv = new Dictionary<String, IList<String>>();
		foreach (var (k, v) in dict)
		{
			if (v._fields != null)
				rv.Add(k, v._fields);
		}
		return rv;
	}

	public void AddMarkerMetadata(String fieldName)
	{
		_fields ??= new List<String>();
		_fields.Add(fieldName);
	}

	public Boolean IsRoot(IList<Boolean> groups)
	{
		if (_fields == null)
			throw new DataLoaderException("Fields yet not created");
		if (groups.Count != _fields.Count)
			throw new DataLoaderException("Invalid group");
		return groups.Count(x => x == true) == _fields.Count;
	}

	public Boolean IsLeaf(IList<Boolean> groups)
	{
		if (_fields == null)
			throw new DataLoaderException("Fields yet not created");
		if (groups.Count != _fields.Count)
			throw new DataLoaderException("Invalid group");
		return groups.Count(x => x == false) == _fields.Count;
	}

	public void CacheElement(String key, ExpandoObject record)
	{
		_cache ??= new Dictionary<String, ExpandoObject>();
		if (_cache.ContainsKey(key))
			throw new DataLoaderException($"Group.Cache. Element with the key '{key}' already has been added.");
		_cache.Add(key, record);
	}

	public ExpandoObject GetCachedElement(String key)
	{
		if (_cache == null)
			throw new DataLoaderException($"Group.Cache. There is no element with the key '{key}'.");
		if (_cache.TryGetValue(key, out ExpandoObject? val))
			return val;
		throw new DataLoaderException($"Group.Cache. There is no element with the key '{key}'.");
	}

	public Tuple<String, String> GetKeys(IList<Boolean> groupKeys, ExpandoObject currentRecord)
	{
		if (_fields == null)
			throw new DataLoaderException("Fields yet not created");
		StringBuilder sbKey = new(RootKey);
		StringBuilder sbParent = new(RootKey);
		String? value = null;
		// the groupKeys array is already sorted (SQL)
		for (Int32 i = 0; i < groupKeys.Count; i++)
		{
			if (groupKeys[i])
				break; // here and below - only groups 
			if (value != null)
				sbParent.Append($"[{value}]\b"); // prev tick
			var fieldName = _fields.ElementAt(i);
			value = currentRecord.GetObject(fieldName)?.ToString() ?? String.Empty;
			sbKey.Append($"[{value}]\b");
		}
		return Tuple.Create(sbKey.ToString(), sbParent.ToString());
	}
}

