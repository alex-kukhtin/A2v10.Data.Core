// Copyright © 2021-2022 Alex Kukhtin. All rights reserved.


using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Data Configuration")]
public class TestDataConfiguration
{
	static IServiceProvider GetServiceProvider(IConfiguration configuration, Action<IServiceCollection>? action = null)
	{
		var sc = new ServiceCollection();
		sc.AddSingleton<IConfiguration>(configuration);
		sc.AddOptions<DataConfigurationOptions>();
		sc.AddSingleton<IDataConfiguration, DataConfiguration>();
		action?.Invoke(sc);
		return sc.BuildServiceProvider();
	}

	[TestMethod]
	public void DefaultConfig()
	{
		var inMemoryConfig = new Dictionary<String, String> {
			{"ConnectionStrings:DefaultConnection", "DefaultConnectionStringValue"}
		};
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(inMemoryConfig!)
			.Build();
		var sp = GetServiceProvider(configuration);

		var dc = sp.GetService<IDataConfiguration>();
		Assert.IsNotNull(dc);
		Assert.AreEqual("DefaultConnectionStringValue", dc.ConnectionString(null));
		Assert.AreEqual(30, dc.CommandTimeout.TotalSeconds);
		Assert.IsTrue(dc.IsWriteMetadataCacheEnabled);
	}

	[TestMethod]
	public void ConfigWithOptions()
	{
		var inMemoryConfig = new Dictionary<string, string> {
			{"ConnectionStrings:MyConnectionString", "MyConnectionStringValue"},
			{"A2v10:Data:CommandTimeout", "00:01:20" },
			{"A2v10:Data:MetadataCache", "false" },
		};
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(inMemoryConfig!)
			.Build();

		var sp = GetServiceProvider(configuration, sp =>
		{
			sp.Configure<DataConfigurationOptions>(opts =>
			{
				opts.ConnectionStringName = "MyConnectionString";
				opts.DefaultCommandTimeout = configuration.GetValue<TimeSpan>("A2v10:Data:CommandTimeout");
				opts.DisableWriteMetadataCaching = !configuration.GetValue<Boolean>("A2v10:Data:MetadataCache");
			});
		});

		var dc = sp.GetService<IDataConfiguration>();
		Assert.IsNotNull(dc);
		Assert.AreEqual("MyConnectionStringValue", dc.ConnectionString(null));
		Assert.AreEqual(60 + 20, dc.CommandTimeout.TotalSeconds);
		Assert.IsFalse(dc.IsWriteMetadataCacheEnabled);
	}
}

