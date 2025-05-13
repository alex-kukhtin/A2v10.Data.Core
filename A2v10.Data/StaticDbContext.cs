// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

namespace A2v10.Data.Core;

public class StaticDbContext(IDataConfiguration _config) : IStaticDbContext
{
    public void ExecuteNonQuery(String? source, String procedure, Action<DbParameterCollection> onSetParams)
    {
        ExecuteNonQueryInt(source, procedure, CommandType.StoredProcedure, onSetParams);
    }

    public Task ExecuteNonQueryAsync(String? source, String procedure, Action<DbParameterCollection> onSetParams)
    {
        return ExecuteNonQueryIntAsync(source, procedure, CommandType.StoredProcedure, onSetParams);
    }

    public void ExecuteNonQuerySql(String? source, String sqlText, Action<DbParameterCollection> onSetParams)
    {
        ExecuteNonQueryInt(source, sqlText, CommandType.Text, onSetParams);
    }

    public Task ExecuteNonQuerySqlAsync(String? source, String sqlText, Action<DbParameterCollection> onSetParams)
    {
        return ExecuteNonQueryIntAsync(source, sqlText, CommandType.Text, onSetParams);
    }

    public void ExecuteReader(String? source, String procedure, Action<DbParameterCollection> onSetParams, Action<int, IDataReader> onRead)
    {
        ExecuteReaderInt(source, procedure, CommandType.StoredProcedure, onSetParams, onRead);
    }

    public Task ExecuteReaderAsync(String? source, String procedure, Action<DbParameterCollection> onSetParams, Action<int, IDataReader> action)
    {
        return ExecuteReaderIntAsync(source, procedure, CommandType.StoredProcedure, onSetParams, action);
    }

    public void ExecuteReaderSql(String? source, String sqlText, Action<DbParameterCollection> onSetParams, Action<int, IDataReader> onRead)
    {
        ExecuteReaderInt(source, sqlText, CommandType.Text, onSetParams, onRead);
    }

    public Task ExecuteReaderSqlAsync(String? source, String sqlText, Action<DbParameterCollection> onSetParams, Action<int, IDataReader> action)
    {
        return ExecuteReaderIntAsync(source, sqlText, CommandType.Text, onSetParams, action);
    }

    // IMPLEMENTATION

    private void ExecuteNonQueryInt(String? source, String text, CommandType commandType, Action<DbParameterCollection> onSetParams)
    {
        using var cnn = GetConnection(source);
        using var cmd = cnn.CreateCommand();
        cmd.CommandText = text;
        cmd.CommandType = commandType;
        onSetParams(cmd.Parameters);
        cmd.ExecuteNonQuery();
    }

    private async Task ExecuteNonQueryIntAsync(String? source, String text, CommandType commandType, Action<DbParameterCollection> onSetParams)
    {
        using var cnn = await GetConnectionAsync(source);
        using var cmd = cnn.CreateCommand();
        cmd.CommandText = text;
        cmd.CommandType = commandType;
        onSetParams(cmd.Parameters);
        await cmd.ExecuteNonQueryAsync();
    }

    private void ExecuteReaderInt(String? source, String text, CommandType commandType, Action<DbParameterCollection> onSetParams, Action<int, IDataReader> onRead)
    {
        using var cnn = GetConnection(source);
        using var cmd = cnn.CreateCommand();
        cmd.CommandText = text;
        cmd.CommandType = commandType;
        onSetParams(cmd.Parameters);

        Int32 rdrNo = 0;
        using var rdr = cmd.ExecuteReader();
        do
        {
            while (rdr.Read())
            {
                onRead?.Invoke(rdrNo, rdr);
            }
            rdrNo += 1;
        } while (rdr.NextResult());
    }

    private async Task ExecuteReaderIntAsync(String? source, String text, CommandType commandType, Action<DbParameterCollection> onSetParams, Action<int, IDataReader> onRead)
    {
        using var cnn = await GetConnectionAsync(source);
        using var cmd = cnn.CreateCommand();
        cmd.CommandText = text;
        cmd.CommandType = commandType;
        onSetParams(cmd.Parameters);

        Int32 rdrNo = 0;
        using var rdr = await cmd.ExecuteReaderAsync();
        do
        {
            while (await rdr.ReadAsync())
            {
                onRead?.Invoke(rdrNo, rdr);
            }
            rdrNo += 1;
        } while (await rdr.NextResultAsync());
    }
    private SqlConnection GetConnection(String? source)
    {
        var cnnStr = _config.ConnectionString(source);
        var cnn = new SqlConnection(cnnStr);
        cnn.Open();
        return cnn;
    }

    private async Task<SqlConnection> GetConnectionAsync(String? source)
    {
        var cnnStr = _config.ConnectionString(source);
        var cnn = new SqlConnection(cnnStr);
        await cnn.OpenAsync();
        return cnn;
    }
}
