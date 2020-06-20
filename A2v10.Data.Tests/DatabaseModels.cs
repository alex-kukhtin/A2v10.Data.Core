// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;
using A2v10.Data.Interfaces;
using A2v10.Data.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace A2v10.Data.Tests
{
	[TestClass]
	public class DatabaseModels
	{
		IDbContext _dbContext;
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
			IDataModel dm = null;
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
	}
}