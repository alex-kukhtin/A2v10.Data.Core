﻿// Copyright © 2015-2023 Oleksandr  Kukhtin. All rights reserved.

using System.IO;
using Newtonsoft.Json;

namespace A2v10.Data;

public class JsonValidator
{
	private readonly AllModels _models;

	public static JsonValidator FromJson(String json)
	{
		return new JsonValidator(json);
	}

	public static JsonValidator FromFile(String path)
	{
		var json = File.ReadAllText(path);
		return new JsonValidator(json);
	}

	private JsonValidator(String json)
	{
		_models = JsonConvert.DeserializeObject<AllModels>(json)!;
		_models.Parse();
	}

	public IDataModelValidator CreateValidator(String name)
	{
		if (!_models.TryGetValue(name, out var validator))
			throw new DataValidationException($"Model {name} not found");
		return validator;
	}
}
