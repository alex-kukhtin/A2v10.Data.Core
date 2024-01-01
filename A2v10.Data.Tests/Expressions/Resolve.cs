// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using System.Threading.Tasks;

using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Resolve")]
public class TestResolve
{

	readonly IDbContext _dbContext;
	public TestResolve()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task DataModelExpressions()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[SimpleModel.Load]");

		var result = dm.CalcExpression<Int32>("Model.Id");
		Assert.AreEqual(123, result);

		result = dm.CalcExpression<Int32>("Root.Model.Id + 55");
		Assert.AreEqual(123 + 55, result);

		var objResult = dm.CalcExpression<Object>("77 + Root.Model.Id");
		Assert.AreEqual(123M + 77M, objResult);

		var decResult = dm.CalcExpression<Decimal>("Model.Decimal");
		Assert.AreEqual(55.1234M, decResult);

		objResult = dm.CalcExpression<Object>("Model.Name + ' 1'");
		Assert.AreEqual("ObjectName 1", objResult);

		objResult = dm.CalcExpression<Object>("Model.Name + ` @_$ 5`");
		Assert.AreEqual("ObjectName @_$ 5", objResult);
	}

	[TestMethod]
	public async Task DataModelResolve()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[ComplexModel]");

		var result = dm.Resolve("Id = {{Document.Id}}");
		Assert.AreEqual("Id = 123", result);

		result = dm.Resolve("{{Document.Id}}_{{Document.Agent.Id}}");
		Assert.AreEqual("123_512", result);

		result = dm.Resolve("{{Document.Id}}_{{Document.Rows1[0].Qty}}");
		Assert.AreEqual("123_4", result);
	}
}
