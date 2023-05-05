// Copyright © 2015-2023 Alex Kukhtin. All rights reserved.

using System.Dynamic;
using System.Threading.Tasks;

using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Data Types")]
public class DatabaseDataTypes
{
	readonly IDbContext _dbContext;
	public DatabaseDataTypes()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task ScalarDataTypes()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[TypesModel.Load]");

		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TModel");
		md.HasAllProperties("TRoot", "Model");
		md.HasAllProperties("TModel", "Name,Id,Decimal,Int,BigInt,Short,Tiny,Float,Date,DateTime");
		md.IsId("TModel", "Id");
		md.IsName("TModel", "Name");

		var dt = new DataTester(dm, "Model");
		dt.AreValueEqual(123, "Id");
		dt.AreValueEqual("ObjectName", "Name");
		dt.AreValueEqual(55.1234M, "Decimal");
		dt.AreValueEqual(32, "Int");
		dt.AreValueEqual(77223344L, "BigInt");
		dt.AreValueEqual((Int16)27823, "Short");
		dt.AreValueEqual((Byte)255, "Tiny");
		dt.AreValueEqual(77.6633, "Float");
		dt.AreValueEqual(new DateTime(2018, 02, 19), "Date");
		dt.AreValueEqual(new DateTime(2018, 02, 19, 15, 10, 20), "DateTime");
	}

	[TestMethod]
	public async Task ParamsTypes()
	{

		var prms = new ExpandoObject()
		{
			{"Date", "20230420" },
			{"Date2", "2023-04-22T12:37:20.000" }
		};
		var res = await _dbContext.ReadExpandoAsync(null, "a2test.[TypesModel.Params]", prms)
			?? throw new InvalidOperationException("Model is null");

		Assert.AreEqual("20230420", res.Get<DateTime>("Date").ToString("yyyyMMdd"));
		Assert.AreEqual("22.04.2023 12:37:20", res.Get<DateTime>("Date2").ToString());
	}
}

