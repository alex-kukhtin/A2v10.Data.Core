// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;
using A2v10.Data.Interfaces;

namespace A2v10.Data.Tests.Configuration
{
	public class TestConfig : IDataConfiguration
	{
		public String ConnectionString(String source)
		{
			/*
            if (String.IsNullOrEmpty(source))
                source = "Default";
            var cnnStr = ConfigurationManager.ConnectionStrings[source];
            if (cnnStr == null)
                throw new ConfigurationErrorsException($"Connection string '{source}' not found");
            return cnnStr.ConnectionString;
            */
			return "ERROR HERE";
		}

		public TimeSpan CommandTimeout { get; }

	}
}
