// Copyright © 2012-2024 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using System.Globalization;

using Newtonsoft.Json;

namespace A2v10.Data;
public static class DataHelpers
{
	public static Boolean IsIdIsNull(Object? id) =>
		id switch
		{
			null => true,
			Int64 int64 => int64 == 0,
			Int32 int32 => int32 == 0,
			Int16 int16 => int16 == 0,
			String strVal => String.IsNullOrEmpty(strVal),
			_ => false
		};

	public static void Add(this ExpandoObject eo, String key, Object? value)
	{
		var d = eo as IDictionary<String, Object?>;
		d.Add(key, value);
	}

	public static Boolean AddChecked(this ExpandoObject eo, String key, Object? value)
	{
		var d = eo as IDictionary<String, Object?>;
		if (d.ContainsKey(key))
			return false;
		d.Add(key, value);
		return true;
	}

	public static void CopyFrom(this ExpandoObject target, ExpandoObject? source)
	{
		if (source == null)
			return;
		var dTarget = target as IDictionary<String, Object?>;
		if (dTarget.Count != 0)
			return; // skip if already filled
		var dSource = source as IDictionary<String, Object?>;
		foreach (var (k, v) in dSource)
		{
			dTarget.Add(k, v);
		}
	}

	public static void CopyFromUnconditional(this ExpandoObject target, ExpandoObject? source)
	{
		if (source == null) 
			return;	
		var dTarget = target as IDictionary<String, Object>;
		var dSource = source as IDictionary<String, Object>;
		foreach (var itm in dSource)
		{
			dTarget[itm.Key] = itm.Value;
		}
	}

	public static Object GetDateParameter(this ExpandoObject? eo, String name)
	{
		var val = eo?.Get<Object>(name);
		if (val == null)
			return DBNull.Value;
		if (val is DateTime dt)
			return dt;
		else if (val is String strVal)
			return DateTime.ParseExact(strVal, "yyyyMMdd", CultureInfo.InvariantCulture);
		throw new InvalidExpressionException($"Invalid Date Parameter value: {val}");
	}

	public static IDictionary<String, Object?> GetOrCreate(this IDictionary<String, Object?> dict, String key)
	{
		if (dict.TryGetValue(key, out Object? obj))
			return (obj as IDictionary<String, Object?>)!;
		obj = new ExpandoObject();
		dict.Add(key, obj);
		return (obj as IDictionary<String, Object?>)!;
	}

	private static readonly JsonSerializerSettings JsonIsoDateSettings =
		new() { DateFormatHandling = DateFormatHandling.IsoDateFormat, DateTimeZoneHandling = DateTimeZoneHandling.Unspecified };
	public static Object DateTime2StringWrap(Object val)
	{
		if (val is not DateTime)
			return val;
		return $"\"\\/{ JsonConvert.SerializeObject(val, JsonIsoDateSettings)}\\/\"";
	}
	public static Object DateTime2StringWrap2(Object val)
	{
		if (val is not DateTime)
			return val;
		return $"'\\/{JsonConvert.SerializeObject(val, JsonIsoDateSettings)}\\/'";
	}

	public static ExpandoObject? DeserializeJson(String? data) =>
		data == null ? null : JsonConvert.DeserializeObject<ExpandoObject>(data);
}

public class DataHelper : IDataHelper
{
	public Object DateTime2StringWrap(Object val)
	{
		return DataHelpers.DateTime2StringWrap(val);
	}
}

