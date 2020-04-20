﻿// Copyright © 2012-2020 Alex Kukhtin. All rights reserved.


using System;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

using A2v10.Data.Interfaces;

namespace A2v10.Data
{
	public static class DataHelpers
	{
		public static DataType TypeName2DataType(this String s)
		{
			switch (s)
			{
				case "DateTime":
					return DataType.Date;
				case "String":
					return DataType.String;
				case "Int64":
				case "Int32":
				case "Int16":
				case "Double":
				case "Decimal":
					return DataType.Number;
				case "Boolean":
					return DataType.Boolean;
				case "Guid":
					return DataType.String;
				case "Byte":
					return DataType.Number;
				case "Byte[]":
					return DataType.Blob;
			}
			throw new DataLoaderException($"Invalid data type {s}");
		}

		public static FieldType TypeName2FieldType(this String s)
		{
			switch (s)
			{
				case "Object":
				case "LazyObject":
				case "MainObject":
					return FieldType.Object;
				case "MapObject":
					return FieldType.MapObject;
				case "Array":
				case "LazyArray":
					return FieldType.Array;
				case "Map":
					return FieldType.Map;
				case "Tree":
					return FieldType.Tree;
				case "Items": // for tree element
					return FieldType.Array;
				case "Group":
					return FieldType.Group;
				case "CrossArray":
					return FieldType.CrossArray;
				case "CrossObject":
					return FieldType.CrossObject;
				case "Json":
					return FieldType.Json;
			}
			return FieldType.Scalar;
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
			foreach (var itm in dSource)
			{
				dTarget.Add(itm.Key, itm.Value);
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
			Boolean rv;
			if (dataVal is Boolean)
				rv = (Boolean)dataVal;
			else if (dataVal is Int32)
				rv = ((Int32)dataVal) != 0;
			else if (dataVal is Int16)
				rv = ((Int16)dataVal) != 0;
			else
				throw new DataLoaderException($"Could not convert {dataVal.GetType()} to Boolean");
			return rv;
		}

		public static Object DateTime2StringWrap(Object val)
		{
			if (!(val is DateTime)) return val;
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
