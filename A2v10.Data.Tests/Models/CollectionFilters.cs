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

        var mis = dm.Metadata["TRoot"].ModelInfos
            ?? throw new InvalidOperationException("ModelInfos is null");
        var mi = mis["Documents"];
        Assert.IsTrue(mi.HasPageSize);
        Assert.IsTrue(mi.HasOffset);
        Assert.IsTrue(mi.HasSortOrder);
        Assert.IsTrue(mi.HasSortDir);
        Assert.IsFalse(mi.HasGroupBy);
        Assert.IsFalse(mi.HasRowCount);

        var fm = mi.Filters
            ?? throw new InvalidOperationException("Filters is null");
        Assert.HasCount(6, fm);
        Assert.AreEqual(FilterType.Period, fm["Period"].Type);
        Assert.IsNull(fm["Period"].RefType);
        Assert.AreEqual(FilterType.Ref, fm["Agent"].Type);
        Assert.AreEqual("TObject", fm["Agent"].RefType);
        Assert.AreEqual(FilterType.String, fm["Fragment"].Type);
        Assert.AreEqual(FilterType.String, fm["NullString"].Type);
        Assert.AreEqual(FilterType.Ref, fm["Company"].Type);
        Assert.AreEqual("TCompany", fm["Company"].RefType);
        Assert.AreEqual(FilterType.Ref, fm["Warehouse"].Type);
        Assert.AreEqual("TWarehouse", fm["Warehouse"].RefType);
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
        Assert.HasCount(3, agents);

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

        var mis = dm.Metadata["TRoot"].ModelInfos
            ?? throw new InvalidOperationException("ModelInfos is null");
        var fm = mis["Documents"].Filters
            ?? throw new InvalidOperationException("Filters is null");
        Assert.HasCount(5, fm);
        Assert.AreEqual(FilterType.Period, fm["Period"].Type);
        Assert.AreEqual(FilterType.RefArray, fm["Agents"].Type);
        Assert.AreEqual("TAgent", fm["Agents"].RefType);
        Assert.AreEqual(FilterType.String, fm["Fragment"].Type);
        Assert.AreEqual(FilterType.Ref, fm["Company"].Type);
        Assert.AreEqual("TCompany", fm["Company"].RefType);
        Assert.AreEqual(FilterType.Ref, fm["Warehouse"].Type);
        Assert.AreEqual("TWarehouse", fm["Warehouse"].RefType);
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
        Assert.HasCount(0, agents);

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
    public async Task FilterMetadata()
    {
        var dm = await _dbContext.LoadModelAsync(null, "a2test.[FiltersMeta.Load]");

        // legacy root-level PageSize goes to $System only
        Assert.IsNotNull(dm.System);
        Assert.AreEqual(10, dm.System.Get<Int32>("PageSize"));

        var mis = dm.Metadata["TRoot"].ModelInfos
            ?? throw new InvalidOperationException("ModelInfos is null");
        Assert.HasCount(1, mis);
        var mi = mis["Elements"];
        Assert.IsTrue(mi.HasPageSize);
        Assert.IsTrue(mi.HasGroupBy);
        Assert.IsTrue(mi.HasRowCount);
        Assert.IsFalse(mi.HasOffset);
        Assert.IsFalse(mi.HasSortOrder);
        Assert.IsFalse(mi.HasSortDir);

        var fm = mi.Filters
            ?? throw new InvalidOperationException("Filters is null");
        Assert.HasCount(5, fm);
        Assert.AreEqual(FilterType.Boolean, fm["Flag"].Type);
        Assert.AreEqual(FilterType.Number, fm["Count"].Type);
        Assert.AreEqual(FilterType.Date, fm["DateOpt"].Type);
        // the "Period" prefix rule applies to nested nodes only
        Assert.AreEqual(FilterType.Period, fm["PeriodShip"].Type);
        Assert.IsNull(fm["PeriodShip"].RefType);
        Assert.AreEqual(FilterType.String, fm["PeriodKind"].Type);
    }
}
