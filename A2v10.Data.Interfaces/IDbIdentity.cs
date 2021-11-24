// Copyright © 2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Data.Interfaces;
public interface IDbIdentity
{
	Int64? UserId { get; }
	Int32? TenantId { get; }
	String Segment { get; }
}

