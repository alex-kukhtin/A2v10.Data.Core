// Copyright © 2015-2024 Oleksandr  Kukhtin. All rights reserved.

using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using A2v10.Data.DynamicExpression;

namespace A2v10.Data;

[DataContract]
public partial class DynamicDataModel(IDictionary<String, IDataMetadata> metadata, ExpandoObject root, ExpandoObject? system) : IDataModel
{

    #region IDataModel
    public ExpandoObject Root { get; } = root;
    public ExpandoObject? System { get; set; } = system;
    public IDictionary<String, IDataMetadata> Metadata { get; } = metadata;
    public DataElementInfo MainElement { get; set; }
	#endregion

	private Dictionary<String, Delegate>? _lambdas;

    public T? Eval<T>(String expression)
	{
		T? fallback = default;
		return (this.Root).Eval<T>(expression, fallback);
	}

	public T? Eval<T>(ExpandoObject root, String expression)
	{
		T? fallback = default;
		return (root).Eval<T>(expression, fallback);
	}


	public T? CalcExpression<T>(String expression)
	{
		return CalcExpression<T>(this.Root, expression);
	}

	public T? CalcExpression<T>(ExpandoObject root, String expression)
	{
		Object? result;
		if (_lambdas != null && _lambdas.TryGetValue(expression, out Delegate? expr))
		{
			result = expr.DynamicInvoke(root);
		}
		else
		{
			_lambdas ??= [];
			var prms = new ParameterExpression[] {
				Expression.Parameter(typeof(ExpandoObject), "Root")
			};
			var lexpr = DynamicParser.ParseLambda(prms, expression);
			expr = lexpr.Compile();
			_lambdas.Add(expression, expr);
			result = expr.DynamicInvoke(root);
		}
		if (result == null)
			return default;
		if (result is T resultT)
			return resultT;
		var tp = typeof(T);
		if (tp.IsNullableType())
			tp = Nullable.GetUnderlyingType(tp);
		return (T)Convert.ChangeType(result, tp!);
	}

	const String RESOLVE_PATTERN = "\\{\\{(.+?)\\}\\}";
#if NET7_0_OR_GREATER
	[GeneratedRegex(RESOLVE_PATTERN, RegexOptions.None, "en-US")]
	private static partial Regex ResolveRegex();
#else
	private static Regex RESOLVEREGEX => new(RESOLVE_PATTERN, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase );
	private static Regex ResolveRegex() => RESOLVEREGEX;
#endif

	public String? Resolve(String? source)
	{
		if (source == null)
			return source;
		if (String.IsNullOrEmpty(source))
			return source;
		if (!source.Contains("{{"))
			return source;
		var ms = ResolveRegex().Matches(source);
		if (ms.Count == 0)
			return source;
		var sb = new StringBuilder(source);
		foreach (Match m in ms.Cast<Match>())
		{
			String key = m.Groups[1].Value;
			var valObj = Eval<Object>(key);
			if (ms.Count == 1 && m.Groups[0].Value == source)
				return valObj?.ToString() ?? String.Empty; // single element
			if (valObj is String valStr)
				sb.Replace(m.Value, valStr);
			else if (valObj is ExpandoObject valEo)
				sb.Replace(m.Value, JsonConvert.SerializeObject(valEo));
			else
				sb.Replace(m.Value, valObj?.ToString());

		}
		return sb.ToString();
	}

	public String CreateScript(IDataScripter scripter)
	{
        ArgumentNullException.ThrowIfNull(scripter);	
		var sys = System as IDictionary<String, Object?>;
		var meta = Metadata as IDictionary<String, IDataMetadata>;
		return scripter.CreateScript(DataHelper, sys, meta);
	}

	public IDictionary<String, dynamic> GetDynamic()
	{
		return ObjectBuilder.BuildObject(Root as ExpandoObject);
	}

	private readonly IDataHelper _helper = new DataHelper();

	public IDataHelper DataHelper => _helper;

	public void SetReadOnly()
	{
		System ??= [];
		System.Set("ReadOnly", true);
		System.Set("StateReadOnly", true);
	}

	public void MakeCopy()
	{
		if (Root == null)
			return;
		foreach (var md in Metadata)
		{
			if (!String.IsNullOrEmpty(md.Value.Id))
			{
				// TODO
			}
		}
	}

	public Boolean IsReadOnly
	{
		get
		{
			if (System != null)
				return System.Get<Boolean>("ReadOnly");
			return false;
		}
	}

	public Boolean IsEmpty
	{
		get
		{
			if (Metadata != null)
				return false;
			if (Root != null && !Root.IsEmpty())
				return false;
			return true;
		}
	}


	public void Merge(IDataModel src)
	{
		var trgMeta = Metadata as IDictionary<String, IDataMetadata>;
		var srcMeta = src.Metadata as IDictionary<String, IDataMetadata>;
		var trgRoot = Root;
		var srcRoot = src.Root as IDictionary<String, Object>;
		var rootObj = trgMeta["TRoot"];
		var trgSystem = System;
		foreach (var sm in srcMeta)
		{
			if (sm.Key != "TRoot")
			{
				if (trgMeta.ContainsKey(sm.Key))
					trgMeta[sm.Key] = sm.Value;
				else
					trgMeta.Add(sm.Key, sm.Value);
			}
			else
			{
				foreach (var (k, v) in sm.Value.Fields)
					rootObj.Fields.Add(k, v);
			}
		}
		foreach (var sr in srcRoot)
		{
			if (!trgRoot.AddChecked(sr.Key, sr.Value))
				throw new DataLoaderException($"DataModel.Merge. Item with '{sr.Key}' already has been added");
		}
		if (src.System is IDictionary<String, Object> srcSystem)
		{
			trgSystem ??= [];
			foreach (var (k, v) in srcSystem)
				trgSystem.AddChecked(k, v);
		}
	}

	public void Validate(IDataModelValidator validator)
	{
		foreach (var m in Metadata)
		{
			validator.ValidateType(m.Key, m.Value);
		}
	}

	private static ExpandoObject GetOrCreateCross(ExpandoObject val)
	{
		const String crossProp = "$cross";
		var d = val as IDictionary<String, Object>;
		if (d.TryGetValue(crossProp, out Object? value))
		{
			if (value is ExpandoObject rv)
				return rv;
			throw new InvalidOperationException("Cross is not an ExpandoObject");
		}
		var c = new ExpandoObject();
		val.Add(crossProp, c);
		return c;
	}

	public void AddRuntimeProperties()
	{
		ExpandoObject? rt = null;
		foreach (var m in Metadata)
		{
			IDataMetadata md = m.Value;
			if (md.HasCross)
			{
				rt ??= [];
				var cross = GetOrCreateCross(rt);
				var xo = new ExpandoObject();
				if (md.Cross != null)
				{
					foreach (var ci in md.Cross)
						xo.Add(ci.Key, ci.Value);
				}
				cross.Add(m.Key, xo);
			}
		}
		if (rt != null)
		{
			Root.Add("$runtime", rt);
		}
	}
}

