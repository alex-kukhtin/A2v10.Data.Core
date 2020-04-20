// Copyright © 2015-2019 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data.Interfaces
{
	public interface IExternalDataRecord
	{
		Object FieldValue(String name);
		Boolean FieldExists(String name);
		Boolean IsFieldEmpty(String name);
		Boolean IsEmpty { get; }
	}
}
