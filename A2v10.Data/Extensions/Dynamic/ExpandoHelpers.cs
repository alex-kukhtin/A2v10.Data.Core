
// Copyright © 2024 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using System.Globalization;

namespace A2v10.Data.Core.Extensions.Dynamic;

public static class ExpandoHelpers
{
    public static void Add(this ExpandoObject eo, String key, Object? value)
    {
        var d = eo as IDictionary<String, Object?>;
        d.Add(key, value);
    }

    public static Boolean AddChecked(this ExpandoObject eo, String key, Object? value)
    {
        var d = eo as IDictionary<String, Object?>;
        if (d.ContainsKey(key))
            return false;
        d.Add(key, value);
        return true;
    }

    public static void CopyFrom(this ExpandoObject target, ExpandoObject? source)
    {
        if (source == null)
            return;
        var dTarget = target as IDictionary<String, Object?>;
        if (dTarget.Count != 0)
            return; // skip if already filled
        var dSource = source as IDictionary<String, Object?>;
        foreach (var (k, v) in dSource)
        {
            dTarget.Add(k, v);
        }
    }

    public static void CopyFromUnconditional(this ExpandoObject target, ExpandoObject? source)
    {
        if (source == null)
            return;
        var dTarget = target as IDictionary<String, Object>;
        var dSource = source as IDictionary<String, Object>;
        foreach (var itm in dSource)
        {
            dTarget[itm.Key] = itm.Value;
        }
    }

    public static IDictionary<String, Object?> GetOrCreate(this IDictionary<String, Object?> dict, String key)
    {
        if (dict.TryGetValue(key, out Object? obj))
            return (obj as IDictionary<String, Object?>)!;
        obj = new ExpandoObject();
        dict.Add(key, obj);
        return (obj as IDictionary<String, Object?>)!;
    }

    public static Object GetDateParameter(this ExpandoObject? eo, String name)
    {
        var val = eo?.Get<Object>(name);
        if (val == null)
            return DBNull.Value;
        if (val is DateTime dt)
            return dt;
        else if (val is String strVal)
            return DateTime.ParseExact(strVal, "yyyyMMdd", CultureInfo.InvariantCulture);
        throw new InvalidExpressionException($"Invalid Date Parameter value: {val}");
    }
}
