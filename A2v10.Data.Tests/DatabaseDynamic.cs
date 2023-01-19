// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data.Tests.Configuration;
using System.Threading.Tasks;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Dynamic Types")]
public class DatabaseDynamic
{
	readonly IDbContext _dbContext;
	public DatabaseDynamic()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task SimpleModel()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[SimpleModel.Load]");

		var dyn = dm.GetDynamic();
		var dynmodel = dyn["Model"];

		Assert.IsNotNull(dynmodel);
		Assert.AreEqual(123, dynmodel.Id);
		Assert.AreEqual("ObjectName", dynmodel.Name);
		Assert.AreEqual(55.1234M, dynmodel.Decimal);

	}
}
