// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;
public static class DataCoreDependencyInjection
{
	public static IServiceCollection UseSimpleDbContext(this IServiceCollection services)
	{
		services.AddOptions<DataConfigurationOptions>();
		services.AddSingleton<IDataConfiguration, DataConfiguration>()
		.AddSingleton<MetadataCache>()
		.AddSingleton<IDataProfiler, NullDataProfiler>()
		.AddSingleton<IDataLocalizer, NullDataLocalizer>()
		.AddSingleton<IDbContext, SqlDbContext>();
		return services;
	}


	public static IServiceCollection ConfigureDbContext(this IServiceCollection services, String DefaultConnectioString, IConfiguration config)
	{
		var sect = new DataConfigurationSection();
		config.GetSection("A2v10:Data").Bind(sect);
		services.Configure<DataConfigurationOptions>(options =>
		{
			options.ConnectionStringName = DefaultConnectioString;
			options.DisableWriteMetadataCaching = !sect.MetadataCache;
			options.DefaultCommandTimeout = sect.CommandTimeout;
			options.CatalogAsDefault = sect.CatalogAsDefault;
		});
		return services;
	}
}

