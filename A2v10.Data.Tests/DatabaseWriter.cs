// Copyright © 2015-2022 Alex Kukhtin. All rights reserved.

using System.Dynamic;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests
{
	[TestClass]
	[TestCategory("Write models")]
	public class DatabaseWriter
	{
		readonly IDbContext _dbContext;

		public DatabaseWriter()
		{
			_dbContext = Starter.Create();
		}

		[TestMethod]
		public async Task WriteSubObjectData()
		{
			// DATA with ROOT
			var jsonData = @"
			{
			    MainObject: {
				    Id : 45,
				    Name: 'MainObjectName',
				    NumValue : 531.55,
				    BitValue : true,
					GuidValue: '0db82076-0bec-4c5c-adbf-73A056FCCB04',
				    SubObject : {
					    Id: 55,
					    Name: 'SubObjectName',
						GuidValue: '',
					    SubArray: [
						    {X: 5, Y:6, D:5.1 },
						    {X: 8, Y:9, D:7.23 }
					    ]
				    }		
			    }
            }
			";
			IDataModel? dm = null;
			var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
			Assert.IsNotNull(dataToSave);
			try
			{
				dm = await _dbContext.SaveModelAsync(null, "a2test.[NestedObject.Update]", dataToSave);
			}
			catch (Exception /*ex*/)
			{
				throw;
			}

			var dt = new DataTester(dm, "MainObject");
			dt.AreValueEqual(45L, "Id");
			dt.AreValueEqual("MainObjectName", "Name");
			var guid = dt.GetValue<Guid>("GUID");
			var guidVal = dt.GetValue<Guid>("GuidValue");
			Assert.AreEqual(Guid.Parse("0db82076-0bec-4c5c-adbf-73A056FCCB04"), guidVal);

			var tdsub = new DataTester(dm, "MainObject.SubObject");
			tdsub.AreValueEqual(55L, "Id");
			tdsub.AreValueEqual("SubObjectName", "Name");
			tdsub.AreValueEqual(guid, "ParentGuid");

			var tdsubarray = new DataTester(dm, "MainObject.SubObject.SubArray");
			tdsubarray.IsArray(2);

			tdsubarray.AreArrayValueEqual(5, 0, "X");
			tdsubarray.AreArrayValueEqual(6, 0, "Y");
			tdsubarray.AreArrayValueEqual(5.1M, 0, "D");

			tdsubarray.AreArrayValueEqual(8, 1, "X");
			tdsubarray.AreArrayValueEqual(9, 1, "Y");
			tdsubarray.AreArrayValueEqual(7.23M, 1, "D");

			dm = await _dbContext.SaveModelAsync(null, "a2test.[NestedObject.Update]", dataToSave);
			dt = new DataTester(dm, "MainObject");
			dt.AreValueEqual(45L, "Id");
			dt.AreValueEqual("MainObjectName", "Name");
		}

		[TestMethod]
		public async Task WriteNewObject()
		{
			// DATA with ROOT
			var jsonData = @"
            {
			    MainObject: {
				    Id : 0,
				    Name: 'MainObjectName',
			    }
            }
			";
			var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
			Assert.IsNotNull(dataToSave);
			IDataModel dm = await _dbContext.SaveModelAsync(null, "a2test.[NewObject.Update]", dataToSave);

			var dt = new DataTester(dm, "MainObject");
			dt.AreValueEqual("Id is null", "Name");

			dm = await _dbContext.SaveModelAsync(null, "a2test.[NewObject.Update]", dataToSave);

			dt = new DataTester(dm, "MainObject");
			dt.AreValueEqual("Id is null", "Name");
		}

		[TestMethod]
		public async Task WriteSubObjects()
		{
			// DATA with ROOT
			var jsonData = @"
			{
				MainObject: {
					Id : 0,
					Name: 'Test Object',
					SubObject: {
						Id: 0,
						Name: 'Test Agent'
					},
					SubObjectString: {
						Id: '',
						Name: 'Test Method'
					}
				}
			}
			";
			var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
			Assert.IsNotNull(dataToSave);
			IDataModel dm = await _dbContext.SaveModelAsync(null, "a2test.[SubObjects.Update]", dataToSave);

			var dt = new DataTester(dm, "MainObject");
			dt.AreValueEqual("null", "RootId");
			dt.AreValueEqual("null", "SubId");
			dt.AreValueEqual("null", "SubIdString");

			IDataModel dm2 = await _dbContext.SaveModelAsync(null, "a2test.[SubObjects.Update]", dataToSave);
			dt = new DataTester(dm2, "MainObject");
			dt.AreValueEqual("null", "RootId");
			dt.AreValueEqual("null", "SubId");
			dt.AreValueEqual("null", "SubIdString");

			Assert.IsNotNull(dataToSave);
			dm = await _dbContext.SaveModelAsync(null, "a2test.[SubObjects.Update]", dataToSave);
			dt = new DataTester(dm, "MainObject");
			dt.AreValueEqual("null", "RootId");
		}

		[TestMethod]
		public async Task WriteJson()
		{
			// DATA with ROOT
			var jsonData = @"
			{
				MainObject: {
					Id : 0,
					Name: 'Test Object',
					SubObject: {
						Id: 112233,
						Name: 'Test Agent',
						Code: 'CODE'
					},
					SubObjectString: {
						Id: '',
						Name: 'Test Method'
					}
				}
			}
			";
			var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
			Assert.IsNotNull(dataToSave);
			IDataModel dm = await _dbContext.SaveModelAsync(null, "a2test.[Json.Update]", dataToSave);

			var dt = new DataTester(dm, "MainObject");
			dt.AreValueEqual("null", "RootId");
			dt.AreValueEqual("not null", "SubId");
			dt.AreValueEqual("null", "SubIdString");

			dt = new DataTester(dm, "MainObject.SubJson");

			dt.AreValueEqual(112233L, "Id");
			dt.AreValueEqual("Test Agent", "Name");
			dt.AreValueEqual("CODE", "Code");

			dt = new DataTester(dm, "RootJson");

			dt.AreValueEqual(112233L, "Id");
			dt.AreValueEqual("Test Agent", "Name");
			dt.AreValueEqual("CODE", "Code");

			IDataModel dm2 = await _dbContext.SaveModelAsync(null, "a2test.[Json.Update]", dataToSave);
			dt = new DataTester(dm2, "MainObject");
			dt.AreValueEqual("null", "RootId");
			dt.AreValueEqual("not null", "SubId");
			dt.AreValueEqual("null", "SubIdString");
		}

		[TestMethod]
		public async Task WriteModelWithGuids()
		{
			// DATA with ROOT
			var jsonData = @"
			{
				Document: {
					Id : 150,
					Rows: [
						{ Id: 10, Code: 'C10', SubRows: [
								{Id: 100, Code: 'SUBCODE:100'}, 
								{Id: 200, Code: 'SUBCODE:200'}
							]},
						{ Id: 20, Code: 'C20'},
					]
				}
			}
			";
			var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
			Assert.IsNotNull(dataToSave);
			IDataModel dm = await _dbContext.SaveModelAsync(null, "a2test.[Guid.Update]", dataToSave);

			var dt = new DataTester(dm, "Document");
			dt.AreValueEqual(150L, "Id");
			var guid = dt.GetValue<Guid>("GUID");
			var rows = new DataTester(dm, "Document.Rows");
			rows.IsArray(2);
			rows.AreArrayValueEqual(guid, 0, "ParentGuid");
			rows.AreArrayValueEqual(guid, 1, "ParentGuid");
			rows.AreArrayValueEqual(10L, 0, "Id");
			rows.AreArrayValueEqual(20L, 1, "Id");
			rows.AreArrayValueEqual(1, 0, "RowNo"); // 1-based
			rows.AreArrayValueEqual(2, 1, "RowNo");

			var rowguid = rows.GetArrayValue<Guid>(0, "GUID");
			Assert.AreNotEqual(guid, rowguid);
			var subrows = new DataTester(dm, "Document.Rows[0].SubRows");
			subrows.IsArray(2);
			subrows.AreArrayValueEqual(rowguid, 0, "ParentGuid");
			subrows.AreArrayValueEqual(rowguid, 1, "ParentGuid");
			subrows.AreArrayValueEqual(1, 0, "RowNo"); // 1-based
			subrows.AreArrayValueEqual(2, 1, "RowNo");
			subrows.AreArrayValueEqual(1, 0, "ParentRN"); // 1-based
			subrows.AreArrayValueEqual(1, 1, "ParentRN");

			// next time
			IDataModel dm2 = await _dbContext.SaveModelAsync(null, "a2test.[Guid.Update]", dataToSave);
			dt = new DataTester(dm2, "Document");
			dt.AreValueEqual(150L, "Id");
			rows = new DataTester(dm, "Document.Rows");
			rows.IsArray(2);
			rows.IsArray(2);
			rows.AreArrayValueEqual(guid, 0, "ParentGuid");
			rows.AreArrayValueEqual(guid, 1, "ParentGuid");
			rows.AreArrayValueEqual(10L, 0, "Id");
			rows.AreArrayValueEqual(20L, 1, "Id");
			rows.AreArrayValueEqual(1, 0, "RowNo"); // 1-based
			rows.AreArrayValueEqual(2, 1, "RowNo");

		}

		[TestMethod]
		public async Task WriteModelFallback()
		{
			// DATA with ROOT
			var jsonData = @"
			{
				Document: {
					Id : 150,
					Rows: [
						{ Id: 10, Code: 'C10', Nested: {Key: 3, Value: 'Value3'}},
						{ Id: 20, Code: 'C20', Nested: {Key: 5, Value: 'Value5'}}
					]
				}
			}
			";
			var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
			Assert.IsNotNull(dataToSave);
			IDataModel dm = await _dbContext.SaveModelAsync(null, "a2test.[Fallback.Update]", dataToSave);
			var dt = new DataTester(dm, "Document");
			dt.AreValueEqual(150L, "Id");
			var rows = new DataTester(dm, "Document.Rows");
			rows.IsArray(2);

			var nv0 = new DataTester(dm, "Document.Rows[0].Nested");
			nv0.AreValueEqual(3, "Key");
			nv0.AreValueEqual("Value3", "Value");

			var nv1 = new DataTester(dm, "Document.Rows[1].Nested");
			nv1.AreValueEqual(5, "Key");
			nv1.AreValueEqual("Value5", "Value");

			dm = await _dbContext.SaveModelAsync(null, "a2test.[Fallback.Update]", dataToSave);
			dt = new DataTester(dm, "Document");
			dt.AreValueEqual(150L, "Id");
			rows = new DataTester(dm, "Document.Rows");
			rows.IsArray(2);

			nv0 = new DataTester(dm, "Document.Rows[0].Nested");
			nv0.AreValueEqual(3, "Key");
			nv0.AreValueEqual("Value3", "Value");

			nv1 = new DataTester(dm, "Document.Rows[1].Nested");
			nv1.AreValueEqual(5, "Key");
			nv1.AreValueEqual("Value5", "Value");
		}
	}
}