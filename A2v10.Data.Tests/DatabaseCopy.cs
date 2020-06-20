// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Data.Interfaces;
using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests
{
	[TestClass]
	public class DatabaseCopy
	{
		IDbContext _dbContext;
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
}
