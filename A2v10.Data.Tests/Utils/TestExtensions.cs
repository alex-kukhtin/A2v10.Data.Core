using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace A2v10.Data.Tests
{
	public class TestExtensions
	{
		public static async Task ThrowsAsync<TException>(Func<Task> action, bool allowDerivedTypes = true)
		{
			try
			{
				await action();
				Assert.Fail("Delegate did not throw expected exception " + typeof(TException).Name + ".");
			}
			catch (Exception ex)
			{
				if (allowDerivedTypes && ex is not TException)
					Assert.Fail("Delegate threw exception of type " + ex.GetType().Name + ", but " + typeof(TException).Name + " or a derived type was expected.");
				if (!allowDerivedTypes && ex.GetType() != typeof(TException))
					Assert.Fail("Delegate threw exception of type " + ex.GetType().Name + ", but " + typeof(TException).Name + " was expected.");
			}
		}
	}
}
