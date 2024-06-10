// Copyright © 2024 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data;

public class DataModelBuilder
{
	private readonly IDictionary<String, Object?> _sys = new ExpandoObject();
	private readonly Dictionary<String, GroupMetadata> _groupMetadata = [];
	private readonly Dictionary<String, IDataMetadata> _metadata = [];
	public IDataModel CreateDataModel(ExpandoObject root)
	{
		if (_groupMetadata.Count > 0)
			_sys.Add("Levels", GroupMetadata.GetLevels(_groupMetadata));
		return new DynamicDataModel(_metadata, root, _sys as ExpandoObject)
		{
			MainElement = new()
		};
	}

	public ElementMetadata AddMetadata(String typeName)
	{
		if (_metadata.TryGetValue(typeName, out IDataMetadata? elemMeta))
		{
			if (elemMeta is ElementMetadata realElem)
				return realElem;
			throw new DataLoaderException("Invalid element metadata");
		}
		var newMeta = new ElementMetadata();
		_metadata.Add(typeName, newMeta);
		return newMeta;
	}
}
