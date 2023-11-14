// Copyright © 2015-2023 Olekdandr Kukhtin. All rights reserved.

using System.Data;
using System.Linq;
using System.Text;

using Microsoft.Data.SqlClient;

namespace A2v10.Data;
internal record ParameterDef(String ParamName, String ValueName);

internal class BatchCommandBuilder(Boolean allowEmptyStrings)
{
	private readonly StringBuilder _sb = new();

	private readonly List<SqlParameter> _values = [];
	private readonly HashSet<String> _globalParams = [];
	private readonly Boolean _allowEmptyStrings = allowEmptyStrings;

    public String CommandText => BuildText();


	public void AddMainCommand(SqlCommand cmd)
	{
		var prms = InputAndOutputParameters(cmd.Parameters);
		var prmDefs = new List<ParameterDef>();
		foreach (var prm in prms)
		{
			if (prm is ICloneable cloneable)
			{
				if (cloneable.Clone() is SqlParameter sqlclone)
					_values.Add(sqlclone);
				else
					throw new NotImplementedException("Invalid SqlParameter.IClonable");
			}
			else
				throw new NotImplementedException("SqlParameter.IClonable");
			_globalParams.Add(prm.ParameterName);
			prmDefs.Add(new ParameterDef(prm.ParameterName, prm.ParameterName));
		}
		_sb.AppendLine($"exec {cmd.CommandText} {CommandParameters(cmd, prmDefs)};");
	}

	internal void AddBatchCommand(SqlCommand cmd, BatchProcedure batch, Int32 index)
	{
		var prms = InputParameters(cmd.Parameters);

		var paramDefs = new List<ParameterDef>();

		foreach (var prm in prms)
		{
			if (prm is not ICloneable cloneable)
				throw new NotImplementedException("SqlParameter.IClonable");
			var propName = prm.ParameterName.Remove(0, 1); // without @;
			var propValue = batch.Parameters.Eval<Object>(propName);
			if (propValue == null)
			{
				if (_globalParams.Contains(prm.ParameterName))
					paramDefs.Add(new ParameterDef(prm.ParameterName, prm.ParameterName));
				continue;
			}

			if (cloneable.Clone() is SqlParameter sqlParam)
			{ 
				var originalName = sqlParam.ParameterName;
				sqlParam.ParameterName = $"{sqlParam.ParameterName}_${index}";
				sqlParam.Value = SqlExtensions.ConvertTo(propValue, sqlParam.SqlDbType.ToType(), _allowEmptyStrings);
				_values.Add(sqlParam);
				paramDefs.Add(new ParameterDef(originalName, sqlParam.ParameterName));
			}
		}
		_sb.AppendLine($"exec {cmd.CommandText} {CommandParameters(cmd, paramDefs)};");
	}

	String BuildText()
	{
		var res = new StringBuilder();

		res.AppendLine("set nocount on;")
			.AppendLine("set transaction isolation level read committed;")
			.AppendLine("set xact_abort on;")
			.AppendLine("begin tran;")
			.AppendLine()
			.AppendLine(_sb.ToString())
			.AppendLine("commit tran;");

		return res.ToString();
	}

	public static IEnumerable<SqlParameter> InputAndOutputParameters(SqlParameterCollection prms)
	{
		return prms.OfType<SqlParameter>().Where(p =>
			p.Value != null && (p.Direction == ParameterDirection.Input || p.Direction == ParameterDirection.Output)
		);
	}

	public static IEnumerable<SqlParameter> InputParameters(SqlParameterCollection prms)
	{
		return prms.OfType<SqlParameter>().Where(p => p.Direction == ParameterDirection.Input);
	}

	static String CommandParameters(SqlCommand cmd, IEnumerable<ParameterDef> prms)
	{
		if (cmd.Parameters == null)
			return String.Empty;
		var list = new List<String>();
		foreach (var prm in prms)
		{
			list.Add($"{prm.ParamName} = {prm.ValueName}");
		}
		return String.Join(", ", list);
	}

	internal void SetAllParameters(SqlCommand cmd)
	{
		foreach (var prm in _values)
			cmd.Parameters.Add(prm);
	}
}

