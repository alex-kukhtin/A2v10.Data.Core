// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.


using A2v10.Data.Interfaces;

namespace A2v10.Data.Tests.Configuration
{

	public static class Starter
	{
		public static IDbContext Create()
		{
			IDataProfiler profiler = new TestProfiler();
			IDataConfiguration config = new TestConfig();
			IDataLocalizer localizer = new TestLocalizer();
			return new SqlDbContext(profiler, config, localizer);
		}
	}
}
