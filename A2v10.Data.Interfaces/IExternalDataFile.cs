// Copyright © 2015-2020 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Data.Interfaces;
public interface IExternalDataFile
{
	Int32 FieldCount { get; }
	Int32 NumRecords { get; }
	String FieldName(Int32 index);

	IEnumerable<IExternalDataRecord> Records { get; }
}

