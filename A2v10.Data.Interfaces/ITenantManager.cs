// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Data.Interfaces;

public record TenantInfoParam(String ParamName, Object? Value);

public interface ITenantInfo
{
	String Procedure { get; }
	IEnumerable<TenantInfoParam> Params { get; }
}

public interface ITenantManager
{
	ITenantInfo? GetTenantInfo(String? source);
}

