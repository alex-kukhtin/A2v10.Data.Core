// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using A2v10.Data.DynamicExpression;

using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace A2v10.Data;

[DataContract]
public class DynamicDataModel : IDataModel
{

	#region IDataModel
	public ExpandoObject Root { get; }
	public ExpandoObject? System { get; set; }
	public IDictionary<String, IDataMetadata> Metadata { get; }
	public DataElementInfo MainElement { get; set; }
	#endregion

	private IDictionary<String, Delegate>? _lambdas;

	public DynamicDataModel(IDictionary<String, IDataMetadata> metadata, ExpandoObject root, ExpandoObject? system)
	{
		Root = root;
		System = system;
		Metadata = metadata;
	}

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
			_lambdas ??= new Dictionary<String, Delegate>();
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

	public String CreateScript(IDataScripter scripter)
	{
		if (scripter == null)
			throw new ArgumentNullException(nameof(scripter));
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
		System ??= new ExpandoObject();
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
			trgSystem ??= new ExpandoObject();
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

	public void AddRuntimeProperties()
	{
		ExpandoObject? rt = null;
		foreach (var m in Metadata)
		{
			IDataMetadata md = m.Value;
			if (md.HasCross)
			{
				rt ??= new ExpandoObject();
				var cross = new ExpandoObject();
				rt.Add("$cross", cross);
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

