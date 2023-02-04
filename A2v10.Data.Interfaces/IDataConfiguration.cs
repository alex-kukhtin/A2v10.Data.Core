// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data.Interfaces;

public interface IDataConfiguration
{
	String? ConnectionString(String? source);
	TimeSpan CommandTimeout { get; }	
	Boolean IsWriteMetadataCacheEnabled { get; }
	Boolean AllowEmptyStrings { get; }
}

