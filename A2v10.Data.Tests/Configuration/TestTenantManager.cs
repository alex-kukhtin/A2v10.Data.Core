using A2v10.Data.Interfaces;
// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data.Tests.Configuration
{
	public record TenantInfo : ITenantInfo
	{
		public String Procedure => "a2test.[SetTenantId]";
		public String ParamName => "@TenantId";
		public Int32 TenantId => 123;
	}

	public class TestTenantManager : ITenantManager
	{
		public ITenantInfo GetTenantInfo(String source)
		{
			return new TenantInfo();
		}
	}
}
