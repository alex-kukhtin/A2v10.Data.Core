// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data.Tests;
using A2v10.Data.Tests.Configuration;
using System.Threading.Tasks;

namespace A2v10.Data.Models;

[TestClass]
[TestCategory("Multiply Parent")]
public class MultiplyParent
{
	readonly IDbContext _dbContext;
	public MultiplyParent()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task LoadMultParent()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[MultParent.Load]");

		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TRepInfo,TItem");
		md.HasAllProperties("TRoot", "RepInfo");
		md.HasAllProperties("TRepInfo", "Id,Views,Grouping,Filters");
		md.IsId("TRepInfo", "Id");

		var dt = new DataTester(dm, "RepInfo");
		dt.AreValueEqual(1, "Id");

		dt = new DataTester(dm, "RepInfo.Views");
		dt.IsArray(1);
		dt.AreArrayValueEqual("View", 0, "Name");

		dt = new DataTester(dm, "RepInfo.Grouping");
		dt.IsArray(1);
		dt.AreArrayValueEqual("Grouping", 0, "Name");

		dt = new DataTester(dm, "RepInfo.Filters");
		dt.IsArray(1);
		dt.AreArrayValueEqual("Filter", 0, "Name");
	}


}
