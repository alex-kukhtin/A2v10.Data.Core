// Copyright © 2015-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data.Providers;
public sealed record Field
{
	public String Name { get; set; }
	public Int32 Size { get; set; }
	public Int32 Decimal { get; set; }
	public FieldType Type { get; set; }

	public Field(String name, FieldType type = default)
    {
		Name = name;
		Type = type;
    }

	internal void SetFieldTypeDbf(SqlDataType dataType, Int32? len, Int32? dec)
	{
		switch (dataType)
		{
			case SqlDataType.Bit:
				Type = FieldType.Boolean;
				Size = 1;
				break;
			case SqlDataType.String:
				Type = FieldType.Char;
				Size = len ?? 254; // max size for dbf
				break;
			case SqlDataType.Guid:
				Type = FieldType.Char;
				Size = 50;
				break;
			case SqlDataType.Date:
			case SqlDataType.Time:
			case SqlDataType.DateTime:
				Type = FieldType.Date;
				Size = 8;
				break;
			case SqlDataType.Bigint:
			case SqlDataType.Int:
				Type = FieldType.Numeric;
				Size = len ?? 19;
				Decimal = dec ?? 0;
				break;
			case SqlDataType.Decimal:
			case SqlDataType.Currency:
				Type = FieldType.Numeric;
				Size = len ?? 19;
				Decimal = dec ?? 4;
				break;
			case SqlDataType.Float:
				Type = FieldType.Numeric;
				Size = len ?? 19;
				Decimal = dec ?? 8;
				break;
		}
	}

	internal void SetFieldTypeCsv()
	{
		Type = FieldType.Char;
		Size = 0;
	}
}

