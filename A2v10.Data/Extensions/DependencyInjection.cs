// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using A2v10.Data;

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
}

