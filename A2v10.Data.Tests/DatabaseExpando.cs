// Copyright © 2021 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

			Assert.IsNotNull(eo);
			Assert.AreEqual(45, eo.Get<Int64>("Id"));
			Assert.AreEqual("RootName", eo.Get<String>("Name"));
			Assert.AreEqual(3, eo.Get<Int32>("Count"));
			Assert.AreEqual(55 + 56 + 57, eo.Get<Int64>("Sum"));
			Assert.AreEqual("Elem 55,Elem 56,Elem 57", eo.Get<String>("Text"));

			eo = await _dbContext.ReadExpandoAsync(null, "a2test.[Expando.Tables]", dataToSave);
			Assert.IsNotNull(eo);
			Assert.AreEqual(45, eo.Get<Int64>("Id"));
			Assert.AreEqual("RootName", eo.Get<String>("Name"));
			Assert.AreEqual(3, eo.Get<Int32>("Count"));
			Assert.AreEqual(55 + 56 + 57, eo.Get<Int64>("Sum"));
			Assert.AreEqual("Elem 55,Elem 56,Elem 57", eo.Get<String>("Text"));
		}


		[TestMethod]
		public void ReadExpando()
		{
			var prms = new ExpandoObject()
			{
				{"Id", 55 },
				{"Name", "Name" },
				{"Number", 22.5 }
			};
			var eo = _dbContext.ReadExpando(null, "a2test.[Expando.Simple]", prms);
			Assert.IsNotNull(eo);
			Assert.AreEqual(55, eo.Get<Int64>("Id"));
			Assert.AreEqual("Name", eo.Get<String>("Name"));
			Assert.AreEqual(22.5, eo.Get<Double>("Number"));

			// second time
			eo = _dbContext.ReadExpando(null, "a2test.[Expando.Simple]", prms);
			Assert.IsNotNull(eo);
			Assert.AreEqual(55, eo.Get<Int64>("Id"));
			Assert.AreEqual("Name", eo.Get<String>("Name"));
			Assert.AreEqual(22.5, eo.Get<Double>("Number"));
		}

		[TestMethod]
		public async Task ReadExpandoAsync()
		{
			var prms = new ExpandoObject()
			{
				{"Id", 55 },
				{"Name", "Name" },
				{"Number", 22.5 }
			};
			var eo = await _dbContext.ReadExpandoAsync(null, "a2test.[Expando.Simple]", prms);
			Assert.IsNotNull(eo);
			Assert.AreEqual(55, eo.Get<Int64>("Id"));
			Assert.AreEqual("Name", eo.Get<String>("Name"));
			Assert.AreEqual(22.5, eo.Get<Double>("Number"));
			
			// second time
			eo = await _dbContext.ReadExpandoAsync(null, "a2test.[Expando.Simple]", prms);
			Assert.IsNotNull(eo);
			Assert.AreEqual(55, eo.Get<Int64>("Id"));
			Assert.AreEqual("Name", eo.Get<String>("Name"));
			Assert.AreEqual(22.5, eo.Get<Double>("Number"));
		}
	}
}
