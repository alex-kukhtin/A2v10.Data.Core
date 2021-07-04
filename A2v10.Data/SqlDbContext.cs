// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using A2v10.Data.Interfaces;

namespace A2v10.Data
{
	public class SqlDbContext : IDbContext
	{
		const String RET_PARAM_NAME = "@RetId";

		private readonly IDataProfiler _profiler;
		private readonly IDataConfiguration _config;
		private readonly ITenantManager _tenantManager;
		private readonly IDataLocalizer _localizer;
		private readonly ITokenProvider _tokenProvider;

		public SqlDbContext(IDataProfiler profiler, IDataConfiguration config, IDataLocalizer localizer, ITenantManager tenantManager = null, ITokenProvider tokenProvider = null)
		{
			_profiler = profiler;
			_config = config;
			_localizer = localizer;
			_tenantManager = tenantManager;
			if (_profiler == null)
				throw new ArgumentNullException(nameof(profiler));
			if (_config == null)
				throw new ArgumentNullException(nameof(config));
			if (_localizer == null)
				throw new ArgumentNullException(nameof(localizer));
			_tokenProvider = tokenProvider;
		}

		Int32 CommandTimeout => (Int32)_config.CommandTimeout.TotalSeconds;

		#region IDbContext
		public String ConnectionString(String source)
		{
			return _config.ConnectionString(source);
		}

		public void Execute<T>(String source, String command, T element) where T : class
		{
			using var p = _profiler.Start(command);
			using var cnn = GetConnection(source);
			using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

			var retParam = SetParametersFrom(cmd, element);
			cmd.ExecuteNonQuery();
			SetReturnParamResult(retParam, element);
		}

		public async Task ExecuteAsync<T>(String source, String command, T element) where T : class
		{
			using var token = _profiler.Start(command);
			using var cnn = await GetConnectionAsync(source);
			using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

			var retParam = SetParametersFrom(cmd, element);
			await cmd.ExecuteNonQueryAsync();
			SetReturnParamResult(retParam, element);
		}

		public async Task ExecuteExpandoAsync(String source, String command, ExpandoObject element)
		{
			using var token = _profiler.Start(command);
			using var cnn = await GetConnectionAsync(source);
			using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

			var retParam = SetParametersFromExpandoObject(cmd, element);
			await cmd.ExecuteNonQueryAsync();
			SetReturnParamResult(retParam, element);
		}

		public async Task<ExpandoObject> ReadExpandoAsync(String source, String command, ExpandoObject prms)
		{
			using var p = _profiler.Start(command);
			using var cnn = await GetConnectionAsync(source);
			using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

			var retParam = SetParametersFromExpandoObject(cmd, prms);
			using (var rdr = await cmd.ExecuteReaderAsync())
			{
				if (rdr.Read())
				{
					var eo = new ExpandoObject();
					for (Int32 i = 0; i < rdr.FieldCount; i++)
					{
						eo.Set(rdr.GetName(i), rdr.IsDBNull(i) ? null : rdr.GetValue(i));
					}
					return eo;
				}
			}
			return null;
		}


		public TOut ExecuteAndLoad<TIn, TOut>(String source, String command, TIn element) where TIn : class where TOut : class
		{
			TOut outValue = null;
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

		public async Task<TOut> ExecuteAndLoadAsync<TIn, TOut>(String source, String command, TIn element) where TIn : class where TOut : class
		{
			TOut outValue = null;

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

		public IDbConnection GetDbConnection(String source)
		{
			return GetConnection(source);
		}


		public SqlConnection GetConnection(String source)
		{
			var cnnStr = _config.ConnectionString(source);
			var cnn = new SqlConnection(cnnStr);
			cnn.Open();
			SetTenantId(source, cnn);
			return cnn;
		}

		public async Task<IDbConnection> GetDbConnectionAsync(String source)
		{
			return await GetConnectionAsync(source);
		}

		public async Task<SqlConnection> GetConnectionAsync(String source)
		{
			var cnnStr = _config.ConnectionString(source);
			var cnn = new SqlConnection(cnnStr);
			await cnn.OpenAsync();
			await SetTenantIdAsync(source, cnn);
			return cnn;
		}

		public T Load<T>(String source, String command, System.Object prms = null) where T : class
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

		public async Task<T> LoadAsync<T>(String source, String command, System.Object prms = null) where T : class
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

		public IList<T> LoadList<T>(String source, String command, Object prms) where T : class
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

		public async Task<IList<T>> LoadListAsync<T>(String source, String command, System.Object prms) where T : class
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

		static String ResolveSource(String source, Object prms)
		{
			if (source == null)
				return null;
			source = source.Trim();
			if (source.StartsWith("{{") && source.EndsWith("}}"))
			{
				String key = source[2..^2].Trim();
				String def = null;
				if (key.Contains("??"))
				{
					Int32 pos = key.IndexOf("??");
					def = key[(pos + 2)..].Trim();
					key = key.Substring(0, pos).Trim();
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

		public IDataModel LoadModel(String source, String command, System.Object prms = null)
		{
			var modelReader = new DataModelReader(_localizer, _tokenProvider);
			source = ResolveSource(source, prms);
			using var token = _profiler.Start(command);
			ReadData(source, command,
				(prm) =>
				{
					modelReader.SetParameters(prm, prms);
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

		public async Task<IDataModel> LoadModelAsync(String source, String command, Object prms = null)
		{
			var modelReader = new DataModelReader(_localizer, _tokenProvider);
			source = ResolveSource(source, prms);
			using var token = _profiler.Start(command);
			await ReadDataAsync(source, command,
				(prm) =>
				{
					modelReader.SetParameters(prm, prms);
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

		public void SaveList<T>(String source, String command, System.Object prms, IEnumerable<T> list) where T : class
		{
			using var token = _profiler.Start(command);
			using var cnn = GetConnection(source);
			using var cmd = cnn.CreateCommandSP(command, CommandTimeout);
			SqlCommandBuilder.DeriveParameters(cmd);
			var retParam = SetParametersWithList<T>(cmd, prms, list);
			cmd.ExecuteNonQuery();
			SetReturnParamResult(retParam, prms);
		}

		public async Task SaveListAsync<T>(String source, String command, System.Object prms, IEnumerable<T> list) where T : class
		{
			using var token = _profiler.Start(command);
			using var cnn = await GetConnectionAsync(source);
			using var cmd = cnn.CreateCommandSP(command, CommandTimeout);

			SqlCommandBuilder.DeriveParameters(cmd);
			var retParam = SetParametersWithList<T>(cmd, prms, list);
			await cmd.ExecuteNonQueryAsync();
			SetReturnParamResult(retParam, prms);
		}

		public IDataModel SaveModel(String source, String command, ExpandoObject data, Object prms = null, Int32 commandTimeout = 0)
		{
			var dataReader = new DataModelReader(_localizer, _tokenProvider);
			var dataWriter = new DataModelWriter();

			using var token = _profiler.Start(command);
			var metadataCommand = command.Update2Metadata();
			using var cnn = GetConnection(source);
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
			using (var cmd = cnn.CreateCommandSP(command, CommandTimeout))
			{
				if (commandTimeout != 0)
					cmd.CommandTimeout = commandTimeout;
				SqlCommandBuilder.DeriveParameters(cmd);
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

		public async Task<IDataModel> SaveModelAsync(String source, String command, ExpandoObject data, Object prms = null, Func<ITableDescription, ExpandoObject> onSetData = null, Int32 commandTimeout = 0)
		{
			var dataReader = new DataModelReader(_localizer, _tokenProvider);
			var dataWriter = new DataModelWriter();
			using var token = _profiler.Start(command);

			var metadataCommand = command.Update2Metadata();
			using var cnn = await GetConnectionAsync(source);

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
			if (onSetData != null)
				data = onSetData(dataWriter.GetTableDescription());

			using (var cmd = cnn.CreateCommandSP(command, CommandTimeout))
			{
				if (commandTimeout != 0)
					cmd.CommandTimeout = commandTimeout;
				SqlCommandBuilder.DeriveParameters(cmd);
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


		public async Task<IDataModel> SaveModelBatchAsync(String source, String command, ExpandoObject data, Object prms = null, IEnumerable<BatchProcedure> batches = null)
		{
			if (batches == null || !batches.Any())
				return await SaveModelAsync(source, command, data, prms);

			var dataReader = new DataModelReader(_localizer, _tokenProvider);
			var dataWriter = new DataModelWriter();
			var batchBuilder = new BatchCommandBuilder();

			using var token = _profiler.Start(command);

			var metadataCommand = command.Update2Metadata();
			using var cnn = await GetConnectionAsync(source);

			using (var cmd = cnn.CreateCommandSP(metadataCommand, CommandTimeout))
			{
				using var rdr = await cmd.ExecuteReaderAsync();
				do
				{
					dataWriter.ProcessOneMetadata(rdr);
				}
				while (await rdr.NextResultAsync());
			}

			// main update
			using (var cmdUpdate = cnn.CreateCommandSP(command, CommandTimeout))
			{
				SqlCommandBuilder.DeriveParameters(cmdUpdate);
				dataWriter.SetTableParameters(cmdUpdate, data, prms);
				batchBuilder.AddMainCommand(cmdUpdate);
			}

			int index = 1;
			foreach (var batch in batches)
			{
				using var cmdBatch = cnn.CreateCommandSP(batch.Procedure, CommandTimeout);
				SqlCommandBuilder.DeriveParameters(cmdBatch);
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

		static SqlParameter SetParametersFromExpandoObject(SqlCommand cmd, ExpandoObject element)
		{
			if (element == null)
				return null;
			var elem = element as IDictionary<String, Object>;
			SqlCommandBuilder.DeriveParameters(cmd);
			var sqlParams = cmd.Parameters;
			SqlParameter retParam = null;
			if (cmd.Parameters.Contains(RET_PARAM_NAME))
			{
				retParam = cmd.Parameters[RET_PARAM_NAME];
				retParam.Value = DBNull.Value;
			}
			foreach (var (k, v) in elem)
			{
				var paramName = "@" + k;
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
						sqlParam.Value = GetTableExpandoParams(sqlVal);
					else
					{
						sqlParam.Value = SqlExtensions.ConvertTo(sqlVal, sqlParam.SqlDbType.ToType());
					}
				}
			}
			return retParam;
		}

		static Object GetTableExpandoParams(Object sqlVal)
		{
			/*
			if (sqlVal is ExpandoObject eo)
			{
				//TOD
			}
			*/
			return null;
		}

		static SqlParameter SetParametersFrom<T>(SqlCommand cmd, T element)
		{
			Type retType = typeof(T);
			var props = retType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			SqlCommandBuilder.DeriveParameters(cmd);
			var sqlParams = cmd.Parameters;
			SqlParameter retParam = null;
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
						sqlParam.Value = SqlExtensions.ConvertTo(sqlVal, sqlParam.SqlDbType.ToType());
					}
				}
			}
			return retParam;
		}

		static void SetReturnParamResult(SqlParameter retParam, Object element)
		{
			if (retParam == null)
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
				if (idProp != null)
					idProp.SetValue(element, retParam.Value);
			}
		}

		async Task ReadDataAsync(String source, String command,
			Action<SqlParameterCollection> setParams,
			Action<Int32, IDataReader> onRead,
			Action<Int32, IDataReader> onMetadata)
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

		void ReadData(String source, String command,
			Action<SqlParameterCollection> setParams,
			Action<Int32, IDataReader> onRead,
			Action<Int32, IDataReader> onMetadata)
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

		static SqlParameter SetParametersWithList<T>(SqlCommand cmd, Object prms, IEnumerable<T> list) where T : class
		{
			SqlParameter retParam = null;
			Type listType = typeof(T);
			Type prmsType = prms?.GetType();
			var props = listType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var propsD = new Dictionary<String, PropertyInfo>();
			DataTable dt = new();
			foreach (var p in props)
			{
				var column = new DataColumn(p.Name, p.PropertyType);
				if (p.PropertyType == typeof(String))
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
							var dbVal = SqlExtensions.ConvertTo(rowVal, col.DataType);
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


		Task SetTenantIdAsync(String source, SqlConnection cnn)
		{
			if (_tenantManager == null)
				return Task.CompletedTask;
			var ti = _tenantManager.GetTenantInfo(source);
			if (ti == null)
				return Task.CompletedTask;
			using var cmd = cnn.CreateCommandSP(ti.Procedure, CommandTimeout);
			cmd.Parameters.AddWithValue(ti.ParamName, ti.TenantId);
			return cmd.ExecuteNonQueryAsync();
		}

		void SetTenantId(String source, SqlConnection cnn)
		{
			if (_tenantManager == null)
				return;
			var ti = _tenantManager.GetTenantInfo(source);
			if (ti == null)
				return;
			using var cmd = cnn.CreateCommandSP(ti.Procedure, CommandTimeout);
			cmd.Parameters.AddWithValue(ti.ParamName, ti.TenantId);
			cmd.ExecuteNonQuery();
		}
	}
}
