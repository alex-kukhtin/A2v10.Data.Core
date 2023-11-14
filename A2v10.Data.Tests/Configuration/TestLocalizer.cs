// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Data.Tests.Configuration
{
	public class TestLocalizer : IDataLocalizer
	{
		private readonly Dictionary<String, String> _dict;

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
		public String? Localize(String? content)
		{
			if (content == null)
				return null;
			if (_dict.TryGetValue(content, out String? outValue))
				return outValue;
			return content;
		}
		#endregion
	}
}
