// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Data.Interfaces;
using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests
{
	[TestClass]
	public class TenantManager
	{
		readonly IDbContext _dbContext;
		public TenantManager()
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
}
