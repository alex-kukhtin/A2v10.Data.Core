﻿// Copyright © 2015-2018 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using A2v10.Data.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace A2v10.Data.Tests
{
	[TestClass]
	public class TestMetadata
	{
		readonly IDbContext _dbContext;
		public TestMetadata()
		{
			_dbContext = Starter.Create();
		}

		[TestMethod]
		public async Task MapObjects()
		{
			var dm = await _dbContext.LoadModelAsync(null, "a2test.[MapObjects.NoKey.Load]");
			var md = new MetadataTester(dm);
			md.HasAllProperties("TRoot", "Document,Categories");
			md.IsItemRefObject("TDocument", "Category", "TCategory", FieldType.Object);
			md.IsItemIsArrayLike("TRoot", "Categories");
			md.IsItemRefObject("TRoot", "Categories", "TCategory", FieldType.Map);
		}
	}
}
