// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Data.Interfaces;
using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests
{
	[TestClass]
	public class DatabaseLoaderSync
	{
		IDbContext _dbContext;
		public DatabaseLoaderSync()
		{
			_dbContext = Starter.Create();
		}

		[TestMethod]
		public void LoadSimpleModelSync()
		{
			var dm = _dbContext.LoadModel(null, "a2test.[SimpleModel.Load]");

			var md = new MetadataTester(dm);
			md.IsAllKeys("TRoot,TModel");
			md.HasAllProperties("TRoot", "Model");
			md.HasAllProperties("TModel", "Name,Id,Decimal");
			md.IsId("TModel", "Id");
			md.IsName("TModel", "Name");

			var dt = new DataTester(dm, "Model");
			dt.AreValueEqual(123, "Id");
			dt.AreValueEqual("ObjectName", "Name");
			dt.AreValueEqual(55.1234M, "Decimal");
		}
	}
}
