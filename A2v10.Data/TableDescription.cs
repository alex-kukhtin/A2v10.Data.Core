// Copyright © 2012-2017 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;

namespace A2v10.Data
{
	internal class TableDescription : ITableDescription
	{

		private readonly DataTable _table;
		private readonly List<Object> _list;

		public TableDescription(DataTable table)
		{
			_table = table;
			_list = new List<Object>();
		}

		public ExpandoObject NewRow()
		{
			var eo = new ExpandoObject();
			_list.Add(eo);
			return eo;
		}

		public void SetValue(ExpandoObject obj, String propName, Object value)
		{
			var col = _table.Columns[propName];
			if (col == null)
				return;
			var val = ConvertTo(col.DataType, value);
			if (val == null)
				return;
			obj.Set(propName, val);
		}

		Object ConvertTo(Type type, Object value)
		{
			if (value == null)
				return null;
			if (type == value.GetType())
				return value;
			return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
		}

		public ExpandoObject ToObject()
		{
			var eo = new ExpandoObject();
			eo.Set("Rows", _list);
			return eo;
		}
	}
}
