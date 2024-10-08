﻿// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using System.Globalization;

namespace A2v10.Data;

using A2v10.Data.Core.Extensions.Dynamic;

internal class TableDescription(DataTablePattern table) : ITableDescription
{
	public IFormatProvider? FormatProvider { get; set; }

	private readonly DataTablePattern _table = table;
	private readonly List<Object> _list = [];

    public ExpandoObject NewRow()
	{
		var eo = new ExpandoObject();
		_list.Add(eo);
		return eo;
	}

	public void SetValue(ExpandoObject obj, String propName, Object? value)
	{
		var col = _table.GetColumn(propName);
		if (col == null)
			return;
		var val = ConvertTo(col.DataType, value);
		if (val == null)
			return;
		obj.Set(propName, val);
	}

	Object? ConvertTo(Type type, Object? value)
	{
		if (value == null)
			return null;
		if (type == value.GetType())
			return value;
		// special cases
        if (type == typeof(DateTime) && value is Double dblVal)
            return DateTime.FromOADate(dblVal);
        var fp = FormatProvider ?? CultureInfo.InvariantCulture;
		return Convert.ChangeType(value, type, fp);
	}

	public ExpandoObject ToObject()
	{
		var eo = new ExpandoObject();
		eo.Set("Rows", _list);
		return eo;
	}
}

