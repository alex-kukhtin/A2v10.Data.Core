// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using Microsoft.Extensions.Configuration;

namespace A2v10.Data.Tests.Configuration;

public class TestConfig : IDataConfiguration
{

	private readonly IConfiguration _config;

	public TestConfig(IConfiguration config)
	{
		_config = config;
	}

	#region IDataConfiguration
	public String? ConnectionString(String? source)
	{
		if (String.IsNullOrEmpty(source))
			source = "Default";
		return _config.GetConnectionString(source);
	}

	public TimeSpan CommandTimeout => _config.GetValue<TimeSpan>("A2v10:Data:CommandTimeout");

	public Boolean IsWriteMetadataCacheEnabled => true;
	public Boolean AllowEmptyStrings => true;
	#endregion;
}
