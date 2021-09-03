// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using A2v10.Data.Interfaces;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests
{
	[TestClass]
	[TestCategory("Expando objects")]
	public class DatabaseExpando
	{
		readonly IDbContext _dbContext;

		public DatabaseExpando()
		{
			_dbContext = Starter.Create();
		}

		[TestMethod]
		public async Task ExpandoTable()
		{
			// DATA with ROOT
			var jsonData = @"
{
	Id : 45,
	Name: 'RootName',
	Elements : [
		{
			Id: 55,
			Name: 'Elem 55',
			Bool: true
		},
		{
			Id: 56,
			Name: 'Elem 56',
			Bool: false
		},
		{
			Id: 57,
			Name: 'Elem 57',
			Bool: true
		}
	]
}
";
			var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());

			var eo = await _dbContext.ReadExpandoAsync(null, "a2test.[Expando.Tables]", dataToSave);

			Assert.AreEqual(45, eo.Get<Int64>("Id"));
			Assert.AreEqual("RootName", eo.Get<String>("Name"));
			Assert.AreEqual(3, eo.Get<Int32>("Count"));
			Assert.AreEqual(55 + 56 + 57, eo.Get<Int64>("Sum"));
			Assert.AreEqual("Elem 55,Elem 56,Elem 57", eo.Get<String>("Text"));
		}
	}
}
