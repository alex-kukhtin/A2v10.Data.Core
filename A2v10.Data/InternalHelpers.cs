// Copyright © 2012-2023 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data;

internal static class InternalHelpers
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
	public static Object SqlDataTypeDefault(this SqlDataType s)
	{
		return s switch
		{
			SqlDataType.Decimal => (Decimal)0,
			SqlDataType.Currency => (Decimal)0,
			SqlDataType.Float => (Double)0,
			SqlDataType.Bigint => (Int64)0,
			SqlDataType.Int => (Int32)0,
			_ => throw new DataLoaderException($"SqlDataType not supported 's'")
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
			"Sheet" => FieldType.Sheet,
			"Rows" => FieldType.Rows,
			"Columns" => FieldType.Columns,
			"Cells" => FieldType.Cells,
			"Map" => FieldType.Map,
			"Tree" => FieldType.Tree,
			// for tree element
			"Items" => FieldType.Array,
			"Group" => FieldType.Group,
			"CrossArray" => FieldType.CrossArray,
			"CrossObject" => FieldType.CrossObject,
			"Json" => FieldType.Json,
			"Lookup" => FieldType.Lookup,
			_ => FieldType.Scalar,
		};
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

	public static SpecType TypeName2SpecType(this String s)
	{
		if (Enum.TryParse<SpecType>(s, out SpecType st))
			return st;
		return SpecType.Unknown;
	}
	public static void AddToArray(this ExpandoObject eo, String key, ExpandoObject? value)
	{
		var arr = eo.GetArray(key);
		if (value != null)
			arr?.Add(value);
	}
	public static List<ExpandoObject> GetArray(this ExpandoObject eo, String key)
	{
		var d = eo as IDictionary<String, Object?>;
		List<ExpandoObject>? arr;
		if (!d.TryGetValue(key, out Object? objArr))
		{
			arr = [];
			d.Add(key, arr);
		}
		else
		{
			if (objArr is not List<ExpandoObject> eobjArr)
				throw new InvalidProgramException("AddToArrayColumns. Invalid element type");
			arr = eobjArr;
		}
		return arr;
	}

	public static void AddToArrayIndex(this ExpandoObject eo, String key, ExpandoObject? value, Int32 index, String indexName)
	{
		var arr = eo.GetArray(key);
		if (value == null)
			return;
		while ((arr.Count - 1) < index)
			arr.Add(new ExpandoObject()
			{
				{ indexName, arr.Count + 1 /* 1 based */ }
			});
		arr[index] = value;
	}

	public static void AddToArrayIndexKey(this ExpandoObject eo, String key, ExpandoObject? value, String index, String indexName)
	{
		var arr = eo.GetArray(key);
		if (value == null)
			return;
		var int32Index = index.ColumnKey2Index() - 1;
		while ((arr.Count - 1) < int32Index)
			arr.Add(new ExpandoObject()
			{
				{ indexName, Index2Col(arr.Count) }
			});
		arr[int32Index] = value;
	}

	public static void AddToArrayIndexCell(this ExpandoObject eo, String key, ExpandoObject? value, String columnKey)
	{
		var arr = eo.GetArray(key);
		if (value == null)
			return;
		var int32Index = columnKey.ColumnKey2Index() - 1;
		while ((arr.Count - 1) < int32Index)
			arr.Add([]);
		arr[int32Index] = value;
	}

    public static String Index2Col(Int32 index)
	{
		Int32 q = index / 26;

		if (q > 0)
			return Index2Col(q - 1) + (Char)((Int32)'A' + (index % 26));
		else
			return "" + (Char)((Int32)'A' + index);
	}

	public static Int32 ColumnKey2Index(this String? refs)
	{
		if (refs == null)
			return 0;
		Int32 ci = 0;
		refs = refs.ToUpper();
		for (Int32 ix = 0; ix < refs.Length && refs[ix] >= 'A'; ix++)
			ci = (ci * 26) + ((Int32)refs[ix] - 64);
		return ci;
	}

	public static ExpandoObject CreateOrAddObject(this ExpandoObject eo, String key)
	{
		var d = eo as IDictionary<String, Object>;
		if (d.TryGetValue(key, out Object? value))
			return (ExpandoObject)value;
		var neweo = new ExpandoObject();
		d.Add(key, neweo);
		return neweo;
	}

	public static void AddToMap(this ExpandoObject eo, String key, ExpandoObject value, String keyProp)
	{
		var d = eo as IDictionary<String, Object?>;
		if (!d.TryGetValue(key, out _))
			d.Add(key, new ExpandoObject());
		if (d[key] is ExpandoObject expVal)
			expVal.Set(keyProp, value);
	}


	public static void AddToCross(this ExpandoObject eo, String key, ExpandoObject value, String keyProp)
	{
		var d = eo as IDictionary<String, Object?>;
		ExpandoObject? val = d[key] as ExpandoObject;
		if (val == null)
		{
			val = [];
			eo.Set(key, val);
		}
		val.Set(keyProp, value);
	}
}
