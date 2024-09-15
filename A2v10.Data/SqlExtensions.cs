// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace A2v10.Data;

using A2v10.Data.Core.Extensions.Dynamic;

public static class SqlExtensions
{
	public static SqlCommand CreateCommandSP(this SqlConnection cnn, String command, Int32 commandTimeout)
	{
		var cmd = cnn.CreateCommand();
		cmd.CommandType = CommandType.StoredProcedure;
		cmd.CommandText = command;
		if (commandTimeout != 0)
			cmd.CommandTimeout = commandTimeout;
		return cmd;
	}

    public static SqlCommand CreateCommandText(this SqlConnection cnn, String sqlString, Int32 commandTimeout)
    {
        var cmd = cnn.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = sqlString;
        if (commandTimeout != 0)
            cmd.CommandTimeout = commandTimeout;
        return cmd;
    }

    public static Type ToType(this SqlDbType sqlType)
	{
		return sqlType switch
		{
			SqlDbType.BigInt => typeof(Int64),
			SqlDbType.Int => typeof(Int32),
			SqlDbType.SmallInt => typeof(Int16),
			SqlDbType.TinyInt => typeof(Byte),
			SqlDbType.Bit => typeof(Boolean),
			SqlDbType.Float or SqlDbType.Real => typeof(Double),
			SqlDbType.Money or SqlDbType.Decimal => typeof(Decimal),
			SqlDbType.DateTime or SqlDbType.Date or SqlDbType.DateTime2 => typeof(DateTime),
			SqlDbType.DateTimeOffset => typeof(DateTimeOffset),
			SqlDbType.Time => typeof(TimeSpan),
			SqlDbType.NVarChar or SqlDbType.NText or SqlDbType.NChar or SqlDbType.VarChar or SqlDbType.Text or SqlDbType.Char => typeof(String),
			SqlDbType.VarBinary => typeof(Byte[]),
			SqlDbType.UniqueIdentifier => typeof(Guid),
			_ => throw new ArgumentOutOfRangeException($"SqlExtensions.SqlType.ToType. Unable to cast to '{sqlType}'"),
		};
	}

	static readonly String[] dateFormats =
    [
        "yyyyMMdd",
		"yyyy-MM-dd",
		"yyyy-MM-ddTHH:mm"
	];

	public static Object ParseString(String str, Type to)
    {
		if (to == typeof(Int64))
			return Int64.Parse(str, CultureInfo.InvariantCulture);
		else if (to == typeof(Int32))
			return Int32.Parse(str, CultureInfo.InvariantCulture);
		else if (to == typeof(Decimal))
			return Decimal.Parse(str, CultureInfo.InvariantCulture);	
		else if (to == typeof(Double))
			return Double.Parse(str, CultureInfo.InvariantCulture);
		else if (to == typeof(Guid))
			return Guid.Parse(str);
		else if (to == typeof(DateTime))
		{
			if (DateTime.TryParseExact(str, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateResult))
				return dateResult;
			if (DateTime.TryParse(str, out var dateResult2))
				return dateResult2;
			return Convert.ToDateTime(str);
		}
		return str;
    }

	public static Object ConvertTo(Object? value, Type to, Boolean allowEmptyString)
	{
		if (value == null)
			return DBNull.Value;
		if (value is ExpandoObject eo)
		{
			var id = eo.GetObject("Id");
			if (DataHelpers.IsIdIsNull(id))
				return DBNull.Value;
			if (to == typeof(Guid))
				return Guid.Parse(id!.ToString()!);
			return Convert.ChangeType(id, to, CultureInfo.InvariantCulture)!;
		}
		else if (value is String str)
		{
			if (!allowEmptyString && String.IsNullOrEmpty(str))
				return DBNull.Value;
			if (to == typeof(String))
				return value;
			return ParseString(value.ToString()!, to);
		}
		if (value.GetType() == to)
			return value;
		return Convert.ChangeType(value, to, CultureInfo.InvariantCulture);
	}

	public static Object Value2SqlValue(Object? value)
	{
		if (value == null)
			return DBNull.Value;
		return value;
	}

	public static IDictionary<String, Object?>? GetParametersDictionary(Object? prms)
	{
		if (prms == null)
			return null;
		if (prms is ExpandoObject eoPrms)
			return eoPrms as IDictionary<String, Object?>;
		var retDict = new Dictionary<String, Object?>();
		var props = prms.GetType().GetProperties();
		foreach (var p in props)
		{
			retDict.Add(p.Name, p.GetValue(prms, null));
		}
		return retDict;
	}

	public static void RemoveDbName(this SqlParameter prm)
	{
		Int32 dotPos = prm.TypeName.IndexOf('.');
		if (dotPos != -1)
		{
			prm.TypeName = prm.TypeName[(dotPos + 1)..];

			dotPos = prm.TypeName.IndexOf('.');
			// wrap TypeName into []
			var newName = $"[{prm.TypeName[..dotPos]}].[{prm.TypeName[(dotPos + 1)..]}]";
			prm.TypeName = newName;
		}
	}

	public static void SetFromDynamic(SqlParameterCollection prms, Object? vals)
	{
		if (vals == null)
			return;
		IDictionary<String, Object?>? valsD;
		// may be EpandoObject
		valsD = vals as IDictionary<String, Object?>;
		valsD ??= vals.GetType()
				.GetProperties()
				.ToDictionary(key => key.Name, val => val.GetValue(vals));
		foreach (var prop in valsD)
		{
			prms.AddWithValue("@" + prop.Key, prop.Value);
		}
	}

	public static void SetParameters(this SqlParameterCollection prms, Object? vals)
	{
		if (vals == null)
			return;
		if (vals is ExpandoObject eo)
		{
			foreach (var e in eo)
			{
				var val = e.Value;
				if (val != null)
					prms.AddWithValue($"@{e.Key}", val);
			}
		}
		else
		{
			var props = vals.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (var prop in props)
			{
				var val = prop.GetValue(vals);
				if (val != null)
					prms.AddWithValue($"@{prop.Name}", val);
			}
		}
	}


	public static String Update2Metadata(this String source)
	{
		if (source.EndsWith(".Update"))
			return source[0..^7] + ".Metadata";
		else if (source.EndsWith(".Update]"))
			return source[0..^8] + ".Metadata]";
		return source;
	}
}

