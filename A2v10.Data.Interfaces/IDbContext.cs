// Copyright © 2015-2022 Alex Kukhtin. All rights reserved.

using System.Data;
using System.Threading.Tasks;

namespace A2v10.Data.Interfaces;
public interface IDbContext
{
	String? ConnectionString(String? source);

	Task<IDbConnection> GetDbConnectionAsync(String? source);
	IDbConnection GetDbConnection(String? source);

	IDataModel LoadModel(String? source, String command, Object? prms = null);
	Task<IDataModel> LoadModelAsync(String? source, String command, Object? prms = null);

	IDataModel SaveModel(String? source, String command, ExpandoObject data, Object? prms = null, Int32 commandTimeout = 0);
	Task<IDataModel> SaveModelAsync(String? source, String command, ExpandoObject data, Object? prms = null, Func<ITableDescription, ExpandoObject>? onSetData = null, Int32 commandTimeout = 0);
	Task<IDataModel> SaveModelBatchAsync(String? source, String command, ExpandoObject data, Object? prms = null, IEnumerable<BatchProcedure>? batches = null, Int32 commandTimeout = 0);

	T? Load<T>(String? source, String command, Object? prms = null) where T : class;
	Task<T?> LoadAsync<T>(String? source, String command, Object? prms = null) where T : class;

	IList<T>? LoadList<T>(String? source, String command, Object? prms = null) where T : class;
	Task<IList<T>?> LoadListAsync<T>(String? source, String command, Object? prms = null) where T : class;

	void SaveList<T>(String? source, String command, Object? prms, IEnumerable<T> list) where T : class;
	Task SaveListAsync<T>(String? source, String command, Object? prms, IEnumerable<T> list) where T : class;

	void Execute<T>(String? source, String command, T element) where T : class;
	Task ExecuteAsync<T>(String? source, String command, T element) where T : class;

	void ExecuteExpando(String? source, String command, ExpandoObject element);
	Task ExecuteExpandoAsync(String? source, String command, ExpandoObject element);

	ExpandoObject? ReadExpando(String? source, String command, ExpandoObject? prms = null);
	Task<ExpandoObject?> ReadExpandoAsync(String? source, String command, ExpandoObject? prms = null);

	TOut? ExecuteAndLoad<TIn, TOut>(String? source, String command, TIn element) where TIn : class where TOut : class;
	Task<TOut?> ExecuteAndLoadAsync<TIn, TOut>(String? source, String command, TIn element) where TIn : class where TOut : class;

	// TYPED inteface
	Task<T> LoadTypedModelAsync<T>(String? source, String command, Object? prms, Int32 commandTimeout = 0) where T : new();

}

