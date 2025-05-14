// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.Data.Common;
using System.Data;

using A2v10.Data.Core.Extensions;

namespace A2v10.Data;

public class StaticParameterBuilder(DbParameterCollection _prms) : IParameterBuilder
{
    public IParameterBuilder AddBigInt(String name, Int64? value)
    {
        _prms.AddBigInt(name, value);
        return this;
    }
    public IParameterBuilder AddInt(String name, Int32? value)
    {
        _prms.AddInt(name, value);
        return this;

    }
    public IParameterBuilder AddString(String name, String? value, Int32 size = 255)
    {
        _prms.AddString(name, value, size);
        return this;
    }
    public IParameterBuilder AddDate(String name, DateTime? value)
    {
        _prms.AddDate(name, value);
        return this;
    }
    public IParameterBuilder AddDateTime(String name, DateTime? value) {
        _prms.AddDateTime(name, value);
        return this;
    }
    public IParameterBuilder AddBit(String name, Boolean? value)
    {
        _prms.AddBit(name, value);
        return this;
    }
    public IParameterBuilder AddTyped(String name, SqlDbType dbType, Object? value)
    {
        _prms.AddTyped(name, dbType, value);
        return this;
    }
    public IParameterBuilder AddStructured(String name, String dbTypeName, DataTable dataTable)
    {
        _prms.AddStructured(name, dbTypeName, dataTable);
        return this;
    }
}
