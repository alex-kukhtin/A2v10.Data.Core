// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.Options;

using A2v10.Data.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
	public class DataConfigurationOptions
	{
		public String ConnectionStringName { get; set; } = "DefaultConnection";
		public TimeSpan DefaultCommandTimeout { get; set; } = TimeSpan.FromSeconds(30);
	}

	public class DataConfiguration : IDataConfiguration
	{
		private readonly IConfiguration _config;
		private readonly DataConfigurationOptions _options;


		public DataConfiguration(IConfiguration config, IOptions<DataConfigurationOptions> options)
		{
			_config = config;
			_options = options.Value;
		}

		public String ConnectionStringName => _options.ConnectionStringName;

		#region IDataConfiguration

		public TimeSpan CommandTimeout => _options.DefaultCommandTimeout;

		public String ConnectionString(String source)
		{
			if (String.IsNullOrEmpty(source))
				source = _options.ConnectionStringName;
			return _config.GetConnectionString(source);
		}
		#endregion
	}
}
