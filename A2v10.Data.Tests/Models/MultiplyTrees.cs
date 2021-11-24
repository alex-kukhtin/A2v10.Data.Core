
using A2v10.Data.Interfaces;
using A2v10.Data.Tests;
using A2v10.Data.Tests.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace A2v10.Data.Models
{
	[TestClass]
	[TestCategory("Sub Tree")]
	public class MultiplyTrees
	{
		private readonly IDbContext _dbContext;
		public MultiplyTrees()
		{
			_dbContext = Starter.Create();
		}

		[TestMethod]
		public async Task LoadMultiplyTreeModel()
		{
			var dm = await _dbContext.LoadModelAsync(null, "a2test.[MultiplyTrees.Load]");


			/*var json =*/ JsonConvert.SerializeObject(dm.Root);

			var md = new MetadataTester(dm);
			md.IsAllKeys("TRoot,TModel,TElem");
			md.HasAllProperties("TRoot", "Model");
			md.HasAllProperties("TModel", "Id,Name,Elements");
			md.HasAllProperties("TElem", "Id,Name,TreeItems");

			md.IsId("TModel", "Id");
			md.IsId("TElem", "Id");
			md.IsItems("TElem", "TreeItems");

			var dt = new DataTester(dm);
			dt.AllProperties("Model");

			dt = new DataTester(dm, "Model");
			dt.IsArray(2);
			dt.AreArrayValueEqual(5, 0, "Id");
			dt.AreArrayValueEqual(7, 1, "Id");

			dt = new DataTester(dm, "Model[0].Elements");
			dt.IsArray(2);
			dt.AreArrayValueEqual(50, 0, "Id");
			dt.AreArrayValueEqual(51, 1, "Id");
			dt.AreArrayValueEqual("5-50", 0, "Name");
			dt.AreArrayValueEqual("5-51", 1, "Name");

			dt = new DataTester(dm, "Model[0].Elements[0].TreeItems");
			dt.IsArray(2);
			dt.AreArrayValueEqual(500, 0, "Id");
			dt.AreArrayValueEqual("5-50-500", 0, "Name");
			dt.AreArrayValueEqual(510, 1, "Id");
			dt.AreArrayValueEqual("5-50-510", 1, "Name");

			dt = new DataTester(dm, "Model[1].Elements");
			dt.IsArray(1);
			dt.AreArrayValueEqual(70, 0, "Id");
			dt.AreArrayValueEqual("7-70", 0, "Name");

			dt = new DataTester(dm, "Model[1].Elements[0].TreeItems");
			dt.IsArray(1);
			dt.AreArrayValueEqual(700, 0, "Id");
			dt.AreArrayValueEqual("7-70-700", 0, "Name");

			dt = new DataTester(dm, "Model[1].Elements[0].TreeItems[0].TreeItems");
			dt.IsArray(1);
			dt.AreArrayValueEqual(710, 0, "Id");
			dt.AreArrayValueEqual("7-70-700-710", 0, "Name");
		}

		[TestMethod]
		public async Task LoadChildrenTreeModel()
		{
			var dm = await _dbContext.LoadModelAsync(null, "a2test.[ChildrenTree.Load]");


			/*var json* =*/ JsonConvert.SerializeObject(dm.Root);

			var md = new MetadataTester(dm);
			md.IsAllKeys("TRoot,TModel,TElem");
			md.HasAllProperties("TRoot", "Model");
			md.HasAllProperties("TModel", "Id,Name,Elements");
			md.HasAllProperties("TElem", "Id,Name,TreeItems");

			md.IsId("TModel", "Id");
			md.IsId("TElem", "Id");
			md.IsItems("TElem", "TreeItems");

			var dt = new DataTester(dm);
			dt.AllProperties("Model");

			dt = new DataTester(dm, "Model");
			dt.AreValueEqual(5, "Id");

			dt = new DataTester(dm, "Model.Elements");
			dt.IsArray(2);
			dt.AreArrayValueEqual(50, 0, "Id");
			dt.AreArrayValueEqual(51, 1, "Id");
			dt.AreArrayValueEqual("5-50", 0, "Name");
			dt.AreArrayValueEqual("5-51", 1, "Name");

			dt = new DataTester(dm, "Model.Elements[0].TreeItems");
			dt.IsArray(2);
			dt.AreArrayValueEqual(500, 0, "Id");
			dt.AreArrayValueEqual("5-50-500", 0, "Name");
			dt.AreArrayValueEqual(510, 1, "Id");
			dt.AreArrayValueEqual("5-50-510", 1, "Name");

			dt = new DataTester(dm, "Model.Elements[0].TreeItems[1].TreeItems");
			dt.IsArray(1);
			dt.AreArrayValueEqual(5100, 0, "Id");
		}
	}
}
