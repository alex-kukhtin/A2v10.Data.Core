// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;
using System.Threading.Tasks;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Write From Json")]
public class WriteFromJson
{
	private readonly IDbContext _dbContext;

	public WriteFromJson()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public Task WriteModelNormal()
	{
		// DATA with ROOT
		var jsonData = @"
            {
			    MainObject: {
				    Int32Value : 45,
					Int64Value : 33344,
				    StringValue: 'MainObjectName',
				    MoneyValue : 531.55,
					FloatValue: 2233.33445,
				    BitValue : true,
					GuidValue: '0db82076-0bec-4c5c-adbf-73A056FCCB04',
					DateTimeValue: '2022-04-30T00:00:00Z'
				}
            }
			";
		return TestAsync(jsonData);
	}

	[TestMethod]
	public Task WriteModelFromJson()
	{
		// DATA with ROOT
		var jsonData = @"
            {
			    MainObject: {
				    Int32Value : '45',
					Int64Value : '33344',
				    StringValue: 'MainObjectName',
				    MoneyValue : '531.55',
					FloatValue: '2233.33445',
				    BitValue : 'true',
					GuidValue: '0db82076-0bec-4c5c-adbf-73A056FCCB04',
					DateTimeValue: '2022-04-30T00:00:00Z'
				}
            }
			";
		return TestAsync(jsonData);
	}

	private async Task TestAsync(String jsonData)
	{
		IDataModel? dm = null;
		var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
		if (dataToSave == null)
			throw new InvalidOperationException("Data is null");
		try
		{
			dm = await _dbContext.SaveModelAsync(null, "a2test.[ScalarTypes.Update]", dataToSave);
		}
		catch (Exception /*ex*/)
		{
			throw;
		}

		var dt = new DataTester(dm, "MainObject");
		dt.AreValueEqual(45, "Int32Value");
		dt.AreValueEqual((Int64)33344, "Int64Value");
		dt.AreValueEqual("MainObjectName", "StringValue");
		dt.AreValueEqual(531.55M, "MoneyValue");
		dt.AreValueEqual(2233.33445, "FloatValue");
		dt.AreValueEqual(true, "BitValue");
		var guidVal = dt.GetValue<Guid>("GuidValue");
		Assert.AreEqual(Guid.Parse("0db82076-0bec-4c5c-adbf-73A056FCCB04"), guidVal);
		var dateVal = dt.GetValue<DateTime>("DateTimeValue");
		Assert.AreEqual(new DateTime(2022, 04, 30), dateVal);
	}

    [TestMethod]
    public async Task WriteModelNulls()
    {
        // DATA with ROOT
        var jsonData = @"
            {
			    MainObject: {
				    StringValue: null,
				}
            }
			";
        var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
        var dm = await _dbContext.SaveModelAsync(null, "a2test.[ScalarTypes.Update]", dataToSave!);

        var dt = new DataTester(dm, "MainObject");
        dt.IsNull("Int32Value");
        dt.IsNull("Int64Value");
        dt.IsNull("StringValue");
        dt.IsNull("MoneyValue");
        dt.IsNull("FloatValue");
        dt.IsNull("BitValue");
        dt.IsNull("GuidValue");
        dt.IsNull("DateTimeValue");

        jsonData = @"
            {
			    MainObject: {
				    StringValue: '',
				}
            }
			";
        dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
        dm = await _dbContext.SaveModelAsync(null, "a2test.[ScalarTypes.Update]", dataToSave!);
        dt = new DataTester(dm, "MainObject");
        dt.IsNull("StringValue");
    }
}
