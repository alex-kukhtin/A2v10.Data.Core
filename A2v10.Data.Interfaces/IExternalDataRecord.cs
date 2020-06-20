// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data.Interfaces
{
	public interface IExternalDataRecord
	{
		Object FieldValue(String name);
		Object FieldValue(Int32 index);
		Boolean FieldExists(String name);
		Boolean IsFieldEmpty(String name);
		Boolean IsEmpty { get; }
	}
}
