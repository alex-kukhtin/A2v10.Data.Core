using System;
using System.Collections.Generic;
using System.Text;

namespace A2v10.Data.Interfaces
{
	public class NullDataProfiler : IDataProfiler
	{
		public IDisposable Start(String command)
		{
			return null;
		}
	}
}
