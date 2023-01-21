﻿// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System.Reflection.Metadata;
using System.Threading.Tasks;

using A2v10.Data.Tests.Configuration;

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
		var root = await _dbContext.LoadTypedModelAsync<LoadedDocument>(null, "a2test.ComplexModel", null);
		var doc = root.Document;
		Assert.IsNotNull(doc);	

		/*
		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TDocument,TRow,TAgent,TProduct,TSeries,TUnit");
		md.HasAllProperties("TRoot", "Document");
		md.HasAllProperties("TDocument", "Id,No,Date,Agent,Company,Rows1,Rows2");
		md.HasAllProperties("TRow", "Id,Product,Qty,Price,Sum,Series1");
		md.HasAllProperties("TProduct", "Id,Name,Unit");
		md.HasAllProperties("TUnit", "Id,Name");

		var docT = new DataTester(dm, "Document");
		docT.AreValueEqual(123, "Id");
		docT.AreValueEqual("DocNo", "No");

		var agentT = new DataTester(dm, "Document.Agent");
		agentT.AreValueEqual(512, "Id");
		agentT.AreValueEqual("Agent 512", "Name");
		agentT.AreValueEqual("Code 512", "Code");

		agentT = new DataTester(dm, "Document.Company");
		agentT.AreValueEqual(512, "Id");
		agentT.AreValueEqual("Agent 512", "Name");
		agentT.AreValueEqual("Code 512", "Code");

		var row1T = new DataTester(dm, "Document.Rows1");
		row1T.IsArray(1);
		row1T.AreArrayValueEqual(78, 0, "Id");
		row1T.AreArrayValueEqual(4.0, 0, "Qty");

		var row2T = new DataTester(dm, "Document.Rows2");
		row2T.IsArray(1);
		row2T.AreArrayValueEqual(79, 0, "Id");
		row2T.AreArrayValueEqual(7.0, 0, "Qty");

		var row1Obj = new DataTester(dm, "Document.Rows1[0]");
		row1Obj.AreValueEqual(78, "Id");
		row1Obj.AreValueEqual(4.0, "Qty");
		row1Obj.AllProperties("Id,Qty,Price,Sum,Product,Series1");

		var prodObj = new DataTester(dm, "Document.Rows1[0].Product");
		prodObj.AreValueEqual(782, "Id");
		prodObj.AreValueEqual("Product 782", "Name");
		prodObj.AllProperties("Id,Name,Unit");
		var unitObj = new DataTester(dm, "Document.Rows1[0].Product.Unit");
		unitObj.AreValueEqual(7, "Id");
		unitObj.AreValueEqual("Unit7", "Name");
		unitObj.AllProperties("Id,Name");

		prodObj = new DataTester(dm, "Document.Rows2[0].Product");
		prodObj.AreValueEqual(785, "Id");
		prodObj.AreValueEqual("Product 785", "Name");
		unitObj = new DataTester(dm, "Document.Rows2[0].Product.Unit");
		unitObj.AreValueEqual(8, "Id");
		unitObj.AreValueEqual("Unit8", "Name");

		var seriesObj = new DataTester(dm, "Document.Rows1[0].Series1");
		seriesObj.IsArray(1);
		seriesObj.AreArrayValueEqual(500, 0, "Id");
		seriesObj.AreArrayValueEqual(5.0, 0, "Price");

		seriesObj = new DataTester(dm, "Document.Rows2[0].Series1");
		seriesObj.IsArray(1);
		seriesObj.AreArrayValueEqual(501, 0, "Id");
		seriesObj.AreArrayValueEqual(10.0, 0, "Price");
		*/
	}

}

