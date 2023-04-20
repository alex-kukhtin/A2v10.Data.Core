// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System.Diagnostics;
using System.Threading.Tasks;

using A2v10.Data.Tests.Configuration;
using DeepEqual.Syntax;
using Newtonsoft.Json;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Typed Models")]
public class TypedModelTest
{
	private readonly IDbContext _dbContext;

	public TypedModelTest()
	{
		_dbContext = Starter.Create();
	}
	[TestInitialize]
	public void Setup()
	{
	}

	[TestMethod]
	public async Task LoadDocument()
	{
		var root = await _dbContext.LoadTypedModelAsync<LoadedDocument>(null, "a2test.ComplexModelTyped", null)
			?? throw new InvalidOperationException("root is null");
		var doc = root.Document;
		Assert.IsNotNull(doc);
		Assert.AreEqual(123, doc.Id);
		Assert.AreEqual("DocNo", doc.No);
		Assert.AreEqual(doc.Date, new DateTime(2023, 04, 20, 12, 30, 17));
		Assert.IsNotNull(doc.Agent);
		Assert.IsNotNull(doc.Company);
		Assert.AreEqual(doc.Agent, doc.Company);
		Assert.AreEqual(doc.Agent.Id, 512);
		Assert.AreEqual(doc.Agent.Name, "Agent 512");
		Assert.AreEqual(doc.Agent.Code, "Code 512");
		Assert.AreEqual(1, doc.Rows1.Count);
		Assert.AreEqual(1, doc.Rows2.Count);

		var row1 = doc.Rows1[0];
		Assert.AreEqual(78, row1.Id);
		Assert.AreEqual(4.0, row1.Qty);
		Assert.AreEqual(8M, row1.Price);
		Assert.AreEqual(32M, row1.Sum);
		var prod1 = row1.Product;
		Assert.IsNotNull(prod1);
		Assert.AreEqual(782, prod1.Id);
		Assert.AreEqual("Product 782", prod1.Name);
		Assert.AreEqual(1, row1.Series1.Count);
		var ser1 = row1.Series1[0];
		Assert.AreEqual(500, ser1.Id);
		Assert.AreEqual(5.0, ser1.Price);

		var row2 = doc.Rows2[0];
		Assert.AreEqual(79, row2.Id);
		Assert.AreEqual(7.0, row2.Qty);
		Assert.AreEqual(2M, row2.Price);
		Assert.AreEqual(14M, row2.Sum);
		var prod2 = row2.Product;
		Assert.IsNotNull(prod2);
		Assert.AreEqual(785, prod2.Id);
		Assert.AreEqual("Product 785", prod2.Name);
		Assert.AreEqual(1, row2.Series1.Count);
		var ser2 = row2.Series1[0];
		Assert.AreEqual(501, ser2.Id);
		Assert.AreEqual(10.0, ser2.Price);

		Assert.AreEqual(1, root.Agents.Count);
		Assert.AreEqual(2, root.Products.Count);
	}

	[TestMethod]
	public async Task LoadCollection()
	{
		var root = await _dbContext.LoadTypedModelAsync<LoadedDocuments>(null, "a2test.ComplexModelTypedArray", null)
			?? throw new InvalidOperationException("Root is null");
		var docs = root.Documents;
		Assert.IsNotNull(docs);
		Assert.AreEqual(1, docs.Count);
		var doc = docs[0];
		Assert.AreEqual(123, doc.Id);
		Assert.AreEqual("DocNo", doc.No);
		Assert.IsTrue(Math.Abs((doc.Date - DateTime.Now).TotalSeconds) < 1);
		Assert.IsNotNull(doc.Agent);
		Assert.IsNotNull(doc.Company);
	}

    [TestMethod]
    public async Task CompareWithJson()
    {
        var root = await _dbContext.LoadTypedModelAsync<LoadedDocument>(null, "a2test.ComplexModelTyped", null);
        var docTyped = root;
		var dm = await _dbContext.LoadModelAsync(null, "a2test.ComplexModelTyped");
		var strJson = JsonConvert.SerializeObject(dm.Root);
        var docFromJson = JsonConvert.DeserializeObject<LoadedDocument>(strJson);
		docTyped.ShouldDeepEqual(docFromJson);
    }

    [TestMethod]
	public Task LoadingTime()
	{
		return Task.CompletedTask;
		/*
		int count = 50;
		var s1 = Stopwatch.StartNew();
		for (int i= 0; i<count; i++)
		{
            var root = await _dbContext.LoadTypedModelAsync<LoadedDocuments>("NovaEra", "dbo.ComplexModelTyped", null);
        }
        s1.Stop();

        var s2 = Stopwatch.StartNew();
		LoadedDocuments docs;
        for (int i = 0; i < count; i++)
        {
            var root = await _dbContext.LoadModelAsync("NovaEra", "dbo.ComplexModelTyped", null);
			var str = JsonConvert.SerializeObject(root.Root);
			docs = JsonConvert.DeserializeObject<LoadedDocuments>(str);
        }
        s2.Stop();
		*/
    }

    [TestMethod]
    public async Task LoadAndSave()
	{
		var srcModel = await _dbContext.LoadTypedModelAsync<RowsMethods>(null, "a2test.[Document.RowsMethods.Load]", null);
		Assert.IsNotNull(srcModel);
		var resModel = await _dbContext.SaveTypedModelAsync<RowsMethods, RowsMethods>(null, "a2test.[Document.RowsMethodsTyped.Update]", srcModel, null);
        Assert.IsNotNull(resModel);
		srcModel.ShouldDeepEqual(resModel);
    }

}

