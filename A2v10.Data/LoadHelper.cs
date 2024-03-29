﻿// Copyright © 2015-2023 Oleksandr  Kukhtin. All rights reserved.

using System.Data;

namespace A2v10.Data;

public class LoadHelper<T> : LoadHelperBase<T> where T : class
{
	public T ProcessData(IDataReader rdr)
	{
		return CreateInstance(rdr);
	}
}
