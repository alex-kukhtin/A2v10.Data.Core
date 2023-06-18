// Copyright © 2022-2023 Oleksandr Kukhtin. All rights reserved.

using System.Threading.Tasks;


using A2v10.Data.Tests.Configuration;
using A2v10.Data.Tests;
using Newtonsoft.Json;

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
        // var json = JsonConvert.SerializeObject(dm.Root);
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
}