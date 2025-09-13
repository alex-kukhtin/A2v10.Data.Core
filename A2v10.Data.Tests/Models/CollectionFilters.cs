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
        ft.AllProperties("Period,Agent,Company,Fragment,Warehouse,NullString");
		ft.AreValueEqual("FRAGMENT", "Fragment");
        ft.IsNull("NullString");

        var fromDate = dm.Eval<String>("$ModelInfo.Documents.Filter.Period.From")
			?? throw new InvalidOperationException("Period from is null");	
		var resDate = DateTime.Parse(fromDate.Replace("\"\\/\"", ""));
		Assert.AreEqual(resDate, today);

        var agent = dm.Eval<ExpandoObject>("$ModelInfo.Documents.Filter.Agent")
            ?? throw new InvalidOperationException("Agent is null");

        Assert.AreEqual(15, agent.Get<Int32>("Id"));
        Assert.AreEqual("AgentName", agent.Get<String>("Name"));

        var company = dm.Eval<ExpandoObject>("$ModelInfo.Documents.Filter.Company")
            ?? throw new InvalidOperationException("Company is null");

		Assert.AreEqual(127, company.Get<Int32>("Id"));
        Assert.AreEqual("Company 127", company.Get<String>("Name"));

        var wh = dm.Eval<ExpandoObject>("$ModelInfo.Documents.Filter.Warehouse")
            ?? throw new InvalidOperationException("Warehouse is null");
		Assert.IsNull(wh.Get<Object>("Id"));
        Assert.IsNull(wh.Get<Object>("Name"));
    }

    [TestMethod]
    public async Task ArrayFilters()
    {
        var today = DateTime.Today;
        var prms = new ExpandoObject()
        {
            { "Date", today },
        };


        var dm = await _dbContext.LoadModelAsync(null, "a2test.[FiltersArray.Load]", prms);

        var dt = new DataTester(dm, "$ModelInfo.Documents");

        dt.AllProperties("Offset,PageSize,SortOrder,SortDir,Filter");
        dt.AreValueEqual(0, "Offset");
        dt.AreValueEqual(20, "PageSize");
        dt.AreValueEqual("name", "SortOrder");
        dt.AreValueEqual("asc", "SortDir");

        var ft = new DataTester(dm, "$ModelInfo.Documents.Filter");
        ft.AllProperties("Period,Agents,Company,Fragment,Warehouse");
        ft.AreValueEqual("FRAGMENT", "Fragment");

        var fromDate = dm.Eval<String>("$ModelInfo.Documents.Filter.Period.From")
            ?? throw new InvalidOperationException("Period from is null");
        var resDate = DateTime.Parse(fromDate.Replace("\"\\/\"", ""));
        Assert.AreEqual(resDate, today);

        var agents = dm.Eval<List<ExpandoObject>>("$ModelInfo.Documents.Filter.Agents")
            ?? throw new InvalidOperationException("Agent is null");
        Assert.AreEqual(3, agents.Count);

        Assert.AreEqual(15L, agents[0].Get<Int64>("Id"));
        Assert.AreEqual("Agent 15", agents[0].Get<String>("Name"));

        Assert.AreEqual(20L, agents[1].Get<Int64>("Id"));
        Assert.AreEqual("Agent 20", agents[1].Get<String>("Name"));

        Assert.AreEqual(25L, agents[2].Get<Int64>("Id"));
        Assert.AreEqual("Agent 25", agents[2].Get<String>("Name"));

        var company = dm.Eval<ExpandoObject>("$ModelInfo.Documents.Filter.Company")
            ?? throw new InvalidOperationException("Company is null");

        Assert.AreEqual(127, company.Get<Int32>("Id"));
        Assert.AreEqual("Company 127", company.Get<String>("Name"));

        var wh = dm.Eval<ExpandoObject>("$ModelInfo.Documents.Filter.Warehouse")
            ?? throw new InvalidOperationException("Warehouse is null");
        Assert.IsNull(wh.Get<Object>("Id"));
        Assert.IsNull(wh.Get<Object>("Name"));
    }

    [TestMethod]
    public async Task ArrayFiltersNull()
    {
        var today = DateTime.Today;
        var prms = new ExpandoObject()
        {
            { "Date", today },
        };


        var dm = await _dbContext.LoadModelAsync(null, "a2test.[FiltersArrayNull.Load]", prms);

        var dt = new DataTester(dm, "$ModelInfo.Documents");

        dt.AllProperties("Offset,PageSize,SortOrder,SortDir,Filter");
        dt.AreValueEqual(0, "Offset");
        dt.AreValueEqual(20, "PageSize");
        dt.AreValueEqual("name", "SortOrder");
        dt.AreValueEqual("asc", "SortDir");

        var ft = new DataTester(dm, "$ModelInfo.Documents.Filter");
        ft.AllProperties("Period,Agents,Company,Fragment,Warehouse");
        ft.AreValueEqual("FRAGMENT", "Fragment");

        var fromDate = dm.Eval<String>("$ModelInfo.Documents.Filter.Period.From")
            ?? throw new InvalidOperationException("Period from is null");
        var resDate = DateTime.Parse(fromDate.Replace("\"\\/\"", ""));
        Assert.AreEqual(resDate, today);

        var agents = dm.Eval<List<ExpandoObject>>("$ModelInfo.Documents.Filter.Agents")
            ?? throw new InvalidOperationException("Agenst is null");
        Assert.AreEqual(0, agents.Count);

        var company = dm.Eval<ExpandoObject>("$ModelInfo.Documents.Filter.Company")
            ?? throw new InvalidOperationException("Company is null");

        Assert.AreEqual(127, company.Get<Int32>("Id"));
        Assert.AreEqual("Company 127", company.Get<String>("Name"));

        var wh = dm.Eval<ExpandoObject>("$ModelInfo.Documents.Filter.Warehouse")
            ?? throw new InvalidOperationException("Warehouse is null");
        Assert.IsNull(wh.Get<Object>("Id"));
        Assert.IsNull(wh.Get<Object>("Name"));
    }
}
