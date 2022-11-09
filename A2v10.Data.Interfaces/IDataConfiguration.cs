// Copyright © 2015-2022 Alex Kukhtin. All rights reserved.

namespace A2v10.Data.Interfaces;

public interface IDataConfiguration
{
	String? ConnectionString(String? source);
	TimeSpan CommandTimeout { get; }	
	Boolean IsWriteMetadataCacheEnabled { get; }
}

