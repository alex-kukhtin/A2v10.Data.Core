// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;
using System.Threading.Tasks;

using A2v10.Data.Tests.Configuration;
using A2v10.Data.Core.Extensions;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Defaults ($Defaults recordset)")]
public class DatabaseDefaults
{
	readonly IDbContext _dbContext;
	public DatabaseDefaults()
	{
		_dbContext = Starter.Create();
	}

	// a new model: the object recordset returns metadata only,
	// $Defaults may be returned unconditionally
	const String NewModelSql = """
select [Document!TDocument!Object] = null, [Id!!Id] = cast(0 as bigint), [No] = N'',
	[Memo] = N'', [Date] = getdate(), [Done] = cast(0 as bit),
	[Store!TStore!RefId] = cast(null as bigint),
	[Rows!TRow!Array] = null
where 0 <> 0;

select [!TRow!Array] = null, [Id!!Id] = cast(0 as bigint), [Qty] = cast(0.0 as float),
	[!TDocument.Rows!ParentId] = cast(0 as bigint)
where 0 <> 0;

select [!$Defaults!] = null, [Document.Store!TStore!RefId] = cast(22 as bigint),
	[Document.Memo] = N'DefaultMemo';

select [!TStore!Map] = null, [Id!!Id] = cast(22 as bigint), [Name!!Name] = N'Main Store';
""";

	[TestMethod]
	public async Task DefaultsForNewModel()
	{
		var dm = await _dbContext.LoadModelSqlAsync(null, NewModelSql);

		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TDocument,TRow,TStore");
		md.HasAllProperties("TRoot", "Document");
		md.HasAllProperties("TDocument", "Id,No,Memo,Date,Done,Store,Rows");

		// the piece is sparse: defaults only, the client builds the rest from metadata
		var docT = new DataTester(dm, "Document");
		docT.AllProperties("Store,Memo");
		docT.AreValueEqual("DefaultMemo", "Memo");

		var storeT = new DataTester(dm, "Document.Store");
		storeT.AreValueEqual((Int64)22, "Id");
		storeT.AreValueEqual("Main Store", "Name");
	}

	[TestMethod]
	public async Task DefaultsBeforeMapRecordset()
	{
		// $Defaults before the Map: the reference is resolved via the forward definition
		var sqlText = """
select [Document!TDocument!Object] = null, [Id!!Id] = cast(0 as bigint),
	[Store!TStore!RefId] = cast(null as bigint)
where 0 <> 0;

select [!TStore!Map] = null, [Id!!Id] = cast(22 as bigint), [Name!!Name] = N'Main Store';

select [!$Defaults!] = null, [Document.Store!TStore!RefId] = cast(22 as bigint);
""";
		var dm = await _dbContext.LoadModelSqlAsync(null, sqlText);
		var storeT = new DataTester(dm, "Document.Store");
		storeT.AreValueEqual((Int64)22, "Id");
		storeT.AreValueEqual("Main Store", "Name");
	}

	[TestMethod]
	public async Task DefaultsIgnoredForLoadedModel()
	{
		var sqlText = """
select [Document!TDocument!Object] = null, [Id!!Id] = cast(100 as bigint),
	[Memo] = N'RealMemo', [Store!TStore!RefId] = cast(55 as bigint);

select [!$Defaults!] = null, [Document.Store!TStore!RefId] = cast(22 as bigint),
	[Document.Memo] = N'DefaultMemo';

select [!TStore!Map] = null, [Id!!Id] = cast(55 as bigint), [Name!!Name] = N'Store 55'
union all
select null, cast(22 as bigint), N'Store 22';
""";
		var dm = await _dbContext.LoadModelSqlAsync(null, sqlText);

		var docT = new DataTester(dm, "Document");
		docT.AreValueEqual((Int64)100, "Id");
		docT.AreValueEqual("RealMemo", "Memo");

		var storeT = new DataTester(dm, "Document.Store");
		storeT.AreValueEqual((Int64)55, "Id");
		storeT.AreValueEqual("Store 55", "Name");
	}

	[TestMethod]
	public async Task BuildNewInstanceWithDefaults()
	{
		var dm = await _dbContext.LoadModelSqlAsync(null, NewModelSql);
		var instance = dm.BuildNewInstance();

		Assert.AreEqual(0, dm.Eval<Object>(instance, "Document.Id"));
		Assert.AreEqual("", dm.Eval<String>(instance, "Document.No"));
		Assert.IsNull(dm.Eval<Object>(instance, "Document.Date"));
		Assert.AreEqual(false, dm.Eval<Boolean>(instance, "Document.Done"));

		// from $Defaults
		Assert.AreEqual("DefaultMemo", dm.Eval<String>(instance, "Document.Memo"));
		Assert.AreEqual((Int64)22, dm.Eval<Object>(instance, "Document.Store.Id"));
		Assert.AreEqual("Main Store", dm.Eval<String>(instance, "Document.Store.Name"));

		var rows = dm.Eval<List<ExpandoObject>>(instance, "Document.Rows");
		Assert.IsNotNull(rows);
		Assert.AreEqual(0, rows.Count);
	}

	[TestMethod]
	public async Task BuildNewInstanceWithoutDefaults()
	{
		// no $Defaults at all: an existing database, nothing is written into Root,
		// the full instance is built from metadata only
		var sqlText = """
select [Document!TDocument!Object] = null, [Id!!Id] = cast(0 as bigint), [Memo] = N'',
	[Store!TStore!RefId] = cast(null as bigint),
	[Agent!TAgent!RefId] = cast(null as bigint)
where 0 <> 0;

select [!TStore!Map] = null, [Id!!Id] = cast(0 as bigint), [Name!!Name] = N''
where 0 <> 0;

select [!TAgent!Map] = null, [Id!!Id] = cast(0 as bigint), [Name!!Name] = N'',
	[Chief!TAgent!RefId] = cast(null as bigint)
where 0 <> 0;
""";
		var dm = await _dbContext.LoadModelSqlAsync(null, sqlText);
		Assert.IsFalse(((IDictionary<String, Object?>)dm.Root).ContainsKey("Document"));

		var instance = dm.BuildNewInstance();
		Assert.AreEqual(0, dm.Eval<Object>(instance, "Document.Id"));
		Assert.AreEqual("", dm.Eval<String>(instance, "Document.Memo"));

		// references are built one level deep
		Assert.AreEqual(0, dm.Eval<Object>(instance, "Document.Store.Id"));
		Assert.AreEqual("", dm.Eval<String>(instance, "Document.Store.Name"));

		// a reference inside a reference (self-referencing type) stays empty
		var chief = dm.Eval<ExpandoObject>(instance, "Document.Agent.Chief");
		Assert.IsNotNull(chief);
		Assert.AreEqual(0, ((IDictionary<String, Object?>)chief).Count);
	}

	[TestMethod]
	public async Task DefaultsFromStoredProcedure()
	{
		// two references + a reference at depth 2 + scalars of all kinds
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[Defaults.Model.Load]");

		var docT = new DataTester(dm, "Document");
		docT.AllProperties("StoreIn,StoreOut,Agent,Name,IsInvoice,Num");
		docT.AreValueEqual("New Document", "Name");
		docT.AreValueEqual(true, "IsInvoice");
		docT.AreValueEqual(42.5, "Num");

		var storeInT = new DataTester(dm, "Document.StoreIn");
		storeInT.AreValueEqual((Int64)101, "Id");
		storeInT.AreValueEqual("Store In", "Name");

		var storeOutT = new DataTester(dm, "Document.StoreOut");
		storeOutT.AreValueEqual((Int64)102, "Id");
		storeOutT.AreValueEqual("Store Out", "Name");

		// the depth-2 reference creates the intermediate Agent object
		var agentT = new DataTester(dm, "Document.Agent");
		agentT.AllProperties("Contact");
		var contactT = new DataTester(dm, "Document.Agent.Contact");
		contactT.AreValueEqual((Int64)305, "Id");
		contactT.AreValueEqual("Contact 305", "Name");

		// the full create-instance over the same model
		var instance = dm.BuildNewInstance();
		Assert.AreEqual(0, dm.Eval<Object>(instance, "Document.Id"));
		Assert.AreEqual("New Document", dm.Eval<String>(instance, "Document.Name"));
		Assert.AreEqual(true, dm.Eval<Boolean>(instance, "Document.IsInvoice"));
		Assert.AreEqual(42.5, dm.Eval<Object>(instance, "Document.Num"));
		Assert.AreEqual((Int64)101, dm.Eval<Object>(instance, "Document.StoreIn.Id"));
		Assert.AreEqual((Int64)102, dm.Eval<Object>(instance, "Document.StoreOut.Id"));
		// the nested object is completed from metadata, its reference is overlaid
		Assert.AreEqual(0, dm.Eval<Object>(instance, "Document.Agent.Id"));
		Assert.AreEqual("", dm.Eval<String>(instance, "Document.Agent.Memo"));
		Assert.AreEqual((Int64)305, dm.Eval<Object>(instance, "Document.Agent.Contact.Id"));
	}

	[TestMethod]
	public async Task BuildMetaDescription()
	{
		var dm = await _dbContext.LoadModelSqlAsync(null, NewModelSql);
		var meta = dm.BuildDataModelMeta();

		Assert.AreEqual("Id", dm.Eval<String>(meta, "types.TDocument.id"));
		Assert.AreEqual("string", dm.Eval<String>(meta, "types.TDocument.props.Memo.type"));
		Assert.AreEqual("TStore", dm.Eval<String>(meta, "types.TDocument.props.Store.type"));
		Assert.AreEqual("IElementArray<TRow>", dm.Eval<String>(meta, "types.TDocument.props.Rows.type"));
		Assert.AreEqual("Name", dm.Eval<String>(meta, "types.TStore.name"));
	}
}
