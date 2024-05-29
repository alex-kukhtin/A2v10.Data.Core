// Copyright © 2015-2024 Oleksandr  Kukhtin. All rights reserved.

using System.Text.RegularExpressions;

namespace A2v10.Data;
public static partial class DynamicExtensions
{
	public static T? Get<T>(this ExpandoObject obj, String name)
	{
		IDictionary<String, Object?> d = obj;
		if (d.TryGetValue(name, out Object? result))
		{
			if (result is T t)
				return t;
		}
		return default;
	}

	public static IDictionary<String, Object?> Dict(this ExpandoObject eo) => eo;
	public static T? GetConvert<T>(this ExpandoObject obj, String name)
	{
		if (obj.Dict().TryGetValue(name, out Object? result))
		{
			if (result == null)
				return default;
			if (result is T t)
				return t;
			return (T?) Convert.ChangeType(result, typeof(T));
		}
		return default;
	}

	public static T GetOrCreate<T>(this ExpandoObject obj, String name) where T : new()
	{
		IDictionary<String, Object?> d = obj;
		if (obj.Dict().TryGetValue(name, out Object? result))
		{
			if (result is T t)
				return t;
			else
				throw new InvalidCastException();
		}
		var no = new T();
		d.Add(name, no);
		return no;
	}

	public static T? GetOrCreate<T>(this ExpandoObject obj, String name, Func<T> create) where T : new()
	{
		IDictionary<String, Object?> d = obj;
		if (d.TryGetValue(name, out Object? result))
		{
			if (result is T t)
				return t;
			else
				throw new InvalidCastException();
		}
		var no = create();
		d.Add(name, no);
		return no;
	}


	public static Object? GetObject(this ExpandoObject obj, String name)
	{
		IDictionary<String, Object?> d = obj;
		if (d.TryGetValue(name, out Object? result))
			return result;
		return null;
	}

	public static Boolean IsEmpty(this ExpandoObject obj)
	{
		if (obj.Dict().Keys.Count == 0)
			return true;
		return false;
	}

	public static void RemoveKey(this ExpandoObject obj, String name) => obj.Dict().Remove(name);


	public static ExpandoObject EnsureObject(this ExpandoObject obj, String name)
	{
		var d = obj as IDictionary<String, Object?>;
		if (d.TryGetValue(name, out var res) && res is ExpandoObject eo)
			return eo;
		eo = [];
		d[name] = eo;
		return eo;
	}

	public static void Set(this ExpandoObject obj, String name, Object? value)
	{
		IDictionary<String, Object?> d = obj;
		if (d.ContainsKey(name))
			d[name] = value;
		else
			d.Add(name, value);
	}

	public static void SetNotNull(this ExpandoObject obj, String name, Object? value)
	{
		if (value == null)
			return;
		obj.Set(name, value);
	}

	public static T? Eval<T>(this ExpandoObject root, String expression, T? fallback = default, Boolean throwIfError = false)
	{
		if (expression == null)
			return fallback;
		Object? result = root.EvalExpression(expression, throwIfError);
		if (result == null)
			return fallback;
		if (result is T t)
			return t;
		return fallback;
	}


	const String PATTERN = @"(\w+)\[(\d+)\]{1}";
#if NET7_0_OR_GREATER
	[GeneratedRegex(PATTERN, RegexOptions.None, "en-US")]
	private static partial Regex EvalRegex();
#else
	private static Regex EVALREGEX => new(PATTERN, RegexOptions.Compiled);
	private static Regex EvalRegex() => EVALREGEX;
#endif

	private static Object? EvalExpression(this ExpandoObject root, String expression, Boolean throwIfError = false)
	{
		Object currentContext = root;

		foreach (var exp in expression.Split('.'))
		{
			if (currentContext == null)
				return null;
			String prop = exp.Trim();
			var d = currentContext as IDictionary<String, Object?>;
			if (prop.Contains('['))
			{
				var match = EvalRegex().Match(prop);
				prop = match.Groups[1].Value;
				if ((d != null) && d.TryGetValue(prop, out var value))
				{
					if (value is IList<ExpandoObject> dList)
						currentContext = dList[Int32.Parse(match.Groups[2].Value)];
				}
				else
				{
					if (throwIfError)
						throw new ArgumentException($"Error in expression '{expression}'. Property '{prop}' not found");
					return null;
				}
			}
			else
			{
				if (d != null && d.TryGetValue(prop, out var value))
					currentContext = value!;
				else
				{
					if (throwIfError)
						throw new ArgumentException($"Error in expression '{expression}'. Property '{prop}' not found");
					return null;
				}
			}
		}
		return currentContext;
	}
}

