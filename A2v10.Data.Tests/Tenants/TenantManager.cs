// Copyright © 2015-2022 Oleksandr Kukhtin. All rights reserved.

using System.Threading.Tasks;

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
}
