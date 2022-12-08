// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Data.Interfaces;
public interface ITenantInfo
{
	String Procedure { get; }
	String ParamName { get; }
	Object TenantId { get; }
}

public interface ITenantManager
{
	ITenantInfo? GetTenantInfo(String? source);
}

