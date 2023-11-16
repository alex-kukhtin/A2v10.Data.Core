// Copyright © 2012-2020 Oleksandr Kukhtin. All rights reserved.

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
	Int32 FieldIndex { get; }
	String GetObjectType(String fieldName);
    void ToDynamicGroup();

    String TypeForValidate { get; }
	String TypeScriptName { get; }

	Boolean IsRefId { get; }
}

