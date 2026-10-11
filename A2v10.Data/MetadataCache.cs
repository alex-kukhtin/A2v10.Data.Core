// Copyright © 2022-2026 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Concurrent;

using Microsoft.Data.SqlClient;

namespace A2v10.Data;
public class MetadataCache(IDataConfiguration configuration)
{
    private readonly ConcurrentDictionary<String, WriterMetadata> _cache = [];
    private readonly ConcurrentDictionary<String , List<SqlParameter>> _params = [];
    private readonly Boolean _cacheEnabled = configuration.IsWriteMetadataCacheEnabled;

    internal WriterMetadata? GetWriterMetadata(String command)
    {
        if (_cacheEnabled && _cache.TryGetValue(command, out WriterMetadata? metadata)) {
            return metadata;
        }
        return null;
    }

    internal void AddWriterMetadata(String command, WriterMetadata metadata)
    {
        if (_cacheEnabled)
            _cache[command] = metadata;
    }

    static SqlParameter CloneParam(Object prm)
    {
        if (prm is ICloneable cloneable)
        {
            var newval = cloneable.Clone();
            if (newval is SqlParameter sqlParam)
                return sqlParam;
        }
        throw new NotImplementedException("SqlParameter.IClonable");
    }
    internal void DeriveParameters(SqlCommand cmd)
    {
        if (!_cacheEnabled)
        {
            SqlCommandBuilder.DeriveParameters(cmd);
            return;
        }
        if (_params.TryGetValue(cmd.CommandText, out List<SqlParameter>? prms))
        {
            foreach (var p in prms)
                if (p is SqlParameter sqlParam)
                {
                    if (sqlParam.ParameterName.Equals("@RETURN_VALUE", StringComparison.OrdinalIgnoreCase))
                        continue;
                    cmd.Parameters.Add(CloneParam(sqlParam));
                }
        }
        else
        {
            SqlCommandBuilder.DeriveParameters(cmd);
            var coll = new List<SqlParameter>();
            foreach (var p in cmd.Parameters)
                coll.Add(CloneParam(p));
            _params.TryAdd(cmd.CommandText, coll);
        }
    }
}

