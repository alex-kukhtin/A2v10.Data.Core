// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.Data;

namespace A2v10.Data.Interfaces;

public interface IParameterBuilder
{
    IParameterBuilder AddBigInt(String name, Int64? value);
    IParameterBuilder AddInt(String name, Int32? value);
    IParameterBuilder AddString(String name, String? value, Int32 size = 255);
    IParameterBuilder AddDate(String name, DateTime? value);
    IParameterBuilder AddDateTime(String name, DateTime? value);
    IParameterBuilder AddBit(String name, Boolean? value);
    IParameterBuilder AddTyped(String name, SqlDbType dbType, Object? value);
    IParameterBuilder AddStructured(String name, String dbTypeName, DataTable dataTable);
    IParameterBuilder AddStringFromQuery(String name, ExpandoObject qry, String? prop = null);
    IParameterBuilder AddFromQuery(String name, ExpandoObject qry, String? prop = null);
}
