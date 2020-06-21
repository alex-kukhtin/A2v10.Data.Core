using System;

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
