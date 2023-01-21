// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using System.Reflection.Metadata;

namespace A2v10.Data.Core;

internal class TypedDataModelReader<T> where T : new()
{
	private readonly MetadataCache _metadataCache;
	private readonly IDataLocalizer _localizer;
	private readonly ITokenProvider? _tokenProvider;
	private readonly T _root;
	private readonly DataModelReader _metadataReader;
	public TypedDataModelReader(MetadataCache metadataCache, IDataLocalizer localizer, ITokenProvider? tokenProvider) 
	{
		_metadataCache = metadataCache;
		_localizer = localizer;
		_tokenProvider = tokenProvider;
		_metadataReader = new DataModelReader(localizer, tokenProvider);

		_root = new T();
	}

	public void ProcessOneRecord(IDataReader rdr)
	{
		var rsDef = new FieldInfo(rdr.GetName(0));
		if (!String.IsNullOrEmpty(rsDef.PropertyName))
		{
			int z = 55;
		}
		/*
		 * var rdr = metadataCashe.GetReader(fi.PropertyName);
		 * rdr.Read(rdr);
		 * 
		 */
	}

	public void ProcessOneMetadata(IDataReader rdr)
	{
		_metadataReader.ProcessOneMetadata(rdr);
	}

	public void PostProcess()
	{
	}

	public T DataModel => _root;
}
