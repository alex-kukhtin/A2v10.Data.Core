// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.


using A2v10.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace A2v10.Data.Tests.Configuration
{

	public static class Starter
	{
		public static void Init()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}

		public static IDbContext Create()
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddUserSecrets<TestConfig>()
				.Build();


			Init();
			IDataProfiler profiler = new TestProfiler();
			IDataConfiguration config = new TestConfig(configuration);
			IDataLocalizer localizer = new TestLocalizer();
			return new SqlDbContext(profiler, config, localizer);
		}
	}
}
