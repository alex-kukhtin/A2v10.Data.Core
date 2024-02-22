// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System.Threading.Tasks;

using A2v10.Data.Tests.Configuration;
using Newtonsoft.Json;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Sheet")]
public class SheetTest
{
	readonly IDbContext _dbContext;
	public SheetTest()
	{
		_dbContext = Starter.CreateWithTenants();
	}

	[TestMethod]
	public async Task LoadTopModel()
	{
		var dm = await _dbContext.LoadModelAsync("", "a2test.[Sheet.Model.Load]");
		Assert.IsNotNull(dm);

		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TModel,TSheet,TRow,TColumn,TCell");
		md.HasAllProperties("TRoot", "Model");
		md.HasAllProperties("TRow", "Id,Index,Cells");

		// var json = JsonConvert.SerializeObject(dm.Root);

		var dt = new DataTester(dm, "Model.Sheet.Rows");
		dt.IsArray(7);
		for (int i= 0; i < 7; i++)
			dt.AreArrayValueEqual(i + 1, i, "Index");
		dt.AreArrayValueEqual(100L, 0, "Id");
		dt.AreArrayValueEqual(101L, 2, "Id");
		dt.AreArrayValueEqual(102L, 4, "Id");
		dt.AreArrayValueEqual(103L, 6, "Id");

		dt = new DataTester(dm, "Model.Sheet.Columns");
		dt.IsArray(6);
		dt.AreArrayValueEqual("A", 0, "Key");
		dt.AreArrayValueEqual("Column 1", 0, "Name");
		dt.AreArrayValueEqual("B", 1, "Key");
		dt.AreArrayValueEqual("C", 2, "Key");
		dt.AreArrayValueEqual("Column 3", 2, "Name");
		dt.AreArrayValueEqual("D", 3, "Key");
		dt.AreArrayValueEqual("Column 4", 3, "Name");
		dt.AreArrayValueEqual("E", 4, "Key");
		dt.AreArrayValueEqual("F", 5, "Key");
		dt.AreArrayValueEqual("Column 6", 5, "Name");
		dt.AreArrayValueEqual(204L, 5, "Id");

		dt = new DataTester(dm, "Model.Sheet.Rows[0].Cells[0]");
		dt.AreValueEqual("A1", "Value");
		dt.AreValueEqual(500L, "Id");

		dt = new DataTester(dm, "Model.Sheet.Rows[0].Cells[2]");
		dt.AreValueEqual(501L, "Id");
		dt.AreValueEqual("C1", "Value");
		dt = new DataTester(dm, "Model.Sheet.Rows[2].Cells[2]");
		dt.AreValueEqual(502L, "Id");
		dt.AreValueEqual("C3", "Value");

    }

    [TestMethod]
	public async Task LoadSheetModel()
	{
		var dm = await _dbContext.LoadModelAsync("", "a2test.[Sheet.ModelRoot.Load]");
		Assert.IsNotNull(dm);

		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TSheet,TRow,TColumn,TCell");
		md.HasAllProperties("TRoot", "Sheet");
		md.HasAllProperties("TSheet", "Rows,Columns,Id");
		md.HasAllProperties("TRow", "Id,Index,Cells");

		//var json = JsonConvert.SerializeObject(dm.Root);

		var dt = new DataTester(dm, "Sheet.Rows");
		dt.IsArray(7);
		for (int i = 0; i < 7; i++)
			dt.AreArrayValueEqual(i + 1, i, "Index");
		dt.AreArrayValueEqual(100L, 0, "Id");
		dt.AreArrayValueEqual(101L, 2, "Id");
		dt.AreArrayValueEqual(102L, 4, "Id");
		dt.AreArrayValueEqual(103L, 6, "Id");

		dt = new DataTester(dm, "Sheet.Columns");
		dt.IsArray(6);
		dt.AreArrayValueEqual("A", 0, "Key");
		dt.AreArrayValueEqual("Column 1", 0, "Name");
		dt.AreArrayValueEqual("B", 1, "Key");
		dt.AreArrayValueEqual("C", 2, "Key");
		dt.AreArrayValueEqual("Column 3", 2, "Name");
		dt.AreArrayValueEqual("D", 3, "Key");
		dt.AreArrayValueEqual("Column 4", 3, "Name");
		dt.AreArrayValueEqual("E", 4, "Key");
		dt.AreArrayValueEqual("F", 5, "Key");
		dt.AreArrayValueEqual("Column 6", 5, "Name");
		dt.AreArrayValueEqual(204L, 5, "Id");

        dt = new DataTester(dm, "Sheet.Rows[0].Cells[0]");
		dt.AreValueEqual("A1", "Value");
		dt.AreValueEqual(500L, "Id");

		dt = new DataTester(dm, "Sheet.Rows[0].Cells[2]");
		dt.AreValueEqual(501L, "Id");
		dt.AreValueEqual("C1", "Value");
		dt = new DataTester(dm, "Sheet.Rows[2].Cells[2]");
		dt.AreValueEqual(502L, "Id");
		dt.AreValueEqual("C3", "Value");
    }
}
