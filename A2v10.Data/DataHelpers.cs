// Copyright © 2012-2024 Oleksandr Kukhtin. All rights reserved.

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

