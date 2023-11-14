// Copyright © 2021-2023 Oleksandr Kukhtin. All rights reserved.

using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests
{
	[TestClass]
	[TestCategory("Data SimpleDbContext")]
	public class TestSimpleDbContext
	{
		static ServiceProvider GetServiceProvider(Action<IServiceCollection>? action = null)
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddUserSecrets<TestConfig>()
				.Build();
			var sc = new ServiceCollection();
			sc.AddSingleton<IConfiguration>(configuration);
			sc.UseSimpleDbContext();
			action?.Invoke(sc);
			return sc.BuildServiceProvider();
		}

		[TestMethod]
		public async Task SimpleDbConfiguration()
		{
			var sp = GetServiceProvider();
			var dbc = sp.GetService<IDbContext>();
			Assert.IsNotNull(dbc);
			var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => {
				return dbc.LoadModelAsync(null, "a2test.[SimpleModel.Load]");
			});

			Assert.AreEqual("The ConnectionString property has not been initialized.", ex.Message);
		}

		[TestMethod]
		public async Task SimpleDbConfigWithConfigure()
		{
			var inMemoryConfig = new Dictionary<String, String> {
				{"ConnectionStrings:MyConnectionString", "MyConnectionStringValue"},
			};
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(inMemoryConfig!)
				.Build();

			var sp = GetServiceProvider(sp =>
			{
				sp.Configure<DataConfigurationOptions>(opts =>
				{
					opts.ConnectionStringName = "Default";
				});
			});

			var dbc = sp.GetService<IDbContext>();
			Assert.IsNotNull(dbc);
			var dm = await dbc.LoadModelAsync(null, "a2test.[SimpleModel.Load]");
			var md = new MetadataTester(dm);
			md.IsAllKeys("TRoot,TModel");

			var dt = new DataTester(dm, "Model");
			dt.AreValueEqual(123, "Id");
		}
	}
}
