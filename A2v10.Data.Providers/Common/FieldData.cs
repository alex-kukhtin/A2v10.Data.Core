// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data.Providers
{
	public sealed class FieldData 
	{
		public DateTime DateValue { get; set; }
		public Decimal DecimalValue { get; set; }
		public String StringValue { get; set; }
		public Boolean BooleanValue { get; set; }

		public FieldType FieldType { get; set; }

		public Object Value {
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
	}
}
