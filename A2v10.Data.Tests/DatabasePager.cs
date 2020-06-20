// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;
using A2v10.Data.Interfaces;
using A2v10.Data.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.Data.Tests
{
	[TestClass]
	public class DatabasePager
	{
		IDbContext _dbContext;
		public DatabasePager()
		{
			_dbContext = Starter.Create();
		}

		[TestMethod]
		public async Task SimplePager()
		{
			var dm = await _dbContext.LoadModelAsync(null, "a2test.[PagerModel.Load]");

			var md = new MetadataTester(dm);
			md.IsAllKeys("TRoot,TElem");
			md.HasAllProperties("TRoot", "Elems");
			md.HasAllProperties("TElem", "Name,Id");
			md.IsId("TElem", "Id");
			md.IsName("TElem", "Name");

			var dt = new DataTester(dm, "Elems");
			dt.IsArray(1);

			dt = new DataTester(dm, "$ModelInfo.Elems");
			dt.AreValueEqual(20, "PageSize");
			dt.AreValueEqual("asc", "SortDir");
			dt.AreValueEqual("Name", "SortOrder");
		}
	}
}
