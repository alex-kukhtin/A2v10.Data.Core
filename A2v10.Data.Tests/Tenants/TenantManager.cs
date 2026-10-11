// Copyright © 2015-2026 Oleksandr Kukhtin. All rights reserved.

using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Tenant Manager")]
public class TenantManagerTests
{
	readonly IDbContext _dbContext;
	public TenantManagerTests()
	{
		_dbContext = Starter.CreateWithTenants();
	}

	[TestMethod]
	public async Task LoadWithTenantIdAsync()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[TestTenant.Load]");

		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TElem");
		md.HasAllProperties("TRoot", "Elem");
		md.HasAllProperties("TElem", "TenantId");

		var dt = new DataTester(dm, "Elem");
		dt.AreValueEqual(123, "TenantId");
	}

	[TestMethod]
	public void LoadWithTenantId()
	{
		var dm = _dbContext.LoadModel(null, "a2test.[TestTenant.Load]");

		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TElem");
		md.HasAllProperties("TRoot", "Elem");
		md.HasAllProperties("TElem", "TenantId");

		var dt = new DataTester(dm, "Elem");
		dt.AreValueEqual(123, "TenantId");
	}

	// Max Pool Size=1: a connection leaked by the first failure makes the next call
	// time out waiting for the pool (InvalidOperationException) instead of SqlException
	static IDbContext CreateWithMissingTenant(String appName)
	{
		var baseConfig = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.AddUserSecrets<TestConfig>()
			.Build();
		var csb = new SqlConnectionStringBuilder(baseConfig.GetConnectionString("Default"))
		{
			MaxPoolSize = 1,
			ConnectTimeout = 3,
			ApplicationName = appName // own pool per test
		};
		var configuration = new ConfigurationBuilder()
			.AddConfiguration(baseConfig)
			.AddInMemoryCollection(new Dictionary<String, String?>() { ["ConnectionStrings:Default"] = csb.ConnectionString })
			.Build();
		Starter.Init();
		var config = new TestConfig(configuration);
		return new SqlDbContext(new TestProfiler(), config, new TestLocalizer(), new MetadataCache(config), new MissingTenantManager());
	}

	[TestMethod]
	public void TenantFailureReleasesConnection()
	{
		var dbContext = CreateWithMissingTenant("A2v10.Data.Tests.TenantFailure");
		for (Int32 i = 0; i < 3; i++)
			Assert.ThrowsExactly<SqlException>(() => dbContext.LoadModel(null, "a2test.[TestTenant.Load]"));
	}

	[TestMethod]
	public async Task TenantFailureReleasesConnectionAsync()
	{
		var dbContext = CreateWithMissingTenant("A2v10.Data.Tests.TenantFailureAsync");
		for (Int32 i = 0; i < 3; i++)
			await Assert.ThrowsExactlyAsync<SqlException>(() => dbContext.LoadModelAsync(null, "a2test.[TestTenant.Load]"));
	}
}
