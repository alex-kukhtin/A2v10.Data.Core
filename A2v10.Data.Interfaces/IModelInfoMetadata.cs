// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data.Interfaces;

public enum FilterType
{
	String,
	Number,
	Date,
	Boolean,
	Period,
	Ref,
	RefArray
}

public interface IModelInfoFilterMetadata
{
	String Path { get; }
	FilterType Type { get; }
	String? RefType { get; }
}

public interface IModelInfoMetadata
{
	Boolean HasPageSize { get; }
	Boolean HasOffset { get; }
	Boolean HasSortOrder { get; }
	Boolean HasSortDir { get; }
	Boolean HasGroupBy { get; }
	Boolean HasRowCount { get; }
	IDictionary<String, IModelInfoFilterMetadata>? Filters { get; }
}
