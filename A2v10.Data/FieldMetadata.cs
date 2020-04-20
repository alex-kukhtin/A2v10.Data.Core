// Copyright © 2012-2019 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using System;

namespace A2v10.Data
{
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
		Utc
	}

	public class FieldMetadata : IDataFieldMetadata
	{
		public DataType DataType { get; }
		public FieldType ItemType { get; } // for object, array
		public String RefObject { get; private set; } // for object, array
		public Boolean IsLazy { get; }
		public Int32 Length { get; }
		public Boolean IsJson { get; set; }

		public Boolean IsArrayLike { get { return ItemType == FieldType.Object || ItemType == FieldType.Array || ItemType == FieldType.Map; } }

		public FieldMetadata(FieldInfo fi, DataType type, Int32 length)
		{
			DataType = type;
			Length = length;
			IsLazy = fi.IsLazy;
			ItemType = FieldType.Scalar;
			RefObject = null;
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

		public String TypeForValidate
		{
			get
			{
				switch (ItemType)
				{
					case FieldType.Array:
					case FieldType.Tree:
					case FieldType.Map:
					case FieldType.MapObject:
						return RefObject + "[]";
					case FieldType.Object:
					case FieldType.Group:
						return RefObject;
					default:
						return DataType.ToString();
				}
			}
		}

	}
}
