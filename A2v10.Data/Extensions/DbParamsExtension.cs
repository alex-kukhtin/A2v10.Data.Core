// Copyright © 2023-2024 Oleksandr Kukhtin. All rights reserved.

using System.Data.Common;
using System.Data;

using Microsoft.Data.SqlClient;

namespace A2v10.Data.Core.Extensions;

public static class DbParamsExtension
{
	public static DbParameterCollection AddBigInt(this DbParameterCollection coll, String name, Int64? value)
	{
		coll.Add(new SqlParameter(name, SqlDbType.BigInt) { Value = value != null ? value : DBNull.Value });
		return coll;
	}
	public static DbParameterCollection AddInt(this DbParameterCollection coll, String name, Int32? value)
	{
		coll.Add(new SqlParameter(name, SqlDbType.Int) { Value = value != null ? value : DBNull.Value });
		return coll;
	}

	public static DbParameterCollection AddString(this DbParameterCollection coll, String name, String? value, Int32 size = 255)
	{
		coll.Add(new SqlParameter(name, SqlDbType.NVarChar, size) { Value = value != null ? value : DBNull.Value });
		return coll;
	}
	public static DbParameterCollection AddDate(this DbParameterCollection coll, String name, DateTime? value)
	{
		coll.Add(new SqlParameter(name, SqlDbType.Date) { Value = value != null ? value : DBNull.Value });
		return coll;
	}
	public static DbParameterCollection AddBit(this DbParameterCollection coll, String name, Boolean? value)
	{
		coll.Add(new SqlParameter(name, SqlDbType.Bit) { Value = value != null ? value : DBNull.Value });
		return coll;
	}
    public static DbParameterCollection AddTyped(this DbParameterCollection coll, String name, SqlDbType dbType, Object? value)
    {
        coll.Add(new SqlParameter(name, dbType) { Value = value != null ? value : DBNull.Value });
        return coll;
    }
    public static DbParameterCollection AddStructured(this DbParameterCollection coll, String name, String dbTypeName, DataTable dataTable)
    {
        coll.Add(new SqlParameter(name, SqlDbType.Structured) {
            TypeName = dbTypeName,
			Value = dataTable 
		});
        return coll;
    }
}
