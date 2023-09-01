// Copyright © 2019-2023 Oleksandr Kukhtin. All rights reserved.

using System.Threading.Tasks;

using Newtonsoft.Json;

using A2v10.Data.Tests.Configuration;
using System.Dynamic;
using A2v10.Data.Tests;

namespace A2v10.Data.Models;

[TestClass]
[TestCategory("Collection Filters")]
public class CollectionFilters
{
	private readonly IDbContext _dbContext;
	public CollectionFilters()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task SimpleFilters()
	{
		var today = DateTime.Today;
		var prms = new ExpandoObject()
		{
			{ "Date", today },
		};


		var dm = await _dbContext.LoadModelAsync(null, "a2test.[Filters.Load]", prms);

        var dt = new DataTester(dm, "$ModelInfo.Documents");

		dt.AllProperties("Offset,PageSize,SortOrder,SortDir,Filter");
		dt.AreValueEqual(0, "Offset");
        dt.AreValueEqual(20, "PageSize");
        dt.AreValueEqual("name", "SortOrder");
        dt.AreValueEqual("asc", "SortDir");

        var ft = new DataTester(dm, "$ModelInfo.Documents.Filter");
        ft.AllProperties("Period,Agent,Company,Fragment");
		ft.AreValueEqual("FRAGMENT", "Fragment");

		var fromDate = dm.Eval<String>("$ModelInfo.Documents.Filter.Period.From")
			?? throw new InvalidOperationException("Period from is null");	
		var resDate = DateTime.Parse(fromDate.Replace("\"\\/\"", "")).ToUniversalTime();
		Assert.AreEqual(resDate, today);

        var agent = dm.Eval<ExpandoObject>("$ModelInfo.Documents.Filter.Agent")
            ?? throw new InvalidOperationException("Agent is null");

        Assert.AreEqual(15, agent.Get<Int32>("Id"));
        Assert.AreEqual("AgentName", agent.Get<String>("Name"));

        var company = dm.Eval<ExpandoObject>("$ModelInfo.Documents.Filter.Company")
            ?? throw new InvalidOperationException("Company is null");

		Assert.AreEqual(127, company.Get<Int32>("Id"));
        Assert.AreEqual("Company 127", company.Get<String>("Name"));
	}
}
