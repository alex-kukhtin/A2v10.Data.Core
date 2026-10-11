// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

using System.Threading.Tasks;

using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests;

// a {{key}} source is resolved from ExpandoObject parameters only; both refusals happen before connecting
[TestClass]
[TestCategory("Resolve source")]
public class ResolveSourceTests
{
	readonly IDbContext _dbContext;
	public ResolveSourceTests()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task TemplateSourceWithActionParams()
	{
		var ex = await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
			_dbContext.LoadModelSqlAsync("{{x}}", "select 1", prms => { }));
		Assert.Contains("{{x}}", ex.Message);
	}

	[TestMethod]
	public async Task TemplateSourceWithObjectParams()
	{
		var ex = await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
			_dbContext.LoadModelSqlAsync("{{x}}", "select 1", new { x = "Default" }));
		Assert.Contains("{{x}}", ex.Message);
	}
}
