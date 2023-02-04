// Copyright © 2015-2023 Alex Kukhtin. All rights reserved.

using A2v10.Data.Tests;
using A2v10.Data.Tests.Configuration;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Dynamic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace A2v10.Data.Models
{
	[TestClass]
	[TestCategory("AllowEmptyStrings")]
	public class AllowEmptyStrings
	{
		readonly IDbContext _dbContext;
		public AllowEmptyStrings()
		{
			var dc = new DataConfigurationOptions() { 
				AllowEmptyStrings = true 
			};
			_dbContext = Starter.Create(dc);
		}

		[TestMethod]
		public async Task WriteModelEmptyStrings()
		{
			// empty
			{
				var jsonData = @"
				{
					Document: {
						Id : 150,
						Name: ''
					}
				}
				";
				var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
				Assert.IsNotNull(dataToSave);
				IDataModel dm = await _dbContext.SaveModelAsync(null, "a2test.[EmptyString.Update]", dataToSave);
				var dt = new DataTester(dm, "Document");
				dt.AreValueEqual(150L, "Id");
				dt.AreValueEqual("", "Name");
				dt.AreValueEqual("EMPTY", "String");
			}
			// null
			{
				var jsonData = @"
				{
					Document: {
						Id : 150,
					}
				}
				";
				var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
				Assert.IsNotNull(dataToSave);
				IDataModel dm = await _dbContext.SaveModelAsync(null, "a2test.[EmptyString.Update]", dataToSave);
				var dt = new DataTester(dm, "Document");
				dt.AreValueEqual(150L, "Id");
				dt.IsNull("Name");
				dt.AreValueEqual("NULL", "String");
			}
		}

	}
}
