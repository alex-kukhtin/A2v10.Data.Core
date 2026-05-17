// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("SqlExtensions")]
public class TestSqlExtensions
{
	[TestMethod]
	public void ChangeType()
	{
		Assert.AreEqual(5, SqlExtensions.ConvertTo("5", typeof(Int32), true, "ColumnName"));
		Assert.AreEqual(5L, SqlExtensions.ConvertTo(new ExpandoObject() { { "Id", 5 } }, typeof(Int64), true, "ColumnName"));

		Assert.Throws<InvalidOperationException>(() => {
			SqlExtensions.ConvertTo(new List<ExpandoObject>(), typeof(Int32), true, "ColumnName");
		});
    }
}
