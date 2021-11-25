// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using System;
using System.Collections.Generic;

namespace A2v10.Data.Tests.Configuration
{
	public class TestLocalizer : IDataLocalizer
	{
		private readonly IDictionary<String, String> _dict;

		public TestLocalizer()
		{
			_dict = new Dictionary<String, String>()
			{
				{ "@[Item1]", "Item 1" },
				{ "@[Item2]", "Item 2" },
				{ "@[Item3]", "Item 3" },
			};
		}

		#region IDataLocalizer
		public String Localize(String content)
		{
			if (_dict.TryGetValue(content, out String? outValue))
				return outValue;
			return content;
		}
		#endregion
	}
}
