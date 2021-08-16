// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Data.Interfaces;
using A2v10.Data.Tests.Configuration;


namespace A2v10.Data.Tests
{
	public enum Severity
	{
		None,
		Warning,
		Error,
		Info
	}

	public class ListItem
	{
		public String StringValue { get; set; }
		public Int32 Int32Value { get; set; }
		public Int32? Int32ValueNull { get; set; }
		public Severity Severity { get; set; }
		public Severity? SeverityNull { get; set; }
	}

	public record BinaryItem
	{
		public String Name { get; set; }
		public Stream Stream { get; set; }
		public Byte[] ByteArray { get; set; }
		public Stream StreamNull { get; set; }
		public Byte[] ByteArrayNull { get; set; }
		public Int64 Length { get; set; }
	}

	[TestClass]
	[TestCategory("Clr Types")]
	public class ClrTypes
	{
		private IDbContext _dbContext;

		[TestInitialize]
		public void Setup()
		{
			_dbContext = Starter.Create();
		}

		[TestMethod]
		public async Task LoadAsync()
		{
			var item = await _dbContext.LoadAsync<ListItem>(null, "a2test.[ClrTypes.LoadListItem]", null);

			Assert.AreEqual("String 1", item.StringValue);
			Assert.AreEqual(22, item.Int32Value);
			Assert.IsNull(item.Int32ValueNull);
			Assert.AreEqual(Severity.Warning, item.Severity);
			Assert.IsNull(item.SeverityNull);
		}

		[TestMethod]
		public async Task LoadListAsync()
		{
			var list = await _dbContext.LoadListAsync<ListItem>(null, "a2test.[ClrTypes.LoadListItemList]", null);

			Assert.AreEqual(3, list.Count);

			Assert.AreEqual("String 1", list[0].StringValue);
			Assert.AreEqual(22, list[0].Int32Value);
			Assert.AreEqual(22, list[0].Int32ValueNull);
			Assert.AreEqual(Severity.Warning, list[0].Severity);
			Assert.AreEqual(Severity.Info, list[0].SeverityNull);

			Assert.AreEqual("String 2", list[1].StringValue);
			Assert.AreEqual(33, list[1].Int32Value);
			Assert.AreEqual(33, list[1].Int32ValueNull);
			Assert.AreEqual(Severity.Error, list[1].Severity);
			Assert.IsNull(list[1].SeverityNull);

			Assert.IsNull(list[2].StringValue);
			Assert.AreEqual(0, list[2].Int32Value);
			Assert.IsNull(list[2].Int32ValueNull);
			Assert.AreEqual(Severity.None, list[2].Severity);
			Assert.IsNull(list[2].SeverityNull);
		}

		[TestMethod]
		public void LoadList()
		{
			var list = _dbContext.LoadList<ListItem>(null, "a2test.[ClrTypes.LoadListItemList]", null);

			CheckList(list);
		}

		private static void CheckList(IList<ListItem> list)
		{
			Assert.AreEqual(3, list.Count);

			Assert.AreEqual("String 1", list[0].StringValue);
			Assert.AreEqual(22, list[0].Int32Value);
			Assert.AreEqual(22, list[0].Int32ValueNull);
			Assert.AreEqual(Severity.Warning, list[0].Severity);
			Assert.AreEqual(Severity.Info, list[0].SeverityNull);

			Assert.AreEqual("String 2", list[1].StringValue);
			Assert.AreEqual(33, list[1].Int32Value);
			Assert.AreEqual(33, list[1].Int32ValueNull);
			Assert.AreEqual(Severity.Error, list[1].Severity);
			Assert.IsNull(list[1].SeverityNull);

			Assert.IsNull(list[2].StringValue);
			Assert.AreEqual(0, list[2].Int32Value);
			Assert.IsNull(list[2].Int32ValueNull);
			Assert.AreEqual(Severity.None, list[2].Severity);
			Assert.IsNull(list[2].SeverityNull);
		}

		[TestMethod]
		public void LoadVarBinary()
		{
			var item = _dbContext.Load<BinaryItem>(null, "a2test.[ClrTypes.LoadBinary]", null);

			Assert.AreEqual("VarBinary", item.Name);
			Assert.AreEqual(item.Length, item.ByteArray.Length);
			Assert.AreEqual(item.Length, item.Stream.Length);
			Assert.AreEqual(0, item.Stream.Position);
			Assert.IsNull(item.StreamNull);
			Assert.IsNull(item.ByteArrayNull);
		}
	}
}
