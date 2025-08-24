// Copyright © 2015-2018 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data.Tests.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Threading.Tasks;

namespace A2v10.Data.Tests;

[TestCategory("RowVersion")]
[TestClass]
public class DatabaseRowVersion
{
	readonly IDbContext _dbContext;

	public DatabaseRowVersion()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task ReadRowVersion()
	{
		IDataModel dm = await _dbContext.LoadModelAsync(null, "a2test.[RowVersion.Load]", new ExpandoObject() {
            {"Id", 123L}                
         });
        var md = new MetadataTester(dm);
        md.IsAllKeys("TRoot,TModel");
        md.HasAllProperties("TRoot", "Model");
        md.HasAllProperties("TModel", "Name,Id,rv");
        md.IsId("TModel", "Id");
        md.IsName("TModel", "Name");
        md.IsType("TModel", "rv", DataType.String);

        var dt = new DataTester(dm, "Model");
        dt.AreValueEqual(123L, "Id");
        dt.AreValueEqual("ObjectName", "Name");
        dt.AreValueEqual("00000000000007D2", "rv");
    }

    [TestMethod]
    public async Task SaveRowVersion()
    {
        // DATA with ROOT
        var jsonData = """"
        {
            "Model": {
                "Id" : 123,
                "Name": "ObjectName",
                "rv": "00000000000007D2"
            }
        }
        """";
        var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData, new ExpandoObjectConverter());
        Assert.IsNotNull(dataToSave);
        IDataModel dm = await _dbContext.SaveModelAsync(null, "a2test.[RowVersion.Update]", dataToSave);

        var dt = new DataTester(dm, "Model");
        dt.AreValueEqual(123L, "Id");
        dt.AreValueEqual("ObjectName", "Name");
        dt.AreValueEqual("00000000000007D2", "rv");
    }
}
