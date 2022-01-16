// Copyright © 2022 Alex Kukhtin. All rights reserved.

using System.Data;

namespace A2v10.Data;

internal record DataColumnPattern
{
    public Type DataType { get; }
    public String Name { get; }
    public Int32 MaxLength { get; set; } 
    public DataColumnPattern(String name, Type type)
    {
        DataType = type;   
        Name = name;
    }
    public DataColumn ToDataColumn()
    {
        var dc = new DataColumn(Name, DataType);
        if (MaxLength != 0)
            dc.MaxLength = MaxLength;
        return dc;
    }
}
internal class DataTablePattern
{
    private readonly Dictionary<String, DataColumnPattern> _columns = new();

    public void AddColumn(DataColumnPattern column)
    {
        _columns.Add(column.Name, column);
    }

    public DataColumnPattern? GetColumn(String name)
    {
        if (_columns.TryGetValue(name, out DataColumnPattern? column))
            return column;
        return null;
    }

    public DataTable ToDataTable()
    {
        var dt = new DataTable();
        foreach (DataColumnPattern column in _columns.Values)
            dt.Columns.Add(column.ToDataColumn());
        return dt;
    }
}

internal record DataTablePatternTuple(DataTablePattern Table, String Path);