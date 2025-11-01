// Copyright © 2015-2025 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


using Microsoft.Data.SqlClient;

using Newtonsoft.Json;

namespace A2v10.Data;
using A2v10.Data.Core.Extensions.Dynamic;

public class SqlDbContext(IDataProfiler profiler, IDataConfiguration config, IDataLocalizer localizer, MetadataCache metadataCache, ITenantManager? tenantManager = null, ITokenProvider? tokenProvider = null) : IDbContext
{
	const String RET_PARAM_NAME = "@RetId";

	private readonly IDataProfiler _profiler = profiler ?? throw new ArgumentNullException(nameof(profiler));
	private readonly IDataConfiguration _config = config ?? throw new ArgumentNullException(nameof(config));
	private readonly IDataLocalizer _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
	private readonly ITenantManager? _tenantManager = tenantManager;
	private readonly ITokenProvider? _tokenProvider = tokenProvider;
	private readonly MetadataCache _metadataCache = metadataCache;

    Int32 CommandTimeout => (Int32)_config.CommandTimeout.TotalSeconds;

	#region IDbContext
	public String? ConnectionString(String? source)
	{
		return _config.ConnectionString(source);
	}

	public void Execute<T>(String? source, String command, T element) where T : class
	{
		using var p = _profiler.Start(command);
		using var cnn = GetConnection(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

		var retParam = SetParametersFrom(cmd, element);
		cmd.ExecuteNonQuery();
		SetReturnParamResult(retParam, element);
	}

	public async Task ExecuteAsync<T>(String? source, String command, T element) where T : class
	{
		using var token = _profiler.Start(command);
		using var cnn = await GetConnectionAsync(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

		var retParam = SetParametersFrom(cmd, element);
		await cmd.ExecuteNonQueryAsync();
		SetReturnParamResult(retParam, element);
	}

	public void ExecuteExpando(String? source, String command, ExpandoObject element)
	{
		using var token = _profiler.Start(command);
		using var cnn = GetConnection(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

		var retParam = SetParametersFromExpandoObject(cmd, element);
		cmd.ExecuteNonQuery();
		SetReturnParamResult(retParam, element);
	}

	public async Task ExecuteExpandoAsync(String? source, String command, ExpandoObject element)
	{
		using var token = _profiler.Start(command);
		using var cnn = await GetConnectionAsync(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

		var retParam = SetParametersFromExpandoObject(cmd, element);
		await cmd.ExecuteNonQueryAsync();
		SetReturnParamResult(retParam, element);
	}

	public ExpandoObject? ReadExpando(String? source, String command, ExpandoObject? prms = null)
	{
		using var p = _profiler.Start(command);
		using var cnn = GetConnection(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

		var retParam = SetParametersFromExpandoObject(cmd, prms);
        using var rdr = cmd.ExecuteReader();
        if (rdr.Read())
        {
            var eo = new ExpandoObject();
            for (Int32 i = 0; i < rdr.FieldCount; i++)
            {
                eo.Set(rdr.GetName(i), rdr.IsDBNull(i) ? null : rdr.GetValue(i));
            }
            return eo;
        }
        return null;
	}

	public async Task<ExpandoObject?> ReadExpandoAsync(String? source, String command, ExpandoObject? prms = null)
	{
		using var p = _profiler.Start(command);
		using var cnn = await GetConnectionAsync(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

		var retParam = SetParametersFromExpandoObject(cmd, prms);
        using var rdr = await cmd.ExecuteReaderAsync();
        if (rdr.Read())
        {
            var eo = new ExpandoObject();
            for (Int32 i = 0; i < rdr.FieldCount; i++)
            {
                eo.Set(rdr.GetName(i), rdr.IsDBNull(i) ? null : rdr.GetValue(i));
            }
            return eo;
        }
        return null;
	}


	public TOut? ExecuteAndLoad<TIn, TOut>(String? source, String command, TIn element) where TIn : class where TOut : class
	{
		TOut? outValue = null;
		using var p = _profiler.Start(command);
		using var cnn = GetConnection(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

		var retParam = SetParametersFrom(cmd, element);
		using (var rdr = cmd.ExecuteReader())
		{
			var helper = new LoadHelper<TOut>();
			helper.ProcessMetadata(rdr);
			if (rdr.Read())
				outValue = helper.ProcessData(rdr);
		}
		SetReturnParamResult(retParam, element);
		return outValue;
	}

	public async Task<TOut?> ExecuteAndLoadAsync<TIn, TOut>(String? source, String command, TIn element) where TIn : class where TOut : class
	{
		TOut? outValue = null;

		using var token = _profiler.Start(command);
		using var cnn = await GetConnectionAsync(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

		var retParam = SetParametersFrom(cmd, element);
		using (var rdr = await cmd.ExecuteReaderAsync())
		{
			var helper = new LoadHelper<TOut>();
			helper.ProcessMetadata(rdr);
			if (await rdr.ReadAsync())
				outValue = helper.ProcessData(rdr);
		}
		SetReturnParamResult(retParam, element);
		return outValue;
	}

	public IDbConnection GetDbConnection(String? source)
	{
		return GetConnection(source);
	}


	public SqlConnection GetConnection(String? source)
	{
		var cnnStr = _config.ConnectionString(source);
		var cnn = new SqlConnection(cnnStr);
		cnn.Open();
		SetTenantId(source, cnn);
		return cnn;
	}

	public async Task<IDbConnection> GetDbConnectionAsync(String? source)
	{
		return await GetConnectionAsync(source);
	}

	public async Task<SqlConnection> GetConnectionAsync(String? source)
	{
		var cnnStr = _config.ConnectionString(source);
		var cnn = new SqlConnection(cnnStr);
		await cnn.OpenAsync();
		await SetTenantIdAsync(source, cnn);
		return cnn;
	}

	public T? Load<T>(String? source, String command, System.Object? prms = null) where T : class
	{
		using var token = _profiler.Start(command);
		using var cnn = GetConnection(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

		var helper = new LoadHelper<T>();
		SqlExtensions.SetFromDynamic(cmd.Parameters, prms);

		using var rdr = cmd.ExecuteReader();
		helper.ProcessMetadata(rdr);
		if (rdr.Read())
			return helper.ProcessData(rdr);
		return null;
	}

	public async Task<T?> LoadAsync<T>(String? source, String command, System.Object? prms = null) where T : class
	{
		using var token = _profiler.Start(command);
		using var cnn = await GetConnectionAsync(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

		var helper = new LoadHelper<T>();
		SqlExtensions.SetFromDynamic(cmd.Parameters, prms);

		using var rdr = await cmd.ExecuteReaderAsync();
		helper.ProcessMetadata(rdr);
		if (await rdr.ReadAsync())
			return helper.ProcessData(rdr);
		return null;
	}

	public IReadOnlyList<T>? LoadList<T>(String? source, String command, Object? prms = null) where T : class
	{
		using var token = _profiler.Start(command);
		var listLoader = new ListLoader<T>();
		using var cnn = GetConnection(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

		SqlExtensions.SetFromDynamic(cmd.Parameters, prms);
		using (var rdr = cmd.ExecuteReader())
		{
			listLoader.ProcessMetadata(rdr);
			while (rdr.Read())
				listLoader.ProcessData(rdr);
		}
		return listLoader.Result;
	}

	public async Task<IReadOnlyList<T>?> LoadListAsync<T>(String? source, String command, Object? prms = null) where T : class
	{
		using var token = _profiler.Start(command);
		var listLoader = new ListLoader<T>();
		using var cnn = await GetConnectionAsync(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);
		SqlExtensions.SetFromDynamic(cmd.Parameters, prms);

		using (var rdr = await cmd.ExecuteReaderAsync())
		{
			listLoader.ProcessMetadata(rdr);
			while (await rdr.ReadAsync())
			{
				listLoader.ProcessData(rdr);
			}
		}
		return listLoader.Result;
	}

	static String? ResolveSource(String? source, Object? prms)
	{
		if (source == null)
			return null;
		source = source.Trim();
		if (source.StartsWith("{{") && source.EndsWith("}}"))
		{
			String key = source[2..^2].Trim();
			String? def = null;
			if (key.Contains("??"))
			{
				Int32 pos = key.IndexOf("??");
				def = key[(pos + 2)..].Trim();
				key = key[..pos].Trim();
			}
			if (prms is ExpandoObject exp)
			{
				source = exp.Get<String>(key);
				if (String.IsNullOrEmpty(source))
					source = def;
			}
			else
				throw new NotImplementedException("Resolve source without ExpandoObject");
		}
		return source;
	}

	public async Task<T?> LoadTypedModelAsync<T>(String? source, String command, Object? prms = null) where T: new()
	{
		var dm = await LoadModelAsync(source, command, prms);
		if (dm == null)
			return default;
		var jsonString = JsonConvert.SerializeObject(dm.Root);
		return JsonConvert.DeserializeObject<T>(jsonString) ?? default;
	}

    public async Task<TOut?> SaveTypedModelAsync<TIn, TOut>(String? source, String command, TIn data, Object? prms = null) where TOut : new()
	{
		var jsonData = JsonConvert.SerializeObject(data);
		var expData = JsonConvert.DeserializeObject<ExpandoObject>(jsonData);
		if (expData == null)
			return default;
		var newModel = await SaveModelAsync(source, command, expData, prms, null, 0);
		if (newModel == null) 
			return default;
        var jsonString = JsonConvert.SerializeObject(newModel.Root);
        return JsonConvert.DeserializeObject<TOut>(jsonString) ?? default;
    }


    public IDataModel LoadModel(String? source, String command, System.Object? prms = null)
	{
		var modelReader = new DataModelReader(_localizer, _tokenProvider);
		source = ResolveSource(source, prms);
		using var token = _profiler.Start(command);
		ReadData(source, command,
			(prm) =>
			{
				prm.SetParameters(prms);
			},
			(no, rdr) =>
			{
				modelReader.ProcessOneRecord(rdr);
			},
			(no, rdr) =>
			{
				modelReader.ProcessOneMetadata(rdr);
			});
		modelReader.PostProcess();
		return modelReader.DataModel;
	}

    public IDataModel LoadModelSql(String? source, String sqlString, System.Object? prms = null)
    {
        var modelReader = new DataModelReader(_localizer, _tokenProvider);
        source = ResolveSource(source, prms);
        using var token = _profiler.Start("SQL text");
        ReadDataSql(source, sqlString,
            (prm) =>
            {
                prm.SetParameters(prms);
            },
            (no, rdr) =>
            {
                modelReader.ProcessOneRecord(rdr);
            },
            (no, rdr) =>
            {
                modelReader.ProcessOneMetadata(rdr);
            });
        modelReader.PostProcess();
        return modelReader.DataModel;
    }

    public async Task<IDataModel> LoadModelAsync(String? source, String command, Object? prms = null)
	{
		var modelReader = new DataModelReader(_localizer, _tokenProvider);
		source = ResolveSource(source, prms);
		using var token = _profiler.Start(command);
		await ReadDataAsync(source, command,
			(prm) =>
			{
				prm.SetParameters(prms);
			},
			(no, rdr) =>
			{
				modelReader.ProcessOneRecord(rdr);
			},
			(no, rdr) =>
			{
				modelReader.ProcessOneMetadata(rdr);
			});
		modelReader.PostProcess();
		return modelReader.DataModel;
	}

    public async Task<IDataModel> LoadModelSqlAsync(String? source, String sqlString, Object? prms = null)
    {
        var modelReader = new DataModelReader(_localizer, _tokenProvider);
        source = ResolveSource(source, prms);
        using var token = _profiler.Start("SQL text");
        await ReadDataSqlAsync(source, sqlString,
            (prm) =>
            {
                prm.SetParameters(prms);
            },
            (no, rdr) =>
            {
                modelReader.ProcessOneRecord(rdr);
            },
            (no, rdr) =>
            {
                modelReader.ProcessOneMetadata(rdr);
            });
        modelReader.PostProcess();
        return modelReader.DataModel;
    }

	public async Task<IDataModel> LoadModelSqlAsync(String? source, String sqlString, Action<DbParameterCollection> onSetParams)
	{
		var modelReader = new DataModelReader(_localizer, _tokenProvider);
		using var token = _profiler.Start("SQL text");
		await ReadDataSqlAsync(source, sqlString,
			onSetParams.Invoke,
			(no, rdr) =>
			{
				modelReader.ProcessOneRecord(rdr);
			},
			(no, rdr) =>
			{
				modelReader.ProcessOneMetadata(rdr);
			});
		modelReader.PostProcess();
		return modelReader.DataModel;
	}

	public void SaveList<T>(String? source, String command, System.Object? prms, IEnumerable<T> list) where T : class
	{
		using var token = _profiler.Start(command);
		using var cnn = GetConnection(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);
		_metadataCache.DeriveParameters(cmd);
		var retParam = SetParametersWithList<T>(cmd, prms, list);
		cmd.ExecuteNonQuery();
		SetReturnParamResult(retParam, prms);
	}

	public async Task SaveListAsync<T>(String? source, String command, System.Object? prms, IEnumerable<T> list) where T : class
	{
		using var token = _profiler.Start(command);
		using var cnn = await GetConnectionAsync(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

		_metadataCache.DeriveParameters(cmd);
		var retParam = SetParametersWithList<T>(cmd, prms, list);
		await cmd.ExecuteNonQueryAsync();
		SetReturnParamResult(retParam, prms);
	}

	internal WriterMetadata GetWriterMetadata(String command, SqlConnection connection, Int32 commandTimeout = 0)
    {
		var metadataCommand = command.Update2Metadata();
		if (_config.IsWriteMetadataCacheEnabled)
		{
			var wm = _metadataCache.GetWriterMetadata(metadataCommand);
			if (wm != null)
				return wm;
		}
		var writerMetadata = new WriterMetadata();
		using (var cmd = connection.CreateCommandSP(metadataCommand, CommandTimeout))
		{
			if (commandTimeout != 0)
				cmd.CommandTimeout = commandTimeout;
			using var rdr = cmd.ExecuteReader();
			do
			{
				writerMetadata.ProcessOneMetadata(rdr);
			}
			while (rdr.NextResult());
		}
		_metadataCache.AddWriterMetadata(metadataCommand, writerMetadata);
		return writerMetadata;
	}

	public IDataModel SaveModel(String? source, String command, ExpandoObject data, Object? prms = null, Int32 commandTimeout = 0)
	{
		var dataReader = new DataModelReader(_localizer, _tokenProvider);

		using var token = _profiler.Start(command);
		//var metadataCommand = command.Update2Metadata();
		using var cnn = GetConnection(source);
		var dataWriter = new DataModelWriter(GetWriterMetadata(command, cnn, commandTimeout), _config.AllowEmptyStrings);
		/*
		using (var cmd = cnn.CreateCommandSP(metadataCommand, CommandTimeout))
		{
			if (commandTimeout != 0)
				cmd.CommandTimeout = commandTimeout;
			using var rdr = cmd.ExecuteReader();
			do
			{
				dataWriter.ProcessOneMetadata(rdr);
			}
			while (rdr.NextResult());
		}
		*/
		using (var cmd = cnn.CreateCommandSP(command, CommandTimeout))
		{
			if (commandTimeout != 0)
				cmd.CommandTimeout = commandTimeout;
			_metadataCache.DeriveParameters(cmd);
			dataWriter.SetTableParameters(cmd, data, prms);
			using var rdr = cmd.ExecuteReader();
			do
			{
				dataReader.ProcessOneMetadata(rdr);
				while (rdr.Read())
				{
					dataReader.ProcessOneRecord(rdr);
				}
			}
			while (rdr.NextResult());
		}
		dataReader.PostProcess();
		return dataReader.DataModel;
	}

	public async Task<IDataModel> SaveModelAsync(String? source, String command, ExpandoObject data, Object? prms = null, Func<ITableDescription, ExpandoObject>? onSetData = null, Int32 commandTimeout = 0)
	{
		var dataReader = new DataModelReader(_localizer, _tokenProvider);
		using var token = _profiler.Start(command);

		//var metadataCommand = command.Update2Metadata();
		using var cnn = await GetConnectionAsync(source);
		var dataWriter = new DataModelWriter(GetWriterMetadata(command, cnn, commandTimeout), _config.AllowEmptyStrings);

		/*
		using (var cmd = cnn.CreateCommandSP(metadataCommand, CommandTimeout))
		{
			if (commandTimeout != 0)
				cmd.CommandTimeout = commandTimeout;
			using var rdr = await cmd.ExecuteReaderAsync();
			do
			{
				dataWriter.ProcessOneMetadata(rdr);
			}
			while (await rdr.NextResultAsync());
		}
		*/
		if (onSetData != null)
			data = onSetData(dataWriter.GetTableDescription());

		using (var cmd = cnn.CreateCommandSP(command, CommandTimeout))
		{
			if (commandTimeout != 0)
				cmd.CommandTimeout = commandTimeout;
			_metadataCache.DeriveParameters(cmd);
			dataWriter.SetTableParameters(cmd, data, prms);
			using var rdr = await cmd.ExecuteReaderAsync();
			do
			{
				dataReader.ProcessOneMetadata(rdr);
				while (await rdr.ReadAsync())
				{
					dataReader.ProcessOneRecord(rdr);
				}
			}
			while (await rdr.NextResultAsync());
		}
		dataReader.PostProcess();
		return dataReader.DataModel;
	}


	public async Task<IDataModel> SaveModelBatchAsync(String? source, String command, ExpandoObject data, Object? prms = null, IEnumerable<BatchProcedure>? batches = null, Int32 commandTimeout = 0)
	{
		if (batches == null || !batches.Any())
			return await SaveModelAsync(source, command, data, prms);

		var dataReader = new DataModelReader(_localizer, _tokenProvider);
		var batchBuilder = new BatchCommandBuilder(_config.AllowEmptyStrings);

		using var token = _profiler.Start(command);

		//var metadataCommand = command.Update2Metadata();
		using var cnn = await GetConnectionAsync(source);
		var dataWriter = new DataModelWriter(GetWriterMetadata(command, cnn, commandTimeout), _config.AllowEmptyStrings);

		/*
		using (var cmd = cnn.CreateCommandSP(metadataCommand, CommandTimeout))
		{
			using var rdr = await cmd.ExecuteReaderAsync();
			do
			{
				dataWriter.ProcessOneMetadata(rdr);
			}
			while (await rdr.NextResultAsync());
		}
		*/

		// main update
		using (var cmdUpdate = cnn.CreateCommandSP(command, CommandTimeout))
		{
			_metadataCache.DeriveParameters(cmdUpdate);
			dataWriter.SetTableParameters(cmdUpdate, data, prms);
			batchBuilder.AddMainCommand(cmdUpdate);
		}

		int index = 1;
		foreach (var batch in batches)
		{
			using var cmdBatch = cnn.CreateCommandSP(batch.Procedure, CommandTimeout);
			_metadataCache.DeriveParameters(cmdBatch);
			batchBuilder.AddBatchCommand(cmdBatch, batch, index++);
		}

		using (var cmd = cnn.CreateCommand())
		{
			cmd.CommandText = batchBuilder.CommandText;
			cmd.CommandType = CommandType.Text;
			if (CommandTimeout != 0)
				cmd.CommandTimeout = CommandTimeout;
			batchBuilder.SetAllParameters(cmd);
			using var rdr = await cmd.ExecuteReaderAsync();
			do
			{
				dataReader.ProcessOneMetadata(rdr);
				while (await rdr.ReadAsync())
				{
					dataReader.ProcessOneRecord(rdr);
				}
			}
			while (await rdr.NextResultAsync());
		}
		dataReader.PostProcess();
		return dataReader.DataModel;
	}

	#endregion

	SqlParameter? SetParametersFromExpandoObject(SqlCommand cmd, ExpandoObject? element)
	{
		if (element == null)
			return null;
		_metadataCache.DeriveParameters(cmd);
		var sqlParams = cmd.Parameters;
		SqlParameter? retParam = null;
		if (cmd.Parameters.Contains(RET_PARAM_NAME))
		{
			retParam = cmd.Parameters[RET_PARAM_NAME];
			retParam.Value = DBNull.Value;
		}
		foreach (var (k, v) in element)
		{
			var paramName = $"@{k}";
			if (sqlParams.Contains(paramName))
			{
				var sqlParam = sqlParams[paramName];
				var sqlVal = v;

				if (sqlParam.SqlDbType == SqlDbType.VarBinary)
				{
					if (sqlVal == null)
						sqlParam.Value = DBNull.Value;
					else
					{
						if (sqlVal is Byte[] byteArray)
							sqlParam.Value = new SqlBytes(byteArray);
						else if (sqlVal is Stream stream)
							sqlParam.Value = new SqlBytes(stream);
						else
							throw new IndexOutOfRangeException("Stream or byte array expected");
					}
				}
				else if (sqlParam.SqlDbType == SqlDbType.Structured)
				{
					sqlParam.Value = GetTableExpandoParams(sqlVal);
					sqlParam.RemoveDbName();
				}
				else
				{
					sqlParam.Value = SqlExtensions.ConvertTo(sqlVal, sqlParam.SqlDbType.ToType(), _config.AllowEmptyStrings, k);
				}
			}
		}
		return retParam;
	}

	static DataTable? GetTableExpandoParams(Object? sqlVal)
	{
		if (sqlVal is not List<Object> list)
			return null;
		if (list == null || list.Count == 0)
			return null;
		if (list[0] is not ExpandoObject firstElem)
			return null;

		var table = new DataTable();
		foreach (var (k, v) in firstElem)
		{
			if (v == null)
				throw new InvalidProgramException("GetTableExpando() value is null");
			var dc = new DataColumn(k, v.GetType());
			table.Columns.Add(dc);
		}
		foreach (var e in list)
		{
			if (e is not ExpandoObject eo)
				continue;
			var row = table.NewRow();
			foreach (var c in table.Columns) {
				if (c is DataColumn col)
					row[col.ColumnName] = eo.Get<Object>(col.ColumnName);
			}
			table.Rows.Add(row);
		}
		return table;
	}

	SqlParameter? SetParametersFrom<T>(SqlCommand cmd, T element)
	{
		Type retType = typeof(T);
		var props = retType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		_metadataCache.DeriveParameters(cmd);
		var sqlParams = cmd.Parameters;
		SqlParameter? retParam = null;
		if (cmd.Parameters.Contains(RET_PARAM_NAME))
		{
			retParam = cmd.Parameters[RET_PARAM_NAME];
			retParam.Value = DBNull.Value;
		}
		foreach (var p in props)
		{
			var paramName = "@" + p.Name;
			if (sqlParams.Contains(paramName))
			{
				var sqlParam = sqlParams[paramName];
				var sqlVal = p.GetValue(element);

				if (sqlParam.SqlDbType == SqlDbType.VarBinary)
				{
					if (sqlVal == null)
						sqlParam.Value = DBNull.Value;
					else
					{
						if (sqlVal is Byte[] byteArray)
							sqlParam.Value = new SqlBytes(byteArray);
						else if (sqlVal is Stream stream)
							sqlParam.Value = new SqlBytes(stream);
						else
							throw new IndexOutOfRangeException("Stream or byte array expected");
					}
				}
				else
				{
					sqlParam.Value = SqlExtensions.ConvertTo(sqlVal, sqlParam.SqlDbType.ToType(), _config.AllowEmptyStrings, p.Name);
				}
			}
		}
		return retParam;
	}

	static void SetReturnParamResult(SqlParameter? retParam, Object? element)
	{
		if (retParam == null || element == null)
			return;
		if (retParam.Value == DBNull.Value)
			return;
		if (element is ExpandoObject eo)
		{
			eo.Set("Id", retParam.Value);
		}
		else
		{
			var idProp = element.GetType().GetProperty("Id");
			idProp?.SetValue(element, retParam.Value);
		}
	}

	async Task ReadDataAsync(String? source, String command,
		Action<SqlParameterCollection> setParams,
		Action<Int32, IDataReader>? onRead,
		Action<Int32, IDataReader>? onMetadata)
	{
		Int32 rdrNo = 0;
		using var cnn = await GetConnectionAsync(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);
		setParams?.Invoke(cmd.Parameters);

		using var rdr = await cmd.ExecuteReaderAsync();
		do
		{
			onMetadata?.Invoke(rdrNo, rdr);
			while (await rdr.ReadAsync())
			{
				onRead?.Invoke(rdrNo, rdr);
			}
			rdrNo += 1;
		} while (await rdr.NextResultAsync());
	}

    async Task ReadDataSqlAsync(String? source, String sqlString,
        Action<SqlParameterCollection> setParams,
        Action<Int32, IDataReader>? onRead,
        Action<Int32, IDataReader>? onMetadata)
    {
        Int32 rdrNo = 0;
        using var cnn = await GetConnectionAsync(source);
        using var cmd = cnn.CreateCommandText(sqlString, CommandTimeout);
        setParams?.Invoke(cmd.Parameters);

        using var rdr = await cmd.ExecuteReaderAsync();
        do
        {
            onMetadata?.Invoke(rdrNo, rdr);
            while (await rdr.ReadAsync())
            {
                onRead?.Invoke(rdrNo, rdr);
            }
            rdrNo += 1;
        } while (await rdr.NextResultAsync());
    }

    void ReadData(String? source, String command,
		Action<SqlParameterCollection> setParams,
		Action<Int32, IDataReader>? onRead,
		Action<Int32, IDataReader>? onMetadata)
	{
		using var cnn = GetConnection(source);
		using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

		Int32 rdrNo = 0;
		setParams?.Invoke(cmd.Parameters);
		using var rdr = cmd.ExecuteReader();
		do
		{
			onMetadata?.Invoke(rdrNo, rdr);
			while (rdr.Read())
			{
				onRead?.Invoke(rdrNo, rdr);
			}
			rdrNo += 1;
		} while (rdr.NextResult());
	}

    void ReadDataSql(String? source, String sqlString,
        Action<SqlParameterCollection> setParams,
        Action<Int32, IDataReader>? onRead,
        Action<Int32, IDataReader>? onMetadata)
    {
        using var cnn = GetConnection(source);
        using var cmd = cnn.CreateCommandText(sqlString, CommandTimeout);

        Int32 rdrNo = 0;
        setParams?.Invoke(cmd.Parameters);
        using var rdr = cmd.ExecuteReader();
        do
        {
            onMetadata?.Invoke(rdrNo, rdr);
            while (rdr.Read())
            {
                onRead?.Invoke(rdrNo, rdr);
            }
            rdrNo += 1;
        } while (rdr.NextResult());
    }

    SqlParameter? SetParametersWithList<T>(SqlCommand cmd, Object? prms, IEnumerable<T> list) where T : class
	{
		SqlParameter? retParam = null;
		Type listType = typeof(T);
		Type? prmsType = prms?.GetType();
		var props = listType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		var propsD = new Dictionary<String, PropertyInfo>();
		DataTable dt = new();
		foreach (var p in props)
		{
			Type propType = p.PropertyType;
			if (propType.IsNullableType())
				propType = Nullable.GetUnderlyingType(propType) ??
					throw new InvalidOperationException("GetUnderlyingType() is null");
			var column = new DataColumn(p.Name, propType);
			if (propType == typeof(String))
				column.MaxLength = 32767;
			dt.Columns.Add(column);
			propsD.Add(p.Name, p);
		}
		for (Int32 i = 0; i < cmd.Parameters.Count; i++)
		{
			SqlParameter prm = cmd.Parameters[i];
			if (prm.ParameterName == RET_PARAM_NAME)
			{
				retParam = prm;
				prm.Value = DBNull.Value;
				continue;
			}
			var simpleParamName = prm.ParameterName[1..]; // skip @
			if (prm.SqlDbType == SqlDbType.Structured)
			{
				foreach (var itm in list)
				{
					var row = dt.NewRow();
					for (Int32 c = 0; c < dt.Columns.Count; c++)
					{
						var col = dt.Columns[c];
						var rowVal = propsD[col.ColumnName].GetValue(itm);
						var dbVal = SqlExtensions.ConvertTo(rowVal, col.DataType, _config.AllowEmptyStrings, col.ColumnName);
						row[col.ColumnName] = dbVal;
					}
					dt.Rows.Add(row);
				}
				prm.Value = dt;
				prm.RemoveDbName(); // remove first segment (database name)
			}
			else if (prms is ExpandoObject eo)
			{
				var pv = eo.Get<Object>(simpleParamName);
				if (pv != null)
					prm.Value = pv;
			}
			else if (prmsType != null)
			{
				// scalar parameter
				var pi = prmsType.GetProperty(simpleParamName);
				if (pi != null)
					prm.Value = pi.GetValue(prms);
			}
		}
		return retParam;
	}


	async Task SetTenantIdAsync(String? source, SqlConnection cnn)
	{
		if (_tenantManager == null)
			return;
		var ti = _tenantManager.GetTenantInfo(source);
		if (ti == null)
			return;
		using var cmd = cnn.CreateCommandSP(ti.Procedure, CommandTimeout);
		foreach (var tp in ti.Params)
			cmd.Parameters.AddWithValue(tp.ParamName, tp.Value);
		// Do not return TASK here!!!
		await cmd.ExecuteNonQueryAsync();
	}

	void SetTenantId(String? source, SqlConnection cnn)
	{
		if (_tenantManager == null)
			return;
		var ti = _tenantManager.GetTenantInfo(source);
		if (ti == null)
			return;
		using var cmd = cnn.CreateCommandSP(ti.Procedure, CommandTimeout);
		foreach (var tp in ti.Params)
			cmd.Parameters.AddWithValue(tp.ParamName, tp.Value);
		cmd.ExecuteNonQuery();
	}

	public void LoadRaw(String? source, String procedure, ExpandoObject prms, Action<Int32, IDataReader> action) 
	{
		ReadData(source, procedure,
			(prm) => prm.SetParameters(prms),
			(no, rdr) => action(no, rdr),
			null
		);
	}
	public Task LoadRawAsync(String? source, String procedure, ExpandoObject prms, Action<Int32, IDataReader> action)
	{
		return ReadDataAsync(source, procedure,
			(prm) => prm.SetParameters(prms),
			(no, rdr) => action(no, rdr),
			null
		);
	}
    public IParameterBuilder ParameterBuilder(DbParameterCollection prms) => new StaticParameterBuilder(prms);
}

