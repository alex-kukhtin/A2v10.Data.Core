// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Dynamic Types")]
public class DynamicClasses
{
	[TestMethod]
	public void SameSignatureSameType()
	{
		var t1 = ClassFactory.CreateClass([new("Id", typeof(Int32)), new("Name", typeof(String))]);
		var t2 = ClassFactory.CreateClass([new("Id", typeof(Int32)), new("Name", typeof(String))]);
		Assert.AreSame(t1, t2);
	}

	[TestMethod]
	public void DifferentSignatureDifferentType()
	{
		var t1 = ClassFactory.CreateClass([new("Id", typeof(Int32)), new("Name", typeof(String))]);
		var t2 = ClassFactory.CreateClass([new("Id", typeof(Int32)), new("Memo", typeof(String))]);
		var t3 = ClassFactory.CreateClass([new("Id", typeof(Int64)), new("Name", typeof(String))]);
		Assert.AreNotSame(t1, t2);
		Assert.AreNotSame(t1, t3);
	}
}
