﻿// Copyright © 2015-2021 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using A2v10.Data.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Aliases")]
public class DatabaseAliases
{
	private readonly IDbContext _dbContext;

	public DatabaseAliases()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task Aliases()
	{
		Int64 docId = 10;
		ExpandoObject prms = new()
		{
			{ "UserId", 100 },
			{ "Id", docId }
		};
		IDataModel dm = await _dbContext.LoadModelAsync(null, "a2test.[Document.Aliases]", prms);
		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TDocument,TRow,TEntity");
		md.HasAllProperties("TRoot", "Document");
		md.HasAllProperties("TDocument", "Id,Rows");
		md.HasAllProperties("TRow", "Id,Entity");
		md.HasAllProperties("TEntity", "Id,Name");
		var dt = new DataTester(dm, "Document");
		dt.AreValueEqual(docId, "Id");
		dt = new DataTester(dm, "Document.Rows");
		dt.IsArray(1);
		dt.AreArrayValueEqual(59, 0, "Id");
	}
}
