// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

using A2v10.Data.DynamicExpression;
using A2v10.Data.Interfaces;

namespace A2v10.Data
{
	[DataContract]
	public class DynamicDataModel : IDataModel
	{

		#region IDataModel
		public ExpandoObject Root { get; }
		public ExpandoObject System { get; set; }
		public IDictionary<String, IDataMetadata> Metadata { get; }
		public DataElementInfo MainElement { get; set; }
		#endregion

		private IDictionary<String, Delegate> _lambdas;

		public DynamicDataModel(IDictionary<String, IDataMetadata> metadata, ExpandoObject root, ExpandoObject system)
		{
			Root = root;
			System = system;
			Metadata = metadata;
		}

		public T Eval<T>(String expression)
		{
			T fallback = default;
			return (this.Root).Eval<T>(expression, fallback);
		}

		public T Eval<T>(ExpandoObject root, String expression)
		{
			T fallback = default;
			return (root).Eval<T>(expression, fallback);
		}


		public T CalcExpression<T>(String expression)
		{
			return CalcExpression<T>(this.Root, expression);
		}

		public T CalcExpression<T>(ExpandoObject root, String expression)
		{
			Object result;
			if (_lambdas != null && _lambdas.TryGetValue(expression, out Delegate expr))
			{
				result = expr.DynamicInvoke(root);
			}
			else
			{
				if (_lambdas == null)
					_lambdas = new Dictionary<String, Delegate>();
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
			return (T) Convert.ChangeType(result, tp);
		}

		public String CreateScript(IDataScripter scripter)
		{
			if (scripter == null)
				throw new ArgumentNullException(nameof(scripter));
			var sys = System as IDictionary<String, Object>;
			var meta = Metadata as IDictionary<String, IDataMetadata>;
			return scripter.CreateScript(DataHelper, sys, meta);
		}

		public IDictionary<String, dynamic> GetDynamic()
		{
			return ObjectBuilder.BuildObject(Root as ExpandoObject);
		}

		IDataHelper _helper;

		public IDataHelper DataHelper
		{
			get
			{
				if (_helper == null)
					_helper = new DataHelper();
				return _helper;
			}
		}

		public void SetReadOnly()
		{
			if (System == null)
				System = new ExpandoObject();
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
			var srcSystem = src.System as IDictionary<String, Object>;
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
					foreach (var f in sm.Value.Fields)
						rootObj.Fields.Add(f.Key, f.Value);
				}
			}
			foreach (var sr in srcRoot)
			{
				if (!trgRoot.AddChecked(sr.Key, sr.Value))
					throw new DataLoaderException($"DataModel.Merge. Item with '{sr.Key}' already has been added");
			}
			foreach (var sys in srcSystem)
				trgSystem.AddChecked(sys.Key, sys.Value);
		}

		public void Validate(IDataModelValidator validator)
		{
			foreach (var m in Metadata)
			{
				validator.ValidateType(m.Key, m.Value);
			}
		}

	}
}
