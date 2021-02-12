// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;

namespace A2v10.Data.Interfaces
{
	public record BatchProcedure(String Procedure, ExpandoObject Parameters);
}
