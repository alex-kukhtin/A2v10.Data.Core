// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data.Interfaces
{
	public interface IDbIdentity
	{
		Int64? UserId { get; }
		Int32? TenantId { get; }
	}
}
