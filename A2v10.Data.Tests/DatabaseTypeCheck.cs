﻿// Copyright © 2015-2018 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data.Tests.Configuration;
using System.Threading.Tasks;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("TypeCheck")]
public class DatabaseTypeCheck
{
	readonly IDbContext _dbContext;
	public DatabaseTypeCheck()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task MultipleTypes()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[MultipleTypes.Load]");

		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TModel,TAgent");
		md.HasAllProperties("TRoot", "Model");
		md.HasAllProperties("TModel", "Id,Agent1,Agent2");
		md.HasAllProperties("TAgent", "Id,Name,Memo");
		md.IsId("TModel", "Id");
		md.IsId("TAgent", "Id");

		var dt = new DataTester(dm, "Model.Agent1");
		dt.AreValueEqual(5, "Id");
		dt.AreValueEqual("Five", "Name");

		dt = new DataTester(dm, "Model.Agent2");
		dt.AreValueEqual(7, "Id");
		dt.AreValueEqual("Seven", "Name");
		dt.AreValueEqual("Memo for seven", "Memo");
	}
}
