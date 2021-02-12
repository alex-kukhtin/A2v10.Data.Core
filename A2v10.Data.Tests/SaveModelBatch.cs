// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using A2v10.Data.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Threading.Tasks;

namespace A2v10.Data.Tests
{
	[TestClass]
	[TestCategory("Save Model batch")]
	public class SaveModelBatch
	{
		readonly IDbContext _dbContext;

		const String jsonData = 
@"{
	Document: {
		Id : 45,
		Name: 'MainDocument',
		Memo : 'MainMemo',
		Rows : [{
			Id: 55,
			Name: 'Row1',
			Qty: 5.0
		},
		{
			Id: 66,
			Name: 'Row2',
			Qty: 12.0
		}]
	}
}
";

		public SaveModelBatch()
		{
			_dbContext = Starter.Create();
		}

		[TestMethod]
		public async Task BatchModel()
		{
			// DATA with ROOT
			IDataModel dm = null;
			var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData, new ExpandoObjectConverter());

			var batches = new List<BatchProcedure>();
			batches.Add(new BatchProcedure("a2test.[Batch.Proc1]", new ExpandoObject()
			{
				{"Id", 55 },
				{ "Delta", 5.0}
			}));

			batches.Add(new BatchProcedure("a2test.[Batch.Proc1]", new ExpandoObject()
			{
				{"Id", 66 },
				{"Delta", 12.0}
			}));

			var prms = new ExpandoObject()
			{
				{"UserId", 100 }
			};

			dm = await _dbContext.SaveModelBatchAsync(null, "a2test.[BatchModel.Update]", dataToSave, prms, batches);

			var docT = new DataTester(dm, "Document");
			docT.AreValueEqual(45L, "Id");
			docT.AreValueEqual("MainDocument", "Name");
			docT.AreValueEqual("MainMemo", "Memo");

			var rowT0 = new DataTester(dm, "Document.Rows[0]");
			rowT0.AreValueEqual(55L, "Id");
			rowT0.AreValueEqual(5.0, "Qty");

			var rowT1 = new DataTester(dm, "Document.Rows[1]");
			rowT1.AreValueEqual(66L, "Id");
			rowT1.AreValueEqual(12.0, "Qty");


			dm = await _dbContext.LoadModelAsync(null, "a2test.[BatchModel.Load]", new { Id = 45 });

			docT = new DataTester(dm, "Document");
			docT.AreValueEqual(45L, "Id");
			docT.AreValueEqual("MainDocument", "Name");
			docT.AreValueEqual("MainMemo", "Memo");

			rowT0 = new DataTester(dm, "Document.Rows[0]");
			rowT0.AreValueEqual(55L, "Id");
			rowT0.AreValueEqual(10.0, "Qty");

			rowT1 = new DataTester(dm, "Document.Rows[1]");
			rowT1.AreValueEqual(66L, "Id");
			rowT1.AreValueEqual(24.0, "Qty");
		}


		[TestMethod]
		public async Task BatchModelRollback()
		{
			// DATA with ROOT
			IDataModel dm = null;
			var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData, new ExpandoObjectConverter());

			// initial value
			dm = await _dbContext.SaveModelAsync(null, "a2test.[BatchModel.Update]", dataToSave);
			var rowT0 = new DataTester(dm, "Document.Rows[0]");
			rowT0.AreValueEqual(55L, "Id");
			rowT0.AreValueEqual(5.0, "Qty");


			var batches = new List<BatchProcedure>();
			batches.Add(new BatchProcedure("a2test.[Batch.Proc1]", new ExpandoObject()
			{
				{"Id", 55 },
				{ "Delta", 5.0}
			}));

			batches.Add(new BatchProcedure("a2test.[Batch.Throw]", new ExpandoObject()
			{
				{"Id", 5511223 }
			}));

			await TestExtensions.ThrowsAsync<SqlException>(async () =>
			{
				dm = await _dbContext.SaveModelBatchAsync(null, "a2test.[BatchModel.Update]", dataToSave, null, batches);
			});

			dm = await _dbContext.LoadModelAsync(null, "a2test.[BatchModel.Load]", new { Id = 45 });

			var docT = new DataTester(dm, "Document");
			docT.AreValueEqual(45L, "Id");
			docT.AreValueEqual("MainDocument", "Name");
			docT.AreValueEqual("MainMemo", "Memo");

			rowT0 = new DataTester(dm, "Document.Rows[0]");
			rowT0.AreValueEqual(55L, "Id");
			rowT0.AreValueEqual(5.0, "Qty");
		}
	}
}
