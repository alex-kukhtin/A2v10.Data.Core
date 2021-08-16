// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

using Microsoft.Extensions.Configuration;

using A2v10.Data.Interfaces;

namespace A2v10.Data.Config
{
	public class DataConfigurationOptions
	{
		public String ConnectionStringName { get; set; }
	}

	public class DataConfiguration : IDataConfiguration
	{
		private readonly IConfiguration _config;

		private readonly String _connectionStringName;

		const String DefaultConnectionStringName = "DefaultConnection";

		public DataConfiguration(IConfiguration config, Action<DataConfigurationOptions> options = null)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
			_connectionStringName = DefaultConnectionStringName;
			if (options != null)
			{
				var configOptions = new DataConfigurationOptions()
				{
					ConnectionStringName = DefaultConnectionStringName
				};
				options(configOptions);
				_connectionStringName = configOptions.ConnectionStringName;
			}
		}

		public String ConnectionStringName => _connectionStringName;

		#region IDataConfiguration

		public TimeSpan CommandTimeout => _config.GetValue<TimeSpan>("A2v10:Data:CommandTimeout");

		public String ConnectionString(String source)
		{
			if (String.IsNullOrEmpty(source))
				source = _connectionStringName;
			return _config.GetConnectionString(source);
		}
		#endregion
	}
}
