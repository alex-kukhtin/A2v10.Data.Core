// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Globalization;

namespace A2v10.Data.Providers
{
	public sealed class FieldData
	{
		public DateTime DateValue { get; set; }
		public Decimal DecimalValue { get; set; }
		public String StringValue { get; set; }
		public Boolean BooleanValue { get; set; }

		public FieldType FieldType { get; set; }

		public Object Value
		{
			get
			{
				switch (FieldType)
				{
					case FieldType.Char:
					case FieldType.Memo:
						return StringValue;
					case FieldType.Numeric:
					case FieldType.Float:
						return DecimalValue;
					case FieldType.Boolean:
						return BooleanValue;
					case FieldType.Date:
						return DateValue;
				}
				throw new InvalidOperationException($"Invalid FieldType: {FieldType}");
			}
		}

		public Boolean IsEmpty
		{
			get
			{
				switch (FieldType)
				{
					case FieldType.Char:
					case FieldType.Memo:
						return String.IsNullOrEmpty(StringValue);
					case FieldType.Numeric:
					case FieldType.Float:
						return DecimalValue == 0M;
					case FieldType.Boolean:
						return BooleanValue == false;
					case FieldType.Date:
						return DateValue == DateTime.MinValue;
				}
				return true;
			}
		}

		public FieldData()
		{
		}

		public FieldData(DataFileFormat format, FieldType type, Object value)
		{
			FieldType = type;
			switch (format)
			{
				case DataFileFormat.dbf:
					SetValueDbf(value);
					break;
				case DataFileFormat.csv:
					SetValueCsv(value);
					break;
				default:
					throw new InvalidProgramException($"Unknown DataFile format: '{format}'");
			}
		}

		private void SetValueDbf(Object value)
		{
			switch (FieldType)
			{
				case FieldType.Char:
					StringValue = value?.ToString();
					break;
				case FieldType.Date:
					if (value is DateTime dtv)
						DateValue = dtv;
					break;
				case FieldType.Boolean:
					if (value is Boolean bv)
						BooleanValue = bv;
					break;
				case FieldType.Numeric:
					switch (value)
					{
						case Int64 i64v:
							DecimalValue = Convert.ToDecimal(i64v);
							break;
						case Int32 i32v:
							DecimalValue = Convert.ToDecimal(i32v);
							break;
						case Int16 i16v:
							DecimalValue = Convert.ToDecimal(i16v);
							break;
						case Decimal dcv:
							DecimalValue = dcv;
							break;
						case Double dblv:
							DecimalValue = Convert.ToDecimal(dblv);
							break;
					}
					break;
			}
		}

		private void SetValueCsv(Object value)
		{
			if (value == null)
				return;
			switch (value)
			{
				case String strVal:
					StringValue = strVal?.ToString();
					break;
				case DateTime dtVal:
					StringValue = dtVal.ToString("yyyy-MM-dd");
					break;
				case Int64 _:
				case Int32 _:
				case Int16 _:
					StringValue = Convert.ToInt64(value).ToString(CultureInfo.InvariantCulture);
					break;
				case Decimal dcVal:
					StringValue = dcVal.ToString(CultureInfo.InvariantCulture);
					break;
				case Boolean boolVal:
					StringValue = boolVal ? "true" : "false";
					break;
				case Double dblVal:
					StringValue = dblVal.ToString(CultureInfo.InvariantCulture);
					break;
			}
		}
	}
}
