// Copyright © 2015-2021 Alex Kukhtin. All rights reserved.

using System.Threading.Tasks;

namespace A2v10.Data.Interfaces;

public record ScriptInfo(String? Script, String? DataScript);

public record ModelScriptInfo
{
	public Boolean Admin { get; init; }
	public String? Template { get; init; }
	public String? Path { get; init; }
	public String? BaseUrl { get; init; }
	public IDataModel? DataModel { get; init; }

	public String? RootId { get; init; }
	public Boolean IsDialog { get; init; }
	public Boolean IsIndex { get; init; }
	public Boolean IsSkipDataStack { get; init; }
	public Boolean IsPlain { get; init; }
	public String? RawData { get; init; }
}

public interface IDataScripter
{
	ScriptInfo GetServerScript(ModelScriptInfo msi);
	Task<ScriptInfo> GetModelScript(ModelScriptInfo msi);

	String CreateScript(IDataHelper helper, IDictionary<String, Object?>? sys, IDictionary<String, IDataMetadata> meta);
}

