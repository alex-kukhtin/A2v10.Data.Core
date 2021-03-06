﻿// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using System;

namespace A2v10.Data.Tests.Configuration
{
	public class TestConfig : IDataConfiguration
	{

		private readonly IConfiguration _config;

		public TestConfig(IConfiguration config)
		{
			_config = config;
		}

		#region IDataConfiguration
		public String ConnectionString(String source)
		{
			if (String.IsNullOrEmpty(source))
				source = "Default";
			return _config.GetConnectionString(source);
		}

		public TimeSpan CommandTimeout => _config.GetValue<TimeSpan>("A2v10:Data:CommandTimeout");
		#endregion;
	}
}
