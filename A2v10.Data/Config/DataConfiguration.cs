// Copyright © 2020 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using System;

namespace A2v10.Data.Config
{
	public class DataConfiguration : IDataConfiguration
	{
		private readonly IConfiguration _config;
		public DataConfiguration(IConfiguration config)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
		}

		#region IDataConfiguration

		public TimeSpan CommandTimeout => _config.GetValue<TimeSpan>("A2v10:Data:CommandTimeout");

		public string ConnectionString(string source)
		{
			if (String.IsNullOrEmpty(source))
				source = "DefaultConnection";
			return _config.GetConnectionString(source);
		}
		#endregion
	}
}
