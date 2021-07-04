// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data.Interfaces
{
	public interface ITenantInfo
	{
		String Procedure { get; }
		String ParamName { get; }
		Int32 TenantId { get; }
	}

	public interface ITenantManager
	{
		ITenantInfo GetTenantInfo(String source);
	}
}
