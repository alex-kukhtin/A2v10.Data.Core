// Copyright © 2023-2025 Oleksandr Kukhtin. All rights reserved.

using System.Data.Common;
using System.Data;
using System.Globalization;

using Microsoft.Data.SqlClient;

using A2v10.Data.Core.Extensions.Dynamic;

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

    public static DbParameterCollection AddDateTime(this DbParameterCollection coll, String name, DateTime? value)
    {
        coll.Add(new SqlParameter(name, SqlDbType.DateTime) { Value = value != null ? value : DBNull.Value });
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

    public static DbParameterCollection AddDateFromQuery(this DbParameterCollection coll, String paramName, ExpandoObject qry, String? prop = null)
    {
        prop = prop ?? paramName.TrimStart('@');
        var val = qry.Get<String>(prop);
        return coll.AddDate(paramName, val != null ?
            DateTime.ParseExact(val, "yyyyMMdd", CultureInfo.InvariantCulture) : null);
    }
    public static DbParameterCollection AddBitFromQuery(this DbParameterCollection coll, String paramName, ExpandoObject qry, String prop)
    {
        var val = qry.Get<Object?>(prop);
        Boolean? boolVal = null;
        if (val is Boolean bv)
            boolVal = bv;
        else if (val is String strVal)
            boolVal = Convert.ToBoolean(strVal, CultureInfo.InvariantCulture);
        return coll.AddBit(paramName, boolVal);
    }
    public static DbParameterCollection AddStringFromQuery(this DbParameterCollection coll, String paramName, ExpandoObject qry, String prop)
    {
        var val = qry.Get<String?>(prop);
        return coll.AddString(paramName, val);
    }

    public static DbParameterCollection AddBigIntFromQuery(this DbParameterCollection coll, String paramName, ExpandoObject qry, String prop)
    {
        var val = qry.Get<Object?>(prop);
        Int64? int64Val = null;
        if (val is Int64 iv)
            int64Val = iv;
        else if (val is String strVal)
            int64Val = Convert.ToInt64(strVal, CultureInfo.InvariantCulture);
        else if (val != null)
            int64Val = Convert.ToInt64(val, CultureInfo.InvariantCulture);
        return coll.AddBigInt(paramName, int64Val);
    }
}
