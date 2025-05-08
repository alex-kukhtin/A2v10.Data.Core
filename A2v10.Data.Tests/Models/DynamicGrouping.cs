// Copyright © 2022-2025 Oleksandr Kukhtin. All rights reserved.

using System.Threading.Tasks;
using System.Dynamic;


using A2v10.Data.Tests.Configuration;
using A2v10.Data.Tests;

namespace A2v10.Data.Models;

[TestClass]
[TestCategory("Dynamic Grouping")]
public class DynamicGrouping
{
    private readonly IDbContext _dbContext;
    public DynamicGrouping()
    {
        _dbContext = Starter.Create();
    }

    [TestMethod]
    public async Task LoadDynamicGroups()
    {
        var dm = await _dbContext.LoadModelAsync(null, "a2test.[DynamicGrouping.Index]");

        //var json = JsonConvert.SerializeObject(dm.Root);

        Assert.IsNotNull(dm);
        var md = new MetadataTester(dm);
        md.HasAllProperties("TRoot", "Trans");
        var dt = new DataTester(dm, "Trans");
        dt.AreValueEqual(97M, "Sum");
        dt.AreValueEqual(5.75, "Price");
        dt.AreValueEqual(2, "Qty");
        dt = new DataTester(dm, "Trans.Items");
        dt.IsArray(2);
        dt.AreArrayValueEqual(50M, 0, "Sum");
        dt.AreArrayValueEqual(7.0, 0, "Price");
        dt = new DataTester(dm, "Trans.Items[0].Agent");
        dt.AreValueEqual(100L, "Id");
        dt.AreValueEqual("Agent 100", "Name");
        dt = new DataTester(dm, "Trans.Items[0].Items");
        dt.IsArray(2);
        dt = new DataTester(dm, "Trans.Items[0].Items[0].Cross1[0]");
        dt.AreValueEqual(10.0, "Value");
        dt.AreValueEqual("K1", "Key");
        dt = new DataTester(dm, "Trans.Items[0].Items[0].Cross1[1]");
        dt.AreValueEqual(20.0, "Value");
        dt.AreValueEqual("K2", "Key");
		dt = new DataTester(dm, "Trans.Items[0].Cross1[0]");
		dt.AreValueEqual("K1", "Key");
		dt.AreValueEqual(10.0, "Value");
		dt = new DataTester(dm, "Trans.Items[0].Cross1[1]");
		dt.AreValueEqual("K2", "Key");
		dt.AreValueEqual(20.0, "Value");
		dt = new DataTester(dm, "Trans.Cross1[0]");
		dt.AreValueEqual("K1", "Key");
		dt.AreValueEqual(40.0, "Value");
		dt = new DataTester(dm, "Trans.Cross1[1]");
		dt.AreValueEqual("K2", "Key");
		dt.AreValueEqual(60.0, "Value");
	}

	[TestMethod]
	public async Task DynamicGroupsDate()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[DynamicGrouping.Date]");
        //var json = JsonConvert.SerializeObject(dm.Root);
		Assert.IsNotNull(dm);
		var md = new MetadataTester(dm);
		md.HasAllProperties("TRoot", "Trans");
		var dt = new DataTester(dm, "Trans");
		dt.AreValueEqual(97M, "Sum");
		dt.AreValueEqual(5.75, "Price");
		dt = new DataTester(dm, "Trans.Items");
		dt.IsArray(2);
		dt.AreArrayValueEqual(50M, 0, "Sum");
		dt.AreArrayValueEqual(7.0, 0, "Price");
		dt = new DataTester(dm, "Trans.Items[0]");
		dt.AreValueEqual(new DateTime(2023, 01, 01), "Date");
	}

	[TestMethod]
	public async Task DynamicGroups2_Full()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[DynamicGrouping2.Index]", new ExpandoObject()
		{
			{ "Group", "All" }
		});
		Assert.IsNotNull(dm);
		//var json = JsonConvert.SerializeObject(dm.Root);

		Assert.AreEqual(603M, dm.Root.Eval<Decimal>("Trans.Items[0].Items[0].Items[0].Cross1[0].Sum"));
		Assert.AreEqual(603M, dm.Root.Eval<Decimal>("Trans.Items[0].Items[0].Items[0].Sum"));
		Assert.AreEqual(12.2, dm.Root.Eval<Double>("Trans.Items[0].Items[0].Items[0].Cross1[0].Qty"));

		Assert.AreEqual(252M, dm.Root.Eval<Decimal>("Trans.Items[0].Items[0].Items[1].Cross1[0].Sum"));
		Assert.AreEqual(252M, dm.Root.Eval<Decimal>("Trans.Items[0].Items[0].Items[1].Sum"));
		Assert.AreEqual(14.5, dm.Root.Eval<Double>("Trans.Items[0].Items[0].Items[1].Cross1[0].Qty"));

		Assert.AreEqual(2355M, dm.Root.Eval<Decimal>("Trans.Items[0].Cross1[0].Sum"));
		Assert.AreEqual(2355M, dm.Root.Eval<Decimal>("Trans.Cross1[0].Sum"));
		Assert.AreEqual(2355M, dm.Root.Eval<Decimal>("Trans.Sum"));
		Assert.AreEqual(41.7, dm.Root.Eval<Double>("Trans.Cross1[0].Qty"));

		var dt = new DataTester(dm, "Trans.Items[0].Cross1[0]");
		dt.AreValueEqual(2355M, "Sum");
		dt = new DataTester(dm, "Trans.Items[0].Items[0].Cross1[0]");
		dt.AreValueEqual(2355M, "Sum");

	}

	[TestMethod]
	public async Task DynamicGroups2_Item()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[DynamicGrouping2.Index]", new ExpandoObject()
		{
			{ "Group", "Item" }
		});
		Assert.IsNotNull(dm);

		// var json = JsonConvert.SerializeObject(dm.Root);

		Assert.AreEqual(2355M, dm.Root.Eval<Decimal>("Trans.Cross1[0].Sum"));
		Assert.AreEqual(41.7, dm.Root.Eval<Double>("Trans.Cross1[0].Qty"));
		Assert.AreEqual(2355M, dm.Root.Eval<Decimal>("Trans.Sum"));
		Assert.AreEqual(603M, dm.Root.Eval<Decimal>("Trans.Items[0].Cross1[0].Sum"));
		Assert.AreEqual(12.2, dm.Root.Eval<Double>("Trans.Items[0].Cross1[0].Qty"));

	}

	[TestMethod]
	public async Task DynamicGroups2_Company()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[DynamicGrouping2.Index]", new ExpandoObject()
		{
			{ "Group", "Company" }
		});
		Assert.IsNotNull(dm);

		// var json = JsonConvert.SerializeObject(dm.Root);

		Assert.AreEqual(2355M, dm.Root.Eval<Decimal>("Trans.Cross1[0].Sum"));
		Assert.AreEqual(41.7, dm.Root.Eval<Double>("Trans.Cross1[0].Qty"));
		Assert.AreEqual(2355M, dm.Root.Eval<Decimal>("Trans.Sum"));
	}

	[TestMethod]
	public async Task DynamicGroups2_None()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[DynamicGrouping2.Index]", new ExpandoObject()
		{
			{ "Group", "None" }
		});
		Assert.IsNotNull(dm);

		//var json = JsonConvert.SerializeObject(dm.Root);

		Assert.AreEqual(2355M, dm.Root.Eval<Decimal>("Trans.Cross1[0].Sum"));
		Assert.AreEqual(41.7, dm.Root.Eval<Double>("Trans.Cross1[0].Qty"));
		Assert.AreEqual(2355M, dm.Root.Eval<Decimal>("Trans.Sum"));
	}


    [TestMethod]
    public async Task DynamicGroupsReference()
    {
        var dm = await _dbContext.LoadModelAsync(null, "a2test.[DynamicGrouping.Reference]");

        //var json = JsonConvert.SerializeObject(dm.Root);

        Assert.IsNotNull(dm);
        var md = new MetadataTester(dm);
        md.HasAllProperties("TRoot", "RepData");
        var dt = new DataTester(dm, "RepData");
        dt.AreValueEqual(8000M, "InSum");
        dt.AreValueEqual(500M, "OutSum");
        dt = new DataTester(dm, "RepData.Items");
        dt.IsArray(2);
        dt.AreArrayValueEqual(7000M, 0, "InSum");
        dt.AreArrayValueEqual(100M, 0, "OutSum");
        dt = new DataTester(dm, "RepData.Items[0].Store");
        dt.AreValueEqual(1006L, "Id");
        dt.AreValueEqual("Prod", "Name");
        dt = new DataTester(dm, "RepData.Items[0].Items");
        dt.IsArray(3);
        dt = new DataTester(dm, "RepData.Items[0].Items[0].Item");
        dt.AreValueEqual(1001L, "Id");
        dt.AreValueEqual("T1", "Name");
        dt = new DataTester(dm, "RepData.Items[0].Items[1].Item");
        dt.AreValueEqual(1002L, "Id");
        dt.AreValueEqual("T2", "Name");
    }
}