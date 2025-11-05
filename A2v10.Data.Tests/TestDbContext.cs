// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using Microsoft.Extensions.Configuration;

using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("DbContext")]
public class TestDbContext
{

    [TestMethod]
    public void DbConnection()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<TestConfig>()
            .Build();

        IDataProfiler profiler = new TestProfiler();
        IDataConfiguration config = new TestConfig(configuration);
        IDataLocalizer localizer = new TestLocalizer();
        ITenantManager tenantManager = new TestTenantManager();
        MetadataCache metadataCache = new(config);
        var sqlDbContext = new SqlDbContext(profiler, config, localizer, metadataCache, tenantManager);
        Assert.IsNotNull(sqlDbContext);
        var dbConnection = sqlDbContext.GetDbConnection(null);
        Assert.IsNotNull(dbConnection);
        dbConnection = sqlDbContext.GetDbConnection("Default");
        Assert.IsNotNull(dbConnection);
    }

    [TestMethod]
	public void Exceptions()
	{
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<TestConfig>()
            .Build();

        IDataProfiler profiler = new TestProfiler();
        IDataConfiguration config = new TestConfig(configuration);
        IDataLocalizer localizer = new TestLocalizer();
        ITenantManager tenantManager = new TestTenantManager();
        MetadataCache metadataCache = new(config);
        var sqlDbContext = new SqlDbContext(profiler, config, localizer, metadataCache, tenantManager);
        Assert.IsNotNull(sqlDbContext);
        sqlDbContext = new SqlDbContext(profiler, config, localizer, metadataCache);
        Assert.IsNotNull(sqlDbContext);

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            var ctx = new SqlDbContext(null!, config, localizer, metadataCache, tenantManager);

        }, "Value cannot be null. (Parameter 'profiler')");

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            var ctx = new SqlDbContext(profiler, null!, localizer, metadataCache, tenantManager);

        }, "Value cannot be null. (Parameter 'config')");

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            var ctx = new SqlDbContext(profiler, config, null!, metadataCache, tenantManager);

        }, "Value cannot be null. (Parameter 'localizer')");

        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            var ctx = new SqlDbContext(profiler, config, localizer, null!, tenantManager);

        }, "Value cannot be null. (Parameter 'metadataCache')");
    }
}
