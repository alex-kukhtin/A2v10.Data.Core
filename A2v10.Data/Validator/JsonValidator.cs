// Copyright © 2015-2019 Alex Kukhtin. All rights reserved.

using System;
using Newtonsoft.Json;

using A2v10.Data.Interfaces;
using System.IO;

namespace A2v10.Data.Validator
{
	public class JsonValidator
	{
		private AllModels _models;

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
			_models = JsonConvert.DeserializeObject<AllModels>(json);
			_models.Parse();
		}

		public IDataModelValidator CreateValidator(String name)
		{
			if (!_models.ContainsKey(name))
				throw new DataValidationException($"Model {name} not found");
			return _models[name];
		}
	}
}
