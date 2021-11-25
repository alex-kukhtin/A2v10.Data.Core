// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using A2v10.Data.Interfaces;

namespace A2v10.Data.Tests.Configuration
{

	public static class Starter
	{
		public static void Init()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		public static IServiceProvider BuildServices()
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
			.AddSingleton<IDbContext, SqlDbContext>();

			sc.Configure<DataConfigurationOptions>(opts =>
			{
				opts.ConnectionStringName = "Default";
			});

			return sc.BuildServiceProvider();
		}

		public static IDbContext Create()
		{
			var svc = BuildServices();
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
			return new SqlDbContext(profiler, config, localizer, tenantManager);
		}
	}
}
