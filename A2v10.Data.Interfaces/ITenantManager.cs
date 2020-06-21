// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Data;
using System.Threading.Tasks;

namespace A2v10.Data.Interfaces
{
	public interface ITenantManager
	{
		Task SetTenantIdAsync(IDbConnection cnn, String source);
		void SetTenantId(IDbConnection cnn, String source);
	}
}
