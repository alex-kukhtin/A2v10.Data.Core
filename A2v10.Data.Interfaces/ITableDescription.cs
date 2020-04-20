
using System;
using System.Dynamic;

namespace A2v10.Data.Interfaces
{
	public interface ITableDescription
	{
		ExpandoObject NewRow();
		void SetValue(ExpandoObject obj, String propName, Object value);
		ExpandoObject ToObject();
	}
}
