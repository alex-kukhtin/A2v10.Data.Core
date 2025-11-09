// Copyright © 2015-2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Metadata")]
public class DbMetadataTest
{

	[TestMethod]
	public void TypeScriptNames()
	{
        // string
		var fi = new FieldInfo("XName!!Name");
        var fm = new FieldMetadata(0, fi, DataType.String, SqlDataType.String, 100);

        Assert.AreEqual(0, fm.FieldIndex);
        Assert.AreEqual("string", fm.TypeScriptName);
		Assert.AreEqual(SqlDataType.String, fm.SqlDataType);
        Assert.AreEqual(DataType.String, fm.DataType);
        Assert.AreEqual(100, fm.Length);
        Assert.IsFalse(fm.IsJson);

        // date
        fi = new FieldInfo("DateField");
        fm = new FieldMetadata(0, fi, DataType.Date, SqlDataType.Date, 0);

        Assert.AreEqual(0, fm.FieldIndex);
        Assert.AreEqual("Date", fm.TypeScriptName);
        Assert.AreEqual(SqlDataType.Date, fm.SqlDataType);
        Assert.AreEqual(DataType.Date, fm.DataType);
        Assert.IsFalse(fm.IsJson);

        // refid
        fi = new FieldInfo("Agent!TAgent!RefId");
        fm = new FieldMetadata(10, fi, DataType.Number, SqlDataType.Bigint, 0);

        Assert.AreEqual(10, fm.FieldIndex);
        Assert.AreEqual("TAgent", fm.TypeScriptName);
        Assert.AreEqual(SqlDataType.Bigint, fm.SqlDataType);
        Assert.AreEqual(DataType.Number, fm.DataType);
        Assert.AreEqual(0, fm.Length);
        Assert.IsTrue(fm.IsRefId);
        Assert.AreEqual("TAgent", fm.RefObject);

        // array
        fi = new FieldInfo("Agent!TAgent!Array");
        fm = new FieldMetadata(0, fi, DataType.Number, SqlDataType.Bigint, 0);

        Assert.AreEqual("IElementArray<TAgent>", fm.TypeScriptName);
        Assert.AreEqual(SqlDataType.Bigint, fm.SqlDataType);
        Assert.AreEqual(DataType.Number, fm.DataType);
        Assert.AreEqual(0, fm.Length);
        Assert.IsTrue(fm.IsArrayLike);
        Assert.AreEqual("TAgent", fm.RefObject);

        // map
        fi = new FieldInfo("!TAgent!Map");
        fm = new FieldMetadata(0, fi, DataType.Number, SqlDataType.Bigint, 0);

        Assert.AreEqual("TAgent[]", fm.TypeScriptName);
        Assert.AreEqual(SqlDataType.Bigint, fm.SqlDataType);
        Assert.AreEqual(DataType.Number, fm.DataType);
        Assert.AreEqual(0, fm.Length);
        Assert.IsTrue(fm.IsArrayLike);
        Assert.AreEqual("TAgent", fm.RefObject);

        // tree
        fi = new FieldInfo("Agent!TAgent!Tree");
        fm = new FieldMetadata(0, fi, DataType.String, SqlDataType.String, 5);

        Assert.AreEqual("IElementArray<TAgent>", fm.TypeScriptName);
        Assert.AreEqual(SqlDataType.String, fm.SqlDataType);
        Assert.AreEqual(DataType.String, fm.DataType);
        Assert.AreEqual(5, fm.Length);
        Assert.IsFalse(fm.IsArrayLike);
        Assert.AreEqual("TAgent", fm.RefObject);
        Assert.AreEqual("TAgentArray", fm.GetObjectType(""));

        // json
        fi = new FieldInfo("XName!!Json");
        fm = new FieldMetadata(0, fi, DataType.String, SqlDataType.String, 100);

        Assert.AreEqual(0, fm.FieldIndex);
        Assert.AreEqual("string", fm.TypeScriptName);
        Assert.AreEqual(SqlDataType.String, fm.SqlDataType);
        Assert.AreEqual(DataType.String, fm.DataType);
        Assert.AreEqual(100, fm.Length);
        Assert.IsTrue(fm.IsJson);
        Assert.AreEqual("String", fm.GetObjectType(""));
    }
}
