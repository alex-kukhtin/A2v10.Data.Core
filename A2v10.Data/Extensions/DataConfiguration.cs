﻿// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;
public class DataConfigurationOptions
{
	public String ConnectionStringName { get; set; } = "DefaultConnection";
	public TimeSpan DefaultCommandTimeout { get; set; } = TimeSpan.FromSeconds(30);
	public Boolean DisableWriteMetadataCaching { get; set; } = false;
}

public class DataConfiguration : IDataConfiguration
{
	private readonly IConfiguration _config;
	private readonly DataConfigurationOptions _options;


	public DataConfiguration(IConfiguration config, IOptions<DataConfigurationOptions> options)
	{
		_config = config;
		_options = options.Value;
	}

	public String ConnectionStringName => _options.ConnectionStringName;

	#region IDataConfiguration

	public TimeSpan CommandTimeout => _options.DefaultCommandTimeout;

	public String ConnectionString(String? source)
	{
		if (String.IsNullOrEmpty(source))
			source = _options.ConnectionStringName;
		return _config.GetConnectionString(source);
	}

	public Boolean IsWriteMetadataCacheEnabled => !_options.DisableWriteMetadataCaching;
	#endregion
}

