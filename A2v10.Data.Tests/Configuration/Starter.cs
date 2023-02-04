// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace A2v10.Data.Tests.Configuration;


public static class Starter
{
	public static void Init()
	{
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
	}

	public static IServiceProvider BuildServices(DataConfigurationOptions? config)
	{
		Init();
		var configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.AddUserSecrets<TestConfig>()
			.Build();

		var sc = new ServiceCollection();

		sc.AddOptions<DataConfigurationOptions>();

		sc.AddSingleton<IConfiguration>(configuration)
		.AddSingleton<IDataProfiler, TestProfiler>()
		.AddSingleton<IDataConfiguration, DataConfiguration>()
		.AddSingleton<IDataLocalizer, TestLocalizer>()
		.AddSingleton<IDbContext, SqlDbContext>()
		.AddSingleton<MetadataCache>();

		sc.Configure<DataConfigurationOptions>(opts =>
		{
			opts.ConnectionStringName = "Default";
			if (config != null)
			{
				opts.AllowEmptyStrings = config.AllowEmptyStrings;
				opts.DisableWriteMetadataCaching = config.DisableWriteMetadataCaching;
			}
		});

		return sc.BuildServiceProvider();
	}

	public static IDbContext Create(DataConfigurationOptions? options = null)
	{
		var svc = BuildServices(options);
		return svc.GetService<IDbContext>() ?? throw new InvalidProgramException("IDbContext not found");
	}

	public static IDbContext CreateWithTenants()
	{
		var configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.AddUserSecrets<TestConfig>()
			.Build();

		Init();
		IDataProfiler profiler = new TestProfiler();
		IDataConfiguration config = new TestConfig(configuration);
		IDataLocalizer localizer = new TestLocalizer();
		ITenantManager tenantManager = new TestTenantManager();
		MetadataCache metadataCache = new (config);
		return new SqlDbContext(profiler, config, localizer, metadataCache, tenantManager);
	}
}
