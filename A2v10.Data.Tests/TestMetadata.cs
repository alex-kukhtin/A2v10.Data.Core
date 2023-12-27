// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data.Tests.Configuration;
using System.Threading.Tasks;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Data Metadata")]
public class TestMetadata
{
	readonly IDbContext _dbContext;
	public TestMetadata()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task MapObjects()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[MapObjects.NoKey.Load]");
		var md = new MetadataTester(dm);
		md.HasAllProperties("TRoot", "Document,Categories");
		md.IsItemRefObject("TDocument", "Category", "TCategory", FieldType.Object);
		md.IsItemIsArrayLike("TRoot", "Categories");
		md.IsItemRefObject("TRoot", "Categories", "TCategory", FieldType.Map);
	}

	[TestMethod]
	public async Task SheetPlain()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[Sheet.ModelRoot.Load]");
		var md = new MetadataTester(dm);
		md.HasAllProperties("TRoot", "Sheet");
		md.IsItemRefObject("TRoot", "Sheet", "TSheet", FieldType.Sheet);
		md.IsItemIsArrayLike("TSheet", "Rows");
		md.IsItemIsArrayLike("TSheet", "Columns");
	}
}
