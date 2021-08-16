// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.


using System.Collections.Generic;
using System.Data;

namespace A2v10.Data
{
	public class ListLoader<T> : LoadHelperBase<T> where T : class
	{
		public List<T> Result;

		public ListLoader()
			: base()
		{
			Result = new List<T>();
		}

		public void ProcessData(IDataReader rdr)
		{
			T item = CreateInstance(rdr);
			Result.Add(item);
		}
	}
}
