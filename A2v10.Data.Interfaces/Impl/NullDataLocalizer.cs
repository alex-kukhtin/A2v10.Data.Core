// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Data.Interfaces;
public class NullDataLocalizer : IDataLocalizer
{
	public String? Localize(String? content)
	{
		return content;
	}
}

