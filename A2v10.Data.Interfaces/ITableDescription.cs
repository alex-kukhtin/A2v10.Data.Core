﻿// Copyright © 2015-2021 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Data.Interfaces;
public interface ITableDescription
{
	IFormatProvider? FormatProvider { get; set; }

	ExpandoObject NewRow();
	void SetValue(ExpandoObject obj, String propName, Object? value);
	ExpandoObject ToObject();
}

