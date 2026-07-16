// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data;

public sealed record ModelInfoFilterMetadata(String Path, FilterType Type, String? RefType = null) : IModelInfoFilterMetadata;

// mutable accumulator (filled while the $System recordset is being processed), not a record
public sealed class ModelInfoMetadata : IModelInfoMetadata
{
	private Dictionary<String, IModelInfoFilterMetadata>? _filters;

	public Boolean HasPageSize { get; internal set; }
	public Boolean HasOffset { get; internal set; }
	public Boolean HasSortOrder { get; internal set; }
	public Boolean HasSortDir { get; internal set; }
	public Boolean HasGroupBy { get; internal set; }
	public Boolean HasRowCount { get; internal set; }

	public IDictionary<String, IModelInfoFilterMetadata>? Filters => _filters;

	internal void AddFilter(ModelInfoFilterMetadata filter)
	{
		_filters ??= [];
		_filters[filter.Path] = filter;
	}
}
