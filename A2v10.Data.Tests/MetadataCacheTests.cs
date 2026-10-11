// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Metadata Cache")]
public class MetadataCacheTests
{
	static MetadataCache CreateCache(Boolean disabled)
	{
		var config = new DataConfiguration(new ConfigurationBuilder().Build(),
			Options.Create(new DataConfigurationOptions() { DisableWriteMetadataCaching = disabled }));
		return new MetadataCache(config);
	}

	[TestMethod]
	public void WriterMetadataCached()
	{
		var cache = CreateCache(disabled: false);
		var wm = new WriterMetadata();
		cache.AddWriterMetadata("a2test.[Command.Metadata]", wm);
		Assert.AreSame(wm, cache.GetWriterMetadata("a2test.[Command.Metadata]"));
	}

	[TestMethod]
	public void WriterMetadataNotCachedWhenDisabled()
	{
		var cache = CreateCache(disabled: true);
		cache.AddWriterMetadata("a2test.[Command.Metadata]", new WriterMetadata());
		Assert.IsNull(cache.GetWriterMetadata("a2test.[Command.Metadata]"));
	}

	[TestMethod]
	public async Task SaveModelWhenDisabled()
	{
		var dbContext = Starter.Create(new DataConfigurationOptions() { DisableWriteMetadataCaching = true });
		var jsonData = @"
			{
				MainObject: {
					Id : 45,
					Name: 'MainObjectName',
					SubObject : {
						Id: 55,
						Name: 'SubObjectName',
						SubArray: [
							{X: 5, Y:6, D:5.1 }
						]
					}
				}
			}
			";
		var dataToSave = JsonConvert.DeserializeObject<ExpandoObject>(jsonData.Replace('\'', '"'), new ExpandoObjectConverter());
		Assert.IsNotNull(dataToSave);

		// the second call reads metadata and derives parameters again
		for (Int32 i = 0; i < 2; i++)
		{
			var dm = await dbContext.SaveModelAsync(null, "a2test.[NestedObject.Update]", dataToSave);
			var dt = new DataTester(dm, "MainObject");
			dt.AreValueEqual(45L, "Id");
			dt.AreValueEqual("MainObjectName", "Name");
			var tdsubarray = new DataTester(dm, "MainObject.SubObject.SubArray");
			tdsubarray.IsArray(1);
			tdsubarray.AreArrayValueEqual(5, 0, "X");
		}
	}
}
