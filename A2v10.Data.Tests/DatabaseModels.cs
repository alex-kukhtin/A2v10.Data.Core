// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using A2v10.Data.Tests.Configuration;
using System.Data;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Complex Models")]
public class DatabaseModels
{
	readonly IDbContext _dbContext;
	public DatabaseModels()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task LoadDocumentWithRowsAndMethods()
	{
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[Document.RowsMethods.Load]");

		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TDocument,TRow,TMethod,TMethodMap,TMethodData");
		md.HasAllProperties("TRoot", "Document");
		md.HasAllProperties("TDocument", "Name,Id,Rows");
		md.HasAllProperties("TMethodMap", "Mtd1,Mtd2");
		md.HasAllProperties("TMethodData", "Id,Code");
		md.IsId("TDocument", "Id");

		md.HasAllProperties("TRow", "Id,Methods");
		md.HasAllProperties("TMethod", "Key,Name,Id,Data");

		var dt = new DataTester(dm, "Document");
		dt.AreValueEqual(123, "Id");

		dt = new DataTester(dm, "Document.Rows");
		dt.IsArray(1);

		dt = new DataTester(dm, "Document.Rows[0].Methods.Mtd1");
		dt.AreValueEqual("Mtd1", "Key");
		dt.AreValueEqual("Method 1", "Name");

		dt = new DataTester(dm, "Document.Rows[0].Methods.Mtd2");
		dt.AreValueEqual("Mtd2", "Key");
		dt.AreValueEqual("Method 2", "Name");

		dt = new DataTester(dm, "Document.Rows[0].Methods.Mtd1.Data");
		dt.IsArray(1);
		dt.AreArrayValueEqual("Code1", 0, "Code");
		dt.AreArrayValueEqual(276, 0, "Id");
	}

	[TestMethod]
	public async Task WriteDocumentWithRowsAndMethods()
	{
		const String jsonData = @"
{'Document': {
	'Id': 0,
	'Name':'Document',
	'Rows':[{
		'Id': 0,
		'Methods': {
			'Mtd1':{
				'Name':'Method 1',
				'Data':[
					{'Id':0,'Code':'Code1'},
					{'Id':0,'Code':'Code2'}
				]
			},
			'Mtd2':{
				'Name':'Method 2',
				'Data':[
					{'Id':0,'Code':'Code3'}
				]
			}
		},
	},
	{
		'Id': 0,
		'Methods': {
			'Mtd1': {
				'Name': 'Method 3',
				'Data' : [
					{'Id':0,'Code':'Code4'}
				]
			}
		}
	}]
}}";
		var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
		Assert.IsNotNull(dataToSave);
		IDataModel? dm = null;
		try
		{
			dm = await _dbContext.SaveModelAsync(null, "a2test.[Document.RowsMethods.Update]", dataToSave);
		}
		catch (Exception /*ex*/)
		{
			throw;
		}

		var dt = new DataTester(dm, "Document");
		dt.AreValueEqual<Object>(null, "Id");

		dt = new DataTester(dm, "Rows");
		dt.IsArray(2);
		dt.AreArrayValueEqual<Object>(null, 0, "Id");
		dt.AreArrayValueEqual(1, 0, "RowNo");
		dt.AreArrayValueEqual(2, 1, "RowNo");
		dt.AreArrayValueEqual<Object>(null, 0, "Id");
		dt.AreArrayValueEqual<Object>(null, 1, "Id");

		dt = new DataTester(dm, "Methods");
		dt.IsArray(3);
		dt.AreArrayValueEqual("Method 1", 0, "Name");
		dt.AreArrayValueEqual("Mtd1", 0, "Key");
		dt.AreArrayValueEqual(1, 0, "RowNo");

		dt.AreArrayValueEqual("Method 2", 1, "Name");
		dt.AreArrayValueEqual("Mtd2", 1, "Key");
		dt.AreArrayValueEqual(1, 1, "RowNo");

		dt.AreArrayValueEqual("Method 3", 2, "Name");
		dt.AreArrayValueEqual("Mtd1", 2, "Key");
		dt.AreArrayValueEqual(2, 2, "RowNo");

		dt = new DataTester(dm, "MethodData");
		dt.IsArray(4);
		dt.AreArrayValueEqual("Code1", 0, "Code");
		dt.AreArrayValueEqual("Code2", 1, "Code");
		dt.AreArrayValueEqual("Code3", 2, "Code");
		dt.AreArrayValueEqual("Code4", 3, "Code");
		dt.AreArrayValueEqual(1, 0, "RowNo");
		dt.AreArrayValueEqual(1, 1, "RowNo");
		dt.AreArrayValueEqual(1, 2, "RowNo");
		dt.AreArrayValueEqual(2, 3, "RowNo");

		dt.AreArrayValueEqual("Mtd1", 0, "Key");
		dt.AreArrayValueEqual("Mtd1", 1, "Key");
		dt.AreArrayValueEqual("Mtd2", 2, "Key");
		dt.AreArrayValueEqual("Mtd1", 3, "Key");
	}


	[TestMethod]
	public async Task WriteModelWithGuid()
	{
		const String jsonData = @"
{'Document': {
	'Id': 274,
	'Name':'Document',
	'Agent': {
		'Id': 'A2CA67CB-0ECF-41A6-9492-4A62BE75AB47'
	},
	'Rows':[{
			'Id': 7,
			'Item' : {
				'Id': 'FE7406D9-960A-4EE6-85F9-45FA3450D25A'
			}
		},
		{
			'Id': 8,
			'Item' : {
				'Id': '573E31C9-3567-46BF-BB59-D4A49BF3E23F'
			}
		}]
}}";
		var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
		IDataModel? dm = null;
		if (dataToSave == null)
			throw new NoNullAllowedException();
		try
		{
			dm = await _dbContext.SaveModelAsync(null, "a2test.[Document.Guids.Update]", dataToSave);
		}
		catch (Exception /*ex*/)
		{
			throw;
		}

		var dt = new DataTester(dm, "Document");
		dt.AreValueEqual<Int64>(274, "Id");
		dt.AreValueEqual<Guid>(Guid.Parse("A2CA67CB-0ECF-41A6-9492-4A62BE75AB47"), "Agent");

		dt = new DataTester(dm, "Document.Rows");
		dt.IsArray(2);
		dt.AreArrayValueEqual<Int64>(7, 0, "Id");
		dt.AreArrayValueEqual<Int64>(8, 1, "Id");
		dt.AreArrayValueEqual<Guid>(Guid.Parse("FE7406D9-960A-4EE6-85F9-45FA3450D25A"), 0, "Item");
		dt.AreArrayValueEqual<Guid>(Guid.Parse("573E31C9-3567-46BF-BB59-D4A49BF3E23F"), 1, "Item");
	}

	[TestMethod]
	public async Task WriteModelSameProps()
	{
		const String jsonData = @"
{'Agent': {
	'Id': 274,
	'Tags':[
		{'Id': 7, SubTags: [{Id: 15 }, {Id: 20}] }, 
		{'Id': 8, SubTags: [{Id: 12 }, {Id: 21}] }
	]},
  'Tags': [
	{'Id': 3,}, 
	{'Id': 4,}
  ]
}";
		var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
		IDataModel? dm = null;
		try
		{
			dm = await _dbContext.SaveModelAsync(null, "a2test.[Agent.SameProps.Update]", dataToSave);
		}
		catch (Exception /*ex*/)
		{
			throw;
		}

		var dt = new DataTester(dm, "Agent");
		dt.AreValueEqual<Int64>(274, "Id");

		dt = new DataTester(dm, "Agent.Tags");
		dt.IsArray(2);
		dt.AreArrayValueEqual<Int64>(7, 0, "Id");
		dt.AreArrayValueEqual<Int64>(8, 1, "Id");

		dt = new DataTester(dm, "Agent.Tags[0].SubTags");
		dt.IsArray(2);
		dt.AreArrayValueEqual<Int64>(15, 0, "Id");
		dt.AreArrayValueEqual<Int64>(20, 1, "Id");

		dt = new DataTester(dm, "Agent.Tags[1].SubTags");
		dt.IsArray(2);
		dt.AreArrayValueEqual<Int64>(12, 0, "Id");
		dt.AreArrayValueEqual<Int64>(21, 1, "Id");
	}
}