// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using A2v10.Data.Interfaces;
using A2v10.Data.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.Data.Tests
{
	[TestClass]
	public class DatabaseLocalization
	{
		IDbContext _dbContext;

		public DatabaseLocalization()
		{
			_dbContext = Starter.Create();
		}

		[TestMethod]
		public async Task LocalizeSimpleModel()
		{
			var dm = await _dbContext.LoadModelAsync(null, "a2test.[SimpleModel.Localization.Load]");

			var md = new MetadataTester(dm);
			md.IsAllKeys("TRoot,TModel");
			md.HasAllProperties("TRoot", "Model");
			md.HasAllProperties("TModel", "Name,Id");
			md.IsId("TModel", "Id");
			md.IsName("TModel", "Name");

			var dt = new DataTester(dm, "Model");
			dt.AreValueEqual(234, "Id");
			dt.AreValueEqual("Item 1", "Name");
		}

		[TestMethod]
		public async Task LocalizeComplexObjects()
		{
			IDataModel dm = await _dbContext.LoadModelAsync(null, "a2test.[ComplexObject.Localization.Load]");
			var md = new MetadataTester(dm);
			md.IsAllKeys("TRoot,TDocument,TAgent");
			md.IsItemType("TRoot", "Document", FieldType.Object);

			md.IsId("TDocument", "Id");
			md.IsType("TDocument", "Id", DataType.Number);
			md.IsItemType("TDocument", "Agent", FieldType.Object);

			var dt = new DataTester(dm, "Document");
			dt.AreValueEqual(200, "Id");

			dt = new DataTester(dm, "Document.Agent");
			dt.AreValueEqual(300, "Id");
			dt.AreValueEqual("Item 2", "Name");
		}

	}
}
