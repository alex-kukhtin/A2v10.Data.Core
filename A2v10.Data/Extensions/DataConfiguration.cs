﻿// Copyright © 2020-2024 Oleksandr Kukhtin. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public class DataConfigurationSection
{
	public Boolean MetadataCache { get; set; } = true;
	public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);
	public Boolean CatalogAsDefault { get; set; }
}

public class DataConfigurationOptions
{
	public String ConnectionStringName { get; set; } = "DefaultConnection";
	public TimeSpan DefaultCommandTimeout { get; set; } = TimeSpan.FromSeconds(30);
	public Boolean DisableWriteMetadataCaching { get; set; } = false;
	public Boolean AllowEmptyStrings { get; set; } = false;
    public Boolean CatalogAsDefault { get; set; } = false;
}

public class DataConfiguration(IConfiguration config, IOptions<DataConfigurationOptions> options) : IDataConfiguration
{
	private readonly IConfiguration _config = config;
	private readonly DataConfigurationOptions _options = options.Value;

    public String ConnectionStringName => _options.ConnectionStringName;

	#region IDataConfiguration

	public TimeSpan CommandTimeout => _options.DefaultCommandTimeout;

	public String? ConnectionString(String? source)
	{
		if (String.IsNullOrEmpty(source))
			source = _options.ConnectionStringName;
		if (_options.CatalogAsDefault && source == "Catalog")
			source = _options.ConnectionStringName;
		return _config.GetConnectionString(source);
	}

	public Boolean IsWriteMetadataCacheEnabled => !_options.DisableWriteMetadataCaching;
	public Boolean AllowEmptyStrings => _options.AllowEmptyStrings;
	#endregion
}

