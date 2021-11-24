﻿// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Data;
public static class PathHelpers
{
	public static String AppendDot(this String This, String append)
	{
		if (String.IsNullOrEmpty(This))
			return append;
		return This + '.' + append;
	}
}

