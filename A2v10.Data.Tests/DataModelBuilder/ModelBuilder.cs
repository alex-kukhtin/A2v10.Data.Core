// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Data.Tests;

/* TODO:
1. Add Array
2. Add Map
 */

[TestClass]
[TestCategory("DataModelBuilder")]
public class ModelBuilder
{

	public ModelBuilder()
	{
	}

	[TestMethod]
	public void SimpleModel()
	{
		var root = new ExpandoObject()
		{
			{"MainObject", new ExpandoObject()
				{
					{ "Int32Value", 45 },
					{ "Int64Value", 33344L },
					{ "StringValue", "MainObjectName" },
					{ "MoneyValue", 531.55M },
					{ "FloatValue", 2233.33445 },
					{ "BitValue", true },
					{ "GuidValue", new Guid("0db82076-0bec-4c5c-adbf-73A056FCCB04") },
					{ "DateTimeValue", DateTime.Parse("2022-04-30T00:00:00Z") }
				}
			}
		};
		var dmb = new DataModelBuilder();
		var tRoot = dmb.AddMetadata("TRoot");
		tRoot.AddField("MainObject", "MainObject");

		var tMain = dmb.AddMetadata("TMainObject");
		tMain.AddField("Int32Value", SqlDataType.Int)
		.AddField("Int64Value", SqlDataType.Bigint)
		.AddField("MoneyValue", SqlDataType.Currency)
		.AddField("FloatValue", SqlDataType.Float)
		.AddField("StringValue", SqlDataType.String, 255)
		.SetId("Int32Value")
		.SetName("StringValue");

		var dm = dmb.CreateDataModel(root);

		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TMainObject");
		md.HasAllProperties("TRoot", "MainObject");
		md.HasAllProperties("TMainObject", "Int32Value,Int64Value,MoneyValue,FloatValue,StringValue");
		md.IsId("TMainObject", "Int32Value");
		md.IsName("TMainObject", "StringValue");

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
		Assert.AreEqual((new DateTime(2022, 04, 30)).ToLocalTime(), dateVal);
	}
}
