// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace A2v10.Data.Interfaces;
public interface IStaticDbContext
{
	Task ExecuteNonQueryAsync(String? source, String procedure, Action<DbParameterCollection> onSetParams);
    void ExecuteNonQuery(String? source, String procedure, Action<DbParameterCollection> onSetParams);

    Task ExecuteNonQuerySqlAsync(String? source, String sqlText, Action<DbParameterCollection> onSetParams);
    void ExecuteNonQuerySql(String? source, String sqlText, Action<DbParameterCollection> onSetParams);

    Task ExecuteReaderAsync(String? source, String procedure, Action<DbParameterCollection> onSetParams, Action<Int32, IDataReader> onRead);
    void ExecuteReader(String? source, String procedure, Action<DbParameterCollection> onSetParams, Action<Int32, IDataReader> onRead);

    Task ExecuteReaderSqlAsync(String? source, String sqlText, Action<DbParameterCollection> onSetParams, Action<Int32, IDataReader> onRead);
    void ExecuteReaderSql(String? source, String sqlText, Action<DbParameterCollection> onSetParams, Action<Int32, IDataReader> onRead);
}

