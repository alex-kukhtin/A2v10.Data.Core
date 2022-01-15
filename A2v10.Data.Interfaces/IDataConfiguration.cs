// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

namespace A2v10.Data.Interfaces;

public interface IDataConfiguration
{
	String ConnectionString(String? source);
	TimeSpan CommandTimeout { get; }	
	Boolean IsWriteMetadataCacheEnabled { get; }
}

