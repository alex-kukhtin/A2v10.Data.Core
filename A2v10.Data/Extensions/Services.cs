// Copyright © 2020 Alex Kukhtin. All rights reserved.

using Microsoft.Extensions.DependencyInjection;

using A2v10.Data.Interfaces;
using A2v10.Data.Config;

namespace A2v10.Data.Extensions
{
	public static class Services
	{
		public static void UseSimpleDbContext(this IServiceCollection services) 
		{
			services.AddSingleton<IDataConfiguration, DataConfiguration>();
			services.AddSingleton<IDataProfiler, NullDataProfiler>();
			services.AddSingleton<IDataLocalizer, NullDataLocalizer>();
			services.AddSingleton<IDbContext, SqlDbContext>();
		}
	}
}
