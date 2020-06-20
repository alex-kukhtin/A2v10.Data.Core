// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Data.Interfaces
{
	public interface IExternalDataFile
	{
		Int32 FieldCount { get; }
		Int32 NumRecords { get; }
		String FieldName(Int32 index);

		IEnumerable<IExternalDataRecord> Records { get; }
	}
}
