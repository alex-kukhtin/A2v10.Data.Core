// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Data Helpers")]
public class DataHelperTests
{
	[TestMethod]
	public void IdIsNull()
	{
		Assert.IsTrue(DataHelpers.IsIdIsNull(null));
		Assert.IsTrue(DataHelpers.IsIdIsNull((Int64)0));
		Assert.IsTrue(DataHelpers.IsIdIsNull((Int32)0));
		Assert.IsTrue(DataHelpers.IsIdIsNull((Int16)0));
		Assert.IsTrue(DataHelpers.IsIdIsNull(""));
		Assert.IsTrue(DataHelpers.IsIdIsNull((String?)null));
		Assert.IsFalse(DataHelpers.IsIdIsNull(-(Int64)1));
		Assert.IsFalse(DataHelpers.IsIdIsNull((Int32)7));
		Assert.IsFalse(DataHelpers.IsIdIsNull((Int16)22));
		Assert.IsFalse(DataHelpers.IsIdIsNull("s"));
	}

	[TestMethod]
	public void DeserializeJson()
	{
		String? arg = null;
		Assert.IsNull(DataHelpers.DeserializeJson(arg));
		arg = """{"x": 5, "text": "string"}""";
		var eo = DataHelpers.DeserializeJson(arg);
		Assert.IsNotNull(eo);
		Assert.AreEqual(5, eo.Get<Int64>("x"));
		Assert.AreEqual(5, eo.GetConvert<Int32>("x"));
		Assert.AreEqual(5, eo.GetConvert<Int16>("x"));
		Assert.AreEqual("5", eo.GetConvert<String>("x"));
		Assert.AreEqual("string", eo.Get<String?>("text"));
	}

	[TestMethod]
	public void GetConvert()
	{
		var eo = new ExpandoObject()
		{
			{ "x", 5 },
			{ "y", null }
		};
		Assert.IsNotNull(eo);
		Assert.AreEqual(5, eo.GetConvert<Int64>("x"));
		Assert.AreEqual(5, eo.GetConvert<Int32>("x"));
		Assert.AreEqual(5, eo.GetConvert<Int16>("x"));
		Assert.AreEqual("5", eo.GetConvert<String>("x"));
		Assert.AreEqual(0, eo.GetConvert<Int64>("y"));
		Assert.IsNull(eo.GetConvert<String>("y"));
	}

	[TestMethod]
	public void IsExpandoEmpty()
	{
		var eo = new ExpandoObject()
		{
			{ "x", 5 },
			{ "y", null }
		};
		Assert.IsFalse(eo.IsEmpty());
		eo = [];
		Assert.IsTrue(eo.IsEmpty());
	}
}
