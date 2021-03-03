// Copyright © 2012-2020 Alex Kukhtin. All rights reserved.


using A2v10.Data.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace A2v10.Data
{
	public static class DataHelpers
	{
		public static DataType TypeName2DataType(this String s)
		{
			return s switch
			{
				"DateTime" => DataType.Date,
				"TimeSpan" => DataType.Date,
				"String" => DataType.String,
				"Int64" or "Int32" or "Int16" or "Double" or "Decimal" => DataType.Number,
				"Boolean" => DataType.Boolean,
				"Guid" => DataType.String,
				"Byte" => DataType.Number,
				"Byte[]" => DataType.Blob,
				_ => throw new DataLoaderException($"Invalid data type {s}"),
			};
		}

		public static SqlDataType SqlTypeName2SqlDataType(this String s)
		{
			return s switch
			{
				"datetime" or "datetime2" or "smalldatetime" or "datetimeoffset" => SqlDataType.DateTime,
				"date" => SqlDataType.Date,
				"time" => SqlDataType.Time,
				"nvarchar" or "varchar" or "nchar" or "char" or "text" or "ntext" => SqlDataType.String,
				"bit" => SqlDataType.Bit,
				"int" or "smallint" or "tinyint" => SqlDataType.Int,
				"bigint" => SqlDataType.Bigint,
				"float" or "real" => SqlDataType.Float,
				"numeric" => SqlDataType.Numeric,
				"decimal" => SqlDataType.Decimal,
				"money" or "smallmoney" => SqlDataType.Currency,
				"binary" or "varbinary" or "image" => SqlDataType.Binary,
				"uniqueidentifier" => SqlDataType.Guid,
				_ => SqlDataType.Unknown,
			};
		}

		public static FieldType TypeName2FieldType(this String s)
		{
			return s switch
			{
				"Object" or "LazyObject" or "MainObject" => FieldType.Object,
				"MapObject" => FieldType.MapObject,
				"Array" or "LazyArray" => FieldType.Array,
				"Map" => FieldType.Map,
				"Tree" => FieldType.Tree,
				// for tree element
				"Items" => FieldType.Array,
				"Group" => FieldType.Group,
				"CrossArray" => FieldType.CrossArray,
				"CrossObject" => FieldType.CrossObject,
				"Json" => FieldType.Json,
				_ => FieldType.Scalar,
			};
		}

		public static SpecType TypeName2SpecType(this String s)
		{
			if (Enum.TryParse<SpecType>(s, out SpecType st))
				return st;
			return SpecType.Unknown;
		}

		public static Boolean IsIdIsNull(Object id)
		{
			if (id == null)
				return true;
			var tp = id.GetType();
			if (tp == typeof(Int64))
				return (Int64)id == 0;
			else if (tp == typeof(Int32))
				return (Int32)id == 0;
			else if (tp == typeof(Int16))
				return (Int16)id == 0;
			else if (tp == typeof(String))
				return String.IsNullOrEmpty(id.ToString());
			return false;
		}

		public static void Add(this ExpandoObject eo, String key, Object value)
		{
			var d = eo as IDictionary<String, Object>;
			d.Add(key, value);
		}

		public static Boolean AddChecked(this ExpandoObject eo, String key, Object value)
		{
			var d = eo as IDictionary<String, Object>;
			if (d.ContainsKey(key))
				return false;
			d.Add(key, value);
			return true;
		}

		public static void AddToArray(this ExpandoObject eo, String key, ExpandoObject value)
		{
			var d = eo as IDictionary<String, Object>;
			List<ExpandoObject> arr;
			if (!d.TryGetValue(key, out Object objArr))
			{
				arr = new List<ExpandoObject>();
				d.Add(key, arr);
			}
			else
			{
				arr = objArr as List<ExpandoObject>;
			}
			if (value != null)
				arr.Add(value);
		}

		public static void AddToMap(this ExpandoObject eo, String key, ExpandoObject value, String keyProp)
		{
			var d = eo as IDictionary<String, Object>;
			if (!d.TryGetValue(key, out _))
			{
				Object objVal = new ExpandoObject();
				d.Add(key, objVal);
			}
			var val = d[key] as ExpandoObject;
			val.Set(keyProp, value);
		}


		public static void AddToCross(this ExpandoObject eo, String key, ExpandoObject value, String keyProp)
		{
			var d = eo as IDictionary<String, Object>;
#pragma warning disable IDE0019 // Use pattern matching
			ExpandoObject val = d[key] as ExpandoObject;
#pragma warning restore IDE0019 // Use pattern matching
			if (val == null)
			{
				val = new ExpandoObject();
				eo.Set(key, val);
			}
			val.Set(keyProp, value);
		}

		public static void CopyFrom(this ExpandoObject target, ExpandoObject source)
		{
			var dTarget = target as IDictionary<String, Object>;
			if (dTarget.Count != 0)
				return; // skip if already filled
			var dSource = source as IDictionary<String, Object>;
			foreach (var (k, v) in dSource)
			{
				dTarget.Add(k, v);
			}
		}

		public static IDictionary<String, Object> GetOrCreate(this IDictionary<String, Object> dict, String key)
		{
			if (dict.TryGetValue(key, out Object obj))
				return obj as IDictionary<String, Object>;
			obj = new ExpandoObject();
			dict.Add(key, obj);
			return obj as IDictionary<String, Object>;
		}

		public static Boolean SqlToBoolean(Object dataVal)
		{
			if (dataVal == null)
				return false;
			if (dataVal == DBNull.Value)
				return false;
			return dataVal switch
			{
				Boolean boolVal => boolVal,
				Int32 int32val => int32val != 0,
				Int16 int16val => int16val != 0,
				_ => throw new DataLoaderException($"Could not convert {dataVal.GetType()} to Boolean"),
			};
		}

		public static Object DateTime2StringWrap(Object val)
		{
			if (!(val is DateTime))
				return val;
			return "\"\\/" +
				JsonConvert.SerializeObject(val, new JsonSerializerSettings() { DateFormatHandling = DateFormatHandling.IsoDateFormat, DateTimeZoneHandling = DateTimeZoneHandling.Utc }) +
				"\\/\"";
		}
	}

	public class DataHelper : IDataHelper
	{
		public Object DateTime2StringWrap(Object val)
		{
			return DataHelpers.DateTime2StringWrap(val);
		}
	}
}
