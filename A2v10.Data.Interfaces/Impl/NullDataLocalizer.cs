// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data.Interfaces
{
	public class NullDataLocalizer : IDataLocalizer
	{
		public String Localize(String content)
		{
			return content;
		}
	}
}
