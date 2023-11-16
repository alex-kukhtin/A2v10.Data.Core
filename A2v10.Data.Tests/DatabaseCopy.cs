// Copyright © 2015-2018 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data.Tests.Configuration;
using System.Threading.Tasks;

namespace A2v10.Data.Tests;

[TestCategory("Copy models")]
[TestClass]
public class DatabaseCopy
{
	readonly IDbContext _dbContext;

	public DatabaseCopy()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task CopyComplexModel()
	{
		IDataModel dm = await _dbContext.LoadModelAsync(null, "a2test.ComplexModel");
		dm.MakeCopy();
	}
}
