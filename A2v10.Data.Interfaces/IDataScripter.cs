﻿// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace A2v10.Data.Interfaces
{
	public class ScriptInfo
	{
		public String Script;
		public String DataScript;
	}

	public class ModelScriptInfo
	{
		public Boolean Admin;
		public String Template;
		public String Path;
		public String BaseUrl;
		public IDataModel DataModel;

		public String RootId;
		public Boolean IsDialog;
		public String RawData;
	}

	public interface IDataScripter
	{
		ScriptInfo GetServerScript(ModelScriptInfo msi);
		Task<ScriptInfo> GetModelScript(ModelScriptInfo msi);

		String CreateScript(IDataHelper helper, IDictionary<String, Object> sys, IDictionary<String, IDataMetadata> meta);
	}
}
