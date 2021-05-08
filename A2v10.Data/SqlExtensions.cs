// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Globalization;
using System.Linq;

namespace A2v10.Data
{
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

		public static Object ConvertTo(Object value, Type to)
		{
			if (value == null)
				return DBNull.Value;
			if (value is ExpandoObject eo)
			{
				var id = eo.GetObject("Id");
				if (DataHelpers.IsIdIsNull(id))
					return DBNull.Value;
				return Convert.ChangeType(id, to, CultureInfo.InvariantCulture);
			}
			if (value is String str)
			{
				if (String.IsNullOrEmpty(str))
					return DBNull.Value;
				if (to == typeof(Guid))
					return Guid.Parse(str);
				return value;
			}
			if (value.GetType() == to)
				return value;
			return Convert.ChangeType(value, to, CultureInfo.InvariantCulture);
		}

		public static Object Value2SqlValue(Object value)
		{
			if (value == null)
				return DBNull.Value;
			return value;
		}

		public static IDictionary<String, Object> GetParametersDictionary(Object prms)
		{
			if (prms == null)
				return null;
			if (prms is ExpandoObject)
				return prms as IDictionary<String, Object>;
			var retDict = new Dictionary<String, Object>();
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
				var newName = $"[{prm.TypeName.Substring(0, dotPos)}].[{prm.TypeName[(dotPos + 1)..]}]";
				prm.TypeName = newName;
			}
		}

		public static void SetFromDynamic(SqlParameterCollection prms, Object vals)
		{
			if (vals == null)
				return;
			IDictionary<String, Object> valsD;
			// may be EpandoObject
			valsD = vals as IDictionary<String, Object>;
			if (valsD == null)
			{
				valsD = vals.GetType()
					.GetProperties()
					.ToDictionary(key => key.Name, val => val.GetValue(vals));
			}
			foreach (var prop in valsD)
			{
				prms.AddWithValue("@" + prop.Key, prop.Value);
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
}
