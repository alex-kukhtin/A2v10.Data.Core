// Copyright © 2018-2019 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data.Interfaces
{
	[Flags]
	public enum StdPermissions
	{
		None = 0,
		CanView = 0x01,
		CanEdit = 0x02,
		CanDelete = 0x04,
		CanApply = 0x08,
		CanUnapply = 0x10
	}
}
