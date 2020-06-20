// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data.Providers
{
	public sealed class Field
	{
		public String Name { get; set; }
		public Int32 Size { get; set; }
		public Int32 Decimal { get; set; }
		public FieldType Type { get; set; }
	}
}
