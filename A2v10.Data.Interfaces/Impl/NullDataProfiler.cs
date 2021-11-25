// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Data.Interfaces;
public class NullDataProfiler : IDataProfiler
{
	public IDisposable? Start(String command)
	{
		return null;
	}
}

