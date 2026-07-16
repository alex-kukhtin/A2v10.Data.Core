// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data;
using A2v10.Data.Core.Extensions.Dynamic;

namespace A2v10.Data.Core.Extensions;

public static class DataModelExtensions
{
	const String ROOT = "TRoot";

	/// <summary>
	/// Builds a declarative description of the model: types with their properties
	/// and the filter dictionary (from IDataMetadata.ModelInfos). Metadata only,
	/// no data values are used.
	/// </summary>
	public static ExpandoObject BuildDataModelMeta(this IDataModel? model)
	{
		if (model == null)
			return [];

		ExpandoObject buildProps(IDataMetadata metadata)
		{
			var props = new ExpandoObject();
			foreach (var (name, fm) in metadata.Fields)
			{
				props.Add(name, new ExpandoObject() {
					{ "type", fm.TypeScriptName },
					{ "len", fm.Length == 0 ? null : fm.Length }
				});
			}
			return props;
		}

		ExpandoObject buildTypes()
		{
			var types = new ExpandoObject();
			foreach (var (name, meta) in model.Metadata)
			{
				types.Add(name, new ExpandoObject()
				{
					{ "props", buildProps(meta) },
					{ "id", meta.Id },
					{ "name", meta.Name },
				});
			}
			return types;
		}

		ExpandoObject? buildFilters()
		{
			if (!model.Metadata.TryGetValue(ROOT, out IDataMetadata? rootMeta)
				|| rootMeta.ModelInfos == null)
				return null;
			var filters = new ExpandoObject();
			foreach (var (rootKey, mi) in rootMeta.ModelInfos)
			{
				if (mi.Filters == null)
					continue;
				var props = new ExpandoObject();
				foreach (var (propName, f) in mi.Filters)
				{
					var prop = new ExpandoObject() {
						{ "type", f.Type switch {
							FilterType.Period => "period",
							FilterType.Ref or FilterType.RefArray => "reference",
							_ => "value"
						} }
					};
					if (f.RefType != null)
						prop.Add("refType", f.RefType);
					if (f.Type == FilterType.RefArray)
						prop.Add("isArray", true);
					props.Add(propName, prop);
				}
				if (((IDictionary<String, Object?>)props).Count > 0)
					filters.Add(rootKey, props);
			}
			return ((IDictionary<String, Object?>)filters).Count > 0 ? filters : null;
		}

		var result = new ExpandoObject()
		{
			{ "types", buildTypes() }
		};
		var filters = buildFilters();
		if (filters != null)
			result.Add("filters", filters);
		return result;
	}

	/// <summary>
	/// Builds a complete instance of a new model from metadata, overlaying values
	/// already present in Root (including the sparse piece written by the $Defaults
	/// recordset). The result shares unmodified sub-objects with Root — treat it
	/// as read-only or serialize it.
	/// Scalar defaults mirror the client element constructor: String → '',
	/// Number → 0, Boolean → false, Date → null, row version → ''. Reference
	/// objects are built one level deep; references inside them stay {}.
	/// </summary>
	public static ExpandoObject BuildNewInstance(this IDataModel model)
	{
		if (!model.Metadata.TryGetValue(ROOT, out IDataMetadata? rootMeta))
			return [];
		var typePath = new HashSet<String>() { ROOT };
		return BuildInstance(model.Metadata, rootMeta, model.Root, insideRef: false, typePath);
	}

	static ExpandoObject BuildInstance(IDictionary<String, IDataMetadata> types, IDataMetadata meta,
		ExpandoObject? source, Boolean insideRef, HashSet<String> typePath)
	{
		var result = new ExpandoObject();
		var src = source as IDictionary<String, Object?>;
		foreach (var (name, fieldMeta) in meta.Fields)
		{
			Object? srcVal = null;
			var hasSrc = src != null && src.TryGetValue(name, out srcVal);
			result.Add(name, BuildFieldValue(types, fieldMeta, srcVal, hasSrc, insideRef, typePath));
		}
		return result;
	}

	static ExpandoObject BuildObject(IDictionary<String, IDataMetadata> types, String typeName,
		Object? srcVal, Boolean insideRef, HashSet<String> typePath)
	{
		if (!types.TryGetValue(typeName, out IDataMetadata? typeMeta))
			return srcVal as ExpandoObject ?? [];
		if (!typePath.Add(typeName))
			return srcVal as ExpandoObject ?? []; // type cycle, keep it empty
		var result = BuildInstance(types, typeMeta, srcVal as ExpandoObject, insideRef, typePath);
		typePath.Remove(typeName);
		return result;
	}

	static Object? BuildFieldValue(IDictionary<String, IDataMetadata> types, IDataFieldMetadata fieldMeta,
		Object? srcVal, Boolean hasSrc, Boolean insideRef, HashSet<String> typePath)
	{
		if (fieldMeta is not FieldMetadata fm)
			return hasSrc ? srcVal : null;
		if (fm.IsRefId)
		{
			if (srcVal is ExpandoObject srcRef && ((IDictionary<String, Object?>)srcRef).Count > 0)
				return srcRef; // already resolved (loaded or $Defaults + Map)
			if (insideRef)
				return new ExpandoObject(); // references are built one level deep
			return BuildObject(types, fm.RefObject, null, insideRef: true, typePath);
		}
		switch (fm.ItemType)
		{
			case FieldType.Scalar:
				if (hasSrc)
					return srcVal;
				return fm.DataType switch
				{
					DataType.String => String.Empty,
					DataType.Number => 0,
					DataType.Boolean => false,
					_ => null // Date, Blob, Undefined
				};
			case FieldType.RowVersion:
				return hasSrc ? srcVal : String.Empty;
			case FieldType.Json:
				return hasSrc ? srcVal : null;
			case FieldType.Object:
			case FieldType.Group:
			case FieldType.Sheet:
			case FieldType.CrossObject:
				if (insideRef)
					return srcVal ?? new ExpandoObject();
				return BuildObject(types, fm.RefObject, srcVal, insideRef, typePath);
			case FieldType.Array:
			case FieldType.Tree:
			case FieldType.Map:
			case FieldType.CrossArray:
			case FieldType.Rows:
			case FieldType.Columns:
			case FieldType.Cells:
				return hasSrc && srcVal != null ? srcVal : new List<ExpandoObject>();
			case FieldType.MapObject:
			case FieldType.Lookup:
				return srcVal ?? new ExpandoObject();
			default:
				return hasSrc ? srcVal : null;
		}
	}
}
