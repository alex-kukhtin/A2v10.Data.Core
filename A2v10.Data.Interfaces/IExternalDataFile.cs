// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

namespace A2v10.Data.Interfaces
{
	public interface IExternalDataFile
	{
		Int32 FieldCount { get; }
		Int32 NumRecords { get; }

		IEnumerable<IExternalDataRecord> Records { get; }
	}
}
