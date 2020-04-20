// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq;

namespace A2v10.Data
{
	public static class SqlExtensions
	{
		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
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
			switch (sqlType)
			{
				case SqlDbType.BigInt:
					return typeof(Int64);
				case SqlDbType.Int:
					return typeof(Int32);
				case SqlDbType.SmallInt:
					return typeof(Int16);
				case SqlDbType.TinyInt:
					return typeof(Byte);
				case SqlDbType.Bit:
					return typeof(Boolean);
				case SqlDbType.Float:
					return typeof(Double);
				case SqlDbType.Money:
				case SqlDbType.Decimal:
					return typeof(Decimal);
				case SqlDbType.Real:
					return typeof(Double);
				case SqlDbType.DateTime:
				case SqlDbType.Date:
				case SqlDbType.DateTime2:
					return typeof(DateTime);
				case SqlDbType.DateTimeOffset:
					return typeof(DateTimeOffset);
				case SqlDbType.NVarChar:
				case SqlDbType.NText:
				case SqlDbType.NChar:
					return typeof(String);
				case SqlDbType.UniqueIdentifier:
					return typeof(Guid);
			}
			throw new ArgumentOutOfRangeException("SqlExtensions.SqlType.ToType");
		}

		public static Object ConvertTo(Object value, Type to)
		{
			if (value == null)
				return DBNull.Value;
			else if (value is ExpandoObject)
			{
				var id = (value as ExpandoObject).GetObject("Id");
				if (DataHelpers.IsIdIsNull(id))
					return DBNull.Value;
				return Convert.ChangeType(id, to, CultureInfo.InvariantCulture);
			}
			else if (value is String)
			{
				var str = value.ToString();
				if (String.IsNullOrEmpty(str))
					return DBNull.Value;
				if (to == typeof(Guid))
					return Guid.Parse(value.ToString());
				return value;
			}
			else
			{
				return Convert.ChangeType(value, to, CultureInfo.InvariantCulture);
			}
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
				prm.TypeName = prm.TypeName.Substring(dotPos + 1);

				dotPos = prm.TypeName.IndexOf('.');
				// wrap TypeName into []
				var newName = $"[{prm.TypeName.Substring(0, dotPos)}].[{prm.TypeName.Substring(dotPos + 1)}]";
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
				return source.Substring(0, source.Length - 7) + ".Metadata";
			else if (source.EndsWith(".Update]"))
				return source.Substring(0, source.Length - 8) + ".Metadata]";
			return source;
		}
	}
}
