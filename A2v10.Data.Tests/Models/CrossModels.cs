// Copyright © 2015-2022 Alex Kukhtin. All rights reserved.

using A2v10.Data.Tests;
using A2v10.Data.Tests.Configuration;
using System.Threading.Tasks;

namespace A2v10.Data.Models
{
	[TestClass]
	[TestCategory("Cross Models")]
	public class TestDataConfiguration
	{
		readonly IDbContext _dbContext;
		public TestDataConfiguration()
		{
			_dbContext = Starter.Create();
		}

		[TestMethod]
		public async Task LoadCrossArray()
		{
			var dm = await _dbContext.LoadModelAsync(null, "a2test.[CrossModel.Load]");

			var md = new MetadataTester(dm);
			md.IsAllKeys("TRoot,TData,TCross");
			md.HasAllProperties("TRoot", "RepData");
			md.HasAllProperties("TData", "Id,S1,N1,Cross1");
			md.IsId("TData", "Id");
			md.HasAllProperties("TCross", "Key,Val");
			md.IsKey("TCross", "Key");
			md.IsItemType("TData", "Cross1", FieldType.CrossArray);

			var dt = new DataTester(dm, "RepData");
			dt.IsArray(2);
			dt.AreArrayValueEqual(10, 0, "Id");
			dt.AreArrayValueEqual(20, 1, "Id");
			dt.AreArrayValueEqual("S1", 0, "S1");
			dt.AreArrayValueEqual("S2", 1, "S1");

			dt = new DataTester(dm, "RepData[0].Cross1");
			dt.IsArray(2);
			dt = new DataTester(dm, "RepData[0].Cross1");
			dt.IsArray(2);
			dt.AreArrayValueEqual("K1", 0, "Key");
			dt.AreArrayValueEqual(11, 0, "Val");

			dt = new DataTester(dm, "RepData[1].Cross1");
			dt.IsArray(2);
			dt.AreArrayValueEqual("K2", 1, "Key");
			dt.AreArrayValueEqual(22, 1, "Val");
		}

		[TestMethod]
		public async Task LoadCrossArrayMulti()
		{
			var dm = await _dbContext.LoadModelAsync(null, "a2test.[CrossModelMulti.Load]");

			var md = new MetadataTester(dm);
			md.IsAllKeys("TRoot,TData,TCross");
			md.HasAllProperties("TRoot", "RepData");
			md.HasAllProperties("TData", "Id,Sum,CrossDt,CrossCt,Items");
			md.IsId("TData", "Id");
			md.HasAllProperties("TCross", "Acc,Sum");
			md.IsKey("TCross", "Acc");
			md.IsItemType("TData", "CrossDt", FieldType.CrossArray);
			md.IsItemType("TData", "CrossCt", FieldType.CrossArray);

			var dt = new DataTester(dm, "RepData.Items");
			dt.IsArray(3);
			dt.AreArrayValueEqual(10, 0, "Id");
			dt.AreArrayValueEqual(20, 1, "Id");
			dt.AreArrayValueEqual(30, 2, "Id");
			dt.AreArrayValueEqual(100, 0, "Sum");
			dt.AreArrayValueEqual(200, 1, "Sum");
			dt.AreArrayValueEqual(300, 2, "Sum");

			int dtSize = 2;
			dt = new DataTester(dm, "RepData.Items[0].CrossDt");
			dt.IsArray(dtSize);
			dt.AreArrayValueEqual("A1", 0, "Acc");
			dt.AreArrayValueEqual(11, 0, "Sum");
			dt.AreArrayValueEqual("A2", 1, "Acc");
			dt.AreArrayValueEqual(22, 1, "Sum");
			dt = new DataTester(dm, "RepData.Items[1].CrossDt");
			dt.IsArray(dtSize);
			dt = new DataTester(dm, "RepData.Items[2].CrossDt");
			dt.IsArray(dtSize);

			var ctSize = 2;
			dt = new DataTester(dm, "RepData.Items[0].CrossCt");
			dt.IsArray(ctSize);
			dt.AreArrayValueEqual("A2", 0, "Acc");
			dt.AreArrayValueEqual(44, 0, "Sum");
		}

		[TestMethod]
		public async Task LoadCrossObject()
		{
			var dm = await _dbContext.LoadModelAsync(null, "a2test.[CrossModelObj.Load]");

			var md = new MetadataTester(dm);
			md.IsAllKeys("TRoot,TData,TCross,TCrossObject");
			md.HasAllProperties("TRoot", "RepData");
			md.HasAllProperties("TData", "Id,S1,N1,Cross1");
			md.IsId("TData", "Id");
			md.HasAllProperties("TCross", "Key,Val");
			md.IsKey("TCross", "Key");
			md.IsItemType("TData", "Cross1", FieldType.CrossObject);

			var dt = new DataTester(dm, "RepData");
			dt.IsArray(2);
			dt.AreArrayValueEqual(10, 0, "Id");
			dt.AreArrayValueEqual(20, 1, "Id");
			dt.AreArrayValueEqual("S1", 0, "S1");
			dt.AreArrayValueEqual("S2", 1, "S1");

			dt = new DataTester(dm, "RepData[0].Cross1.K1");
			dt.AreValueEqual("K1", "Key");
			dt.AreValueEqual(11, "Val");

			dt = new DataTester(dm, "RepData[1].Cross1.K2");
			dt.AreValueEqual("K2", "Key");
			dt.AreValueEqual(22, "Val");
		}

		[TestMethod]
		public async Task LoadCrossGroup()
		{
			var dm = await _dbContext.LoadModelAsync(null, "a2test.[GroupWithCross.Load]");
			//var json = JsonConvert.SerializeObject(dm.Root);

			var md = new MetadataTester(dm);
			md.IsAllKeys("TRoot,TModel,TCross");
			md.HasAllProperties("TRoot", "Model");
			md.HasAllProperties("TModel", "Id,Company,Agent,Amount,Items,Cross1");
			md.IsId("TModel", "Id");

			md.HasAllProperties("TCross", "Key,Val");
			md.IsKey("TCross", "Key");
			md.IsItemType("TModel", "Cross1", FieldType.CrossArray);

			var dt = new DataTester(dm, "Model.Items[0].Items[0]");
			dt.AreValueEqual("Company1", "Company");
			dt.AreValueEqual("Agent1", "Agent");
			dt.AreValueEqual(400M, "Amount");
			dt.AreValueEqual("[Company1:Agent1]", "Id");

			dt = new DataTester(dm, "Model.Items[0].Items[0].Cross1");
			dt.IsArray(2);
			dt.AreArrayValueEqual("K1", 0, "Key");
			dt.AreArrayValueEqual(11, 0, "Val");

			dt = new DataTester(dm, "Model.Items[1].Items[1].Cross1");
			dt.IsArray(2);
			dt.AreArrayValueEqual("K2", 1, "Key");
			dt.AreArrayValueEqual(22, 1, "Val");
		}
	}
}
