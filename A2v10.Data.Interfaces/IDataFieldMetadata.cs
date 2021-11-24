// Copyright © 2012-2020 Alex Kukhtin. All rights reserved.

namespace A2v10.Data.Interfaces;

public enum SqlDataType
{
	Unknown,
	String,
	Int,
	Bigint,
	Bit,
	Float,
	Decimal,
	Numeric,
	Currency,
	Date,
	DateTime,
	Time,
	Binary,
	Guid
}

public interface IDataFieldMetadata
{
	SqlDataType SqlDataType { get; }
	Boolean IsLazy { get; }
	Boolean IsJson { get; }

	String RefObject { get; }
	Int32 Length { get; }

	String GetObjectType(String fieldName);

	String TypeForValidate { get; }
	String TypeScriptName { get; }
}

