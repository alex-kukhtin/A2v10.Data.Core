// Copyright © 2021-2026 Oleksandr Kukhtin. All rights reserved.

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

public record MissingTenantInfo : ITenantInfo
{
	public String Procedure => "a2test.[SetTenantId.Missing]";

	public IEnumerable<TenantInfoParam> Params => [];
}

// the tenant procedure fails on every connection
public class MissingTenantManager : ITenantManager
{
	public ITenantInfo? GetTenantInfo(String? source)
	{
		return new MissingTenantInfo();
	}
}
