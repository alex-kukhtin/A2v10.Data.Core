// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using System.Threading.Tasks;

using A2v10.Data.Tests.Configuration;
using Newtonsoft.Json;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Sheet")]
public class SpreadSheetTest
{
	readonly IDbContext _dbContext;
	public SpreadSheetTest()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task LoadSpreadsheetModel()
	{
		var dm = await _dbContext.LoadModelAsync("", "a2test.[Spreadsheet.Model.Load]");
		Assert.IsNotNull(dm);

		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TSheet,TRow,TColumn,TCell");
		md.HasAllProperties("TRoot", "Sheet");
		md.HasAllProperties("TSheet", "Rows,Columns,Cells,Id");
		md.HasAllProperties("TRow", "Index");
		md.HasAllProperties("TColumn", "Name");
		md.HasAllProperties("TCell", "Value");

		var json = JsonConvert.SerializeObject(dm.Root);
		Assert.IsNotNull(json);

		AssertSheetModel(dm);
	}

	private static void AssertSheetModel(IDataModel dm) 
	{ 
		// rows
		var dt = new DataTester(dm, "Sheet.Rows.1");
		dt.AreValueEqual(1, "Index");
		dt = new DataTester(dm, "Sheet.Rows.3");
		dt.AreValueEqual(3, "Index");
		dt = new DataTester(dm, "Sheet.Rows.5");
		dt.AreValueEqual(5, "Index");
		dt = new DataTester(dm, "Sheet.Rows.7");
		dt.AreValueEqual(7, "Index");

		// columns
		dt = new DataTester(dm, "Sheet.Columns.A");
		dt.AreValueEqual("Column 1", "Name");
		dt = new DataTester(dm, "Sheet.Columns.C");
		dt.AreValueEqual("Column 3", "Name");
		dt = new DataTester(dm, "Sheet.Columns.D");
		dt.AreValueEqual("Column 4", "Name");
		dt = new DataTester(dm, "Sheet.Columns.F");
		dt.AreValueEqual("Column 6", "Name");

		// Cells
		dt = new DataTester(dm, "Sheet.Cells.A1");
		dt.AreValueEqual("A1 value", "Value");
		dt = new DataTester(dm, "Sheet.Cells.C1");
		dt.AreValueEqual("C1 value", "Value");
		dt = new DataTester(dm, "Sheet.Cells.C3");
		dt.AreValueEqual("C3 value", "Value");
	}

	[TestMethod]
	public async Task UpdateSpreadsheetModel()
	{
		var dm = await _dbContext.LoadModelAsync("", "a2test.[Spreadsheet.Model.Load]");
		Assert.IsNotNull(dm);

		var res = await _dbContext.SaveModelAsync("", "a2test.[Spreadsheet.Model.Update]", dm.Root);

		var json = JsonConvert.SerializeObject(res.Root);
		Assert.IsNotNull(json);

		var dt = new DataTester(res, "Sheet");
		dt.AreValueEqual(7, "Id");

		AssertSheetModel(res);
	}

	[TestMethod]
	public async Task LoadModelWithProps()
	{
        var dm = await _dbContext.LoadModelAsync(null, "a2test.[Object.Props.Load]");
        Assert.IsNotNull(dm);
        var md = new MetadataTester(dm);
        //md.IsAllKeys("TRoot,TCard,TAttr");
        md.HasAllProperties("TRoot", "Card");
        md.HasAllProperties("TCard", "Id,Name,Attrs");
        md.HasAllProperties("TAttr", "Value");

        var json = JsonConvert.SerializeObject(dm.Root);
        Assert.IsNotNull(json);

    }
}