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
	public class TestMetadata
	{
		IDbContext _dbContext;
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
