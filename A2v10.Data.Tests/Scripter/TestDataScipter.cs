
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.Data.Tests;

public static class SBExtensions
{
    public static StringBuilder RemoveTailComma(this StringBuilder sb)
    {
        if (sb.Length < 1)
            return sb;
        Int32 len = sb.Length;
        if (sb[len - 1] == ',')
            sb.Remove(len - 1, 1);
        return sb;
    }
    public static String ToJsonObject(this IEnumerable<String> list)
    {
        return $"{{{String.Join(',', list)}}}";
    }
}

internal class TestDataScipter : IDataScripter
{
    public ScriptInfo GetServerScript(ModelScriptInfo msi)
    {
        return new ScriptInfo(null, null);
    }
    public Task<ScriptInfo> GetModelScript(ModelScriptInfo msi)
    {
        return Task.FromResult(new ScriptInfo(null, null));
    }
    public String CreateScript(IDataHelper helper, IDictionary<String, Object?>? sys, IDictionary<String, IDataMetadata> meta)
    {
        var sb = new StringBuilder();
        sb.AppendLine("function modelData(template, data) {");
        sb.AppendLine("const cmn = require('std:datamodel');");
        if (meta != null)
            sb.Append(GetConstructors(meta));
        sb.AppendLine("cmn.implementRoot(TRoot, template, ctors);");
        sb.AppendLine("let root = new TRoot(data);");
        sb.Append(SetModelInfo(helper, sys));
        sb.AppendLine("return root;}");
        return sb.ToString();
    }

    static String GetConstructors(IDictionary<String, IDataMetadata> meta)
    {
        if (meta == null)
            return String.Empty;
        var sb = new StringBuilder();
        foreach (var m in meta)
        {
            sb.Append(GetOneConstructor(m.Key, m.Value));
            sb.AppendLine();
        }
        // make ctors
        var list = new List<String>();
        foreach (var re in meta)
        {
            list.Add(re.Key);
            if (re.Value.IsArrayType)
                list.Add($"{re.Key}Array");
        }
        sb.AppendLine($"const ctors = {list.ToJsonObject()};");
        return sb.ToString();
    }

    static StringBuilder GetOneConstructor(String name, IDataMetadata ctor)
    {
        var sb = new StringBuilder();
        String arrItem = ctor.IsArrayType ? "true" : "false";

        sb.AppendLine($"function {name}(source, path, parent) {{")
        .AppendLine("cmn.createObject(this, source, path, parent);")
        .AppendLine("}")
        // metadata
        .Append($"cmn.defineObject({name}, {{props: {{")
        .Append(GetProperties(ctor))
        .Append('}')
        .Append(GetSpecialProperties(ctor))
        .AppendLine($"}}, {arrItem});");

        if (ctor.IsArrayType)
        {
            sb.AppendLine($"function {name}Array(source, path, parent) {{")
            .AppendLine($"return cmn.createArray(source, path, {name}, {name}Array, parent);")
            .AppendLine("}");
        }
        return sb;
    }

    public static StringBuilder GetProperties(IDataMetadata meta)
    {
        var sb = new StringBuilder();
        foreach (var fd in meta.Fields)
        {
            var fm = fd.Value;
            String propObj = fm.GetObjectType($"{meta.Name}.{fd.Key}");
            if (propObj == "String")
            {
                if (fm.IsJson)
                    propObj = $"{{type:String, len:{fm.Length}, json:true}}";
                else
                    propObj = $"{{type:String, len:{fm.Length}}}";
            }
            else if (propObj == "TPeriod")
                propObj = $"{{type: uPeriod.constructor}}";
            sb.Append($"'{fd.Key}'")
            .Append(':')
            .Append(propObj)
            .Append(',');
        }
        if (sb.Length == 0)
            return sb;
        sb.RemoveTailComma();
        return sb;
    }

    static public String GetSpecialProperties(IDataMetadata meta)
    {
        var sb = new StringBuilder();
        if (!String.IsNullOrEmpty(meta.Id))
            sb.Append($"$id: '{meta.Id}',");
        if (!String.IsNullOrEmpty(meta.Name))
            sb.Append($"$name: '{meta.Name}',");
        if (!String.IsNullOrEmpty(meta.RowNumber))
            sb.Append($"$rowNo: '{meta.RowNumber}',");
        if (!String.IsNullOrEmpty(meta.HasChildren))
            sb.Append($"$hasChildren: '{meta.HasChildren}',");
        if (!String.IsNullOrEmpty(meta.MapItemType))
            sb.Append($"$itemType: {meta.MapItemType},");
        if (!String.IsNullOrEmpty(meta.Permissions))
            sb.Append($"$permissions: '{meta.Permissions}',");
        if (!String.IsNullOrEmpty(meta.Items))
            sb.Append($"$items: '{meta.Items}',");
        if (!String.IsNullOrEmpty(meta.Expanded))
            sb.Append($"$expanded: '{meta.Expanded}',");
        if (!String.IsNullOrEmpty(meta.MainObject))
            sb.Append($"$main: '{meta.MainObject}',");
        if (!String.IsNullOrEmpty(meta.Token))
            sb.Append($"$token: '{meta.Token}',");
        if (meta.IsGroup)
            sb.Append($"$group: true,");
        if (meta.HasCross)
            sb.Append($"$cross: {GetCrossProperties(meta)},");
        var lazyFields = new StringBuilder();
        foreach (var f in meta.Fields)
        {
            if (f.Value.IsLazy)
                lazyFields.Append($"'{f.Key}',");
        }
        if (lazyFields.Length != 0)
        {
            lazyFields.RemoveTailComma();
            sb.Append($"$lazy: [{lazyFields}]");
        }
        if (sb.Length == 0)
            return String.Empty;
        sb.RemoveTailComma();
        return ", " + sb.ToString();
    }

    static String GetCrossProperties(IDataMetadata meta)
    {
        var sb = new StringBuilder("{");
        foreach (var c in meta.Cross!)
        {
            sb.Append($"{c.Key}: [");
            if (c.Value != null)
                sb.Append(String.Join(",", c.Value.Select(s => $"'{s}'")));
            sb.Append("],");
        }
        sb.RemoveTailComma();
        sb.AppendLine("}");
        return sb.ToString();
    }

    static String SetModelInfo(IDataHelper helper, IDictionary<String, Object?>? sys)
    {
        if (sys == null)
            return String.Empty;
        var list = new List<String>();
        foreach (var k in sys)
        {
            var val = k.Value;
            if (val is Boolean bVal)
                val = bVal ? "true" : "false";
            else if (val is String)
                val = $"'{val}'";
            else if (val is DateTime dateTimeObj)
                val = helper.DateTime2StringWrap(dateTimeObj);
            else if (val is Object valObj)
                val = JsonConvert.SerializeObject(valObj);
            list.Add($"'{k.Key}': {val}");
        }
        return $"cmn.setModelInfo(root, {list.ToJsonObject()}, rawData);";
    }
}
