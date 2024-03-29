﻿// Copyright © 2021-2024 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data.Tests.Configuration;

public record TenantInfo : ITenantInfo
{
	public String Procedure => "a2test.[SetTenantId]";

	public IEnumerable<TenantInfoParam> Params =>
		[
			new("@TenantId", 123)
		];
}

public class TestTenantManager : ITenantManager
{
	public ITenantInfo? GetTenantInfo(String? source)
	{
		return new TenantInfo();
	}
}
