// Copyright © 2021 Alex Kukhtin. All rights reserved.


using A2v10.Data.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace A2v10.Data.Tests
{
	public class DummyConfiguration : IConfiguration
	{
		public string this[string key] { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

		public IEnumerable<IConfigurationSection> GetChildren()
		{
			throw new System.NotImplementedException();
		}

		public IChangeToken GetReloadToken()
		{
			throw new System.NotImplementedException();
		}

		public IConfigurationSection GetSection(string key)
		{
			throw new System.NotImplementedException();
		}
	}

	[TestClass]
	[TestCategory("Data Configuration")]
	public class TestDataConfiguration
	{

		[TestMethod]
		public void DefaultConfig()
		{
			var dc = new DataConfiguration(new DummyConfiguration());
			Assert.AreEqual("DefaultConnection", dc.ConnectionStringName);
		}

		[TestMethod]
		public void ConfigWithName()
		{
			var dc = new DataConfiguration(new DummyConfiguration(), opts =>
			{
				opts.ConnectionStringName = "MyName";
			});
			Assert.AreEqual("MyName", dc.ConnectionStringName);
		}
	}
}
