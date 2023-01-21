// Copyright © 2012-2022 Alex Kukhtin. All rights reserved.


namespace A2v10.Data;

public enum DataType
{
	Undefined,
	String,
	Number,
	Date,
	Boolean,
	Blob
}

public enum FieldType
{
	Scalar,
	Object,
	Array,
	Map,
	Tree,
	Group,
	MapObject,
	Json,
	CrossArray,
	CrossObject
}

public enum SpecType
{
	Unknown,
	Id,
	Key,
	Name,
	UtcDate,
	RefId,
	ParentId,
	RowCount,
	RowNumber,
	HasChildren,
	Items,
	Expanded,
	Permissions,
	GroupMarker,
	ReadOnly,
	Copy,
	SortOrder,
	SortDir,
	PageSize,
	Offset,
	GroupBy,
	Filter,
	HasRows,
	Json,
	Utc,
	Token
}

public class FieldMetadata : IDataFieldMetadata
{
	public DataType DataType { get; }
	public FieldType ItemType { get; } // for object, array
	public String RefObject { get; private set; } // for object, array
	public Boolean IsLazy { get; }
	public Int32 Length { get; }
	public Boolean IsJson { get; set; }
	public SqlDataType SqlDataType { get; }

	public Int32 FieldIndex { get; }

	public Boolean IsArrayLike { get { return ItemType == FieldType.Object || ItemType == FieldType.Array || ItemType == FieldType.Map; } }

	public FieldMetadata(Int32 index, FieldInfo fi, DataType type, SqlDataType sqlDataType, Int32 length)
	{
		FieldIndex = index;
		DataType = type;
		SqlDataType = sqlDataType;
		Length = length;
		IsLazy = fi.IsLazy;
		ItemType = FieldType.Scalar;
            RefObject = String.Empty;

            if (fi.IsObjectLike)
		{
			ItemType = fi.FieldType;
			RefObject = fi.TypeName;
		}
		else if (fi.IsRefId)
		{
			ItemType = FieldType.Object;
			RefObject = fi.TypeName;
		}
	}

	public String GetObjectType(String fieldName)
	{
		switch (ItemType)
		{
			case FieldType.Array:
			case FieldType.Tree:
			case FieldType.Map:
			case FieldType.CrossArray:
				return RefObject + "Array";
			case FieldType.Object:
			case FieldType.CrossObject:
			case FieldType.Group:
				return RefObject;
			case FieldType.MapObject:
				return RefObject + "Map";
			case FieldType.Json:
				return "Json";
			default:
				if (DataType == DataType.Undefined)
					throw new DataLoaderException($"Invalid data type for '{fieldName}'");
				else
					return DataType.ToString();
		}
	}

	public void SetType(String type)
	{
		RefObject = type;
	}

	public String TypeForValidate =>
		ItemType switch
		{
			FieldType.Array or
			FieldType.Tree or
			FieldType.Map or
			FieldType.MapObject => RefObject + "[]",
			FieldType.Object or
			FieldType.Group => RefObject,
			_ => DataType.ToString(),
		};


	public String TypeScriptName =>
		ItemType switch
		{
			FieldType.Scalar => DataType switch
			{
				DataType.Number or
				DataType.String or
				DataType.Boolean => DataType.ToString().ToLowerInvariant(),
				DataType.Date => "Date",
				_ => DataType.ToString(),
			},
			FieldType.Array or
			FieldType.Tree => $"IElementArray<{RefObject}>",
			FieldType.Map or
			FieldType.MapObject => RefObject + "[]",
			FieldType.Object or
			FieldType.Group => RefObject,
			_ => DataType.ToString(),
		};

}
