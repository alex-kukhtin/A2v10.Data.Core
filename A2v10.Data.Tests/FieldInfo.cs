// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data.Tests
{
	[TestClass]
	[TestCategory("Field Info")]
	public class TestFieldInfo
	{
		[TestMethod]
		public void IderRegEx()
		{
			new FieldInfo("ІІІ", "TMap").CheckTypeName();
            new FieldInfo("Приклад", "TЕлемент234_2334").CheckTypeName();
            new FieldInfo("Приклад", "_$TЕлемент_$$232").CheckTypeName();
            Assert.Throws<DataLoaderException>(() => new FieldInfo("Test", "212%%").CheckTypeName());
            Assert.Throws<DataLoaderException>(() => new FieldInfo("Test", "2T$").CheckTypeName());
        }
    }
}
