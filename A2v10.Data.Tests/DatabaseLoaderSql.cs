// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data.Tests.Configuration;
using Microsoft.Data.SqlClient;
using System.Dynamic;
using System.Threading.Tasks;

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Load Models from SQL Text")]
public class DatabaseLoaderSql
{
    readonly IDbContext _dbContext;
    public DatabaseLoaderSql()
    {
        _dbContext = Starter.Create();
    }

    [TestMethod]
    public void LoadSimpleModelSqlSync()
    {

        var sqlText = """
				select [Model!TModel!Object] = null, [Id!!Id] = @Id, [Name!!Name]='ObjectName', [Decimal] = @Value;
			""";

        var prms = new ExpandoObject()
        {
            { "Id", 123 },
            { "Value", Convert.ToDecimal(55.1234)}
        };

        var dm = _dbContext.LoadModelSql(null, sqlText, prms);

        var md = new MetadataTester(dm);
        md.IsAllKeys("TRoot,TModel");
        md.HasAllProperties("TRoot", "Model");
        md.HasAllProperties("TModel", "Name,Id,Decimal");
        md.IsId("TModel", "Id");
        md.IsName("TModel", "Name");

        var dt = new DataTester(dm, "Model");
        dt.AreValueEqual(123, "Id");
        dt.AreValueEqual("ObjectName", "Name");
        dt.AreValueEqual(55.1234M, "Decimal");
    }
   
    [TestMethod]
    public async Task LoadComplexModelSqlAsync()
    {
        var sqlString = 
"""
-- @Id bigint,
-- @Agent bigint

select [Document!TDocument!Object] = null, [Id!!Id] = @Id, [No]='DocNo', [Date]=a2sys.fn_getCurrentDate(),
	[Agent!TAgent!RefId] = @Agent, [Company!TAgent!RefId] = @Agent,
	[Rows1!TRow!Array] = null, [Rows2!TRow!Array] = null;

select [!TRow!Array] = null, [Id!!Id] = 78, [!TDocument.Rows1!ParentId] = @Id, 
	[Product!TProduct!RefId] = 782,
	Qty=cast(4.0 as float), Price=cast(8 as money), [Sum] = cast(32.0 as money),
	[Series1!TSeries!Array] = null

select [!TRow!Array] = null, [Id!!Id] = 79, [!TDocument.Rows2!ParentId] = @Id, 
	[Product!TProduct!RefId] = 785,
	Qty=cast(7.0 as float), Price=cast(2 as money), [Sum] = cast(14.0 as money),
	[Series1!TSeries!Array] = null

-- series for rows
select [!TSeries!Array]=null, [Id!!Id] = 500, [!TRow.Series1!ParentId] = 78, Price=cast(5 as float)
union all
select [!TSeries!Array]=null, [Id!!Id] = 501, [!TRow.Series1!ParentId] = 79, Price=cast(10 as float)

-- maps for product
select [!TProduct!Map] = null, [Id!!Id] = 782, [Name!!Name] = N'Product 782',
	[Unit.Id!TUnit!Id] = 7, [Unit.Name!TUnit!Name] = N'Unit7'
union all
select [!TProduct!Map] = null, [Id!!Id] = 785, [Name!!Name] = N'Product 785',
	[Unit.Id!TUnit!Id] = 8, [Unit.Name!TUnit!Name] = N'Unit8'

-- maps for agent
select [!TAgent!Map] = null, [Id!!Id] = @Agent, [Name!!Name] = 'Agent 512', Code=N'Code 512';
""";

        var prms = new ExpandoObject()
        {
            { "Id", Convert.ToInt64(123) },
            { "Agent", Convert.ToInt64(512)}
        };

        IDataModel dm = await _dbContext.LoadModelSqlAsync(null, sqlString, prms);
        var md = new MetadataTester(dm);
        md.IsAllKeys("TRoot,TDocument,TRow,TAgent,TProduct,TSeries,TUnit");
        md.HasAllProperties("TRoot", "Document");
        md.HasAllProperties("TDocument", "Id,No,Date,Agent,Company,Rows1,Rows2");
        md.HasAllProperties("TRow", "Id,Product,Qty,Price,Sum,Series1");
        md.HasAllProperties("TProduct", "Id,Name,Unit");
        md.HasAllProperties("TUnit", "Id,Name");

        var docT = new DataTester(dm, "Document");
        docT.AreValueEqual((Int64) 123, "Id");
        docT.AreValueEqual("DocNo", "No");

        var agentT = new DataTester(dm, "Document.Agent");
        agentT.AreValueEqual((Int64) 512, "Id");
        agentT.AreValueEqual("Agent 512", "Name");
        agentT.AreValueEqual("Code 512", "Code");

        agentT = new DataTester(dm, "Document.Company");
        agentT.AreValueEqual((Int64)512, "Id");
        agentT.AreValueEqual("Agent 512", "Name");
        agentT.AreValueEqual("Code 512", "Code");

        var row1T = new DataTester(dm, "Document.Rows1");
        row1T.IsArray(1);
        row1T.AreArrayValueEqual(78, 0, "Id");
        row1T.AreArrayValueEqual(4.0, 0, "Qty");

        var row2T = new DataTester(dm, "Document.Rows2");
        row2T.IsArray(1);
        row2T.AreArrayValueEqual(79, 0, "Id");
        row2T.AreArrayValueEqual(7.0, 0, "Qty");

        var row1Obj = new DataTester(dm, "Document.Rows1[0]");
        row1Obj.AreValueEqual(78, "Id");
        row1Obj.AreValueEqual(4.0, "Qty");
        row1Obj.AllProperties("Id,Qty,Price,Sum,Product,Series1");

        var prodObj = new DataTester(dm, "Document.Rows1[0].Product");
        prodObj.AreValueEqual(782, "Id");
        prodObj.AreValueEqual("Product 782", "Name");
        prodObj.AllProperties("Id,Name,Unit");
        var unitObj = new DataTester(dm, "Document.Rows1[0].Product.Unit");
        unitObj.AreValueEqual(7, "Id");
        unitObj.AreValueEqual("Unit7", "Name");
        unitObj.AllProperties("Id,Name");

        prodObj = new DataTester(dm, "Document.Rows2[0].Product");
        prodObj.AreValueEqual(785, "Id");
        prodObj.AreValueEqual("Product 785", "Name");
        unitObj = new DataTester(dm, "Document.Rows2[0].Product.Unit");
        unitObj.AreValueEqual(8, "Id");
        unitObj.AreValueEqual("Unit8", "Name");

        var seriesObj = new DataTester(dm, "Document.Rows1[0].Series1");
        seriesObj.IsArray(1);
        seriesObj.AreArrayValueEqual(500, 0, "Id");
        seriesObj.AreArrayValueEqual(5.0, 0, "Price");

        seriesObj = new DataTester(dm, "Document.Rows2[0].Series1");
        seriesObj.IsArray(1);
        seriesObj.AreArrayValueEqual(501, 0, "Id");
        seriesObj.AreArrayValueEqual(10.0, 0, "Price");
    }

	[TestMethod]
	public async Task LoadComplexModelSqlParamsAsync()
	{
		var sqlString =
"""
-- @Id bigint,
-- @Agent bigint

select [Document!TDocument!Object] = null, [Id!!Id] = @Id, [No]='DocNo', [Date]=a2sys.fn_getCurrentDate(),
	[Agent!TAgent!RefId] = @Agent, [Company!TAgent!RefId] = @Agent,
	[Rows1!TRow!Array] = null, [Rows2!TRow!Array] = null;

select [!TRow!Array] = null, [Id!!Id] = 78, [!TDocument.Rows1!ParentId] = @Id, 
	[Product!TProduct!RefId] = 782,
	Qty=cast(4.0 as float), Price=cast(8 as money), [Sum] = cast(32.0 as money),
	[Series1!TSeries!Array] = null

select [!TRow!Array] = null, [Id!!Id] = 79, [!TDocument.Rows2!ParentId] = @Id, 
	[Product!TProduct!RefId] = 785,
	Qty=cast(7.0 as float), Price=cast(2 as money), [Sum] = cast(14.0 as money),
	[Series1!TSeries!Array] = null

-- series for rows
select [!TSeries!Array]=null, [Id!!Id] = 500, [!TRow.Series1!ParentId] = 78, Price=cast(5 as float)
union all
select [!TSeries!Array]=null, [Id!!Id] = 501, [!TRow.Series1!ParentId] = 79, Price=cast(10 as float)

-- maps for product
select [!TProduct!Map] = null, [Id!!Id] = 782, [Name!!Name] = N'Product 782',
	[Unit.Id!TUnit!Id] = 7, [Unit.Name!TUnit!Name] = N'Unit7'
union all
select [!TProduct!Map] = null, [Id!!Id] = 785, [Name!!Name] = N'Product 785',
	[Unit.Id!TUnit!Id] = 8, [Unit.Name!TUnit!Name] = N'Unit8'

-- maps for agent
select [!TAgent!Map] = null, [Id!!Id] = @Agent, [Name!!Name] = 'Agent 512', Code=N'Code 512', [Null] = @Null;
""";

		IDataModel dm = await _dbContext.LoadModelSqlAsync(null, sqlString, (prms) => {
            prms.Add(new SqlParameter("@Id", System.Data.SqlDbType.BigInt) { Value = 123 });
			prms.Add(new SqlParameter("@Agent", System.Data.SqlDbType.BigInt) { Value = 512 });
			prms.Add(new SqlParameter("@Null", System.Data.SqlDbType.NVarChar, 255) { Value = DBNull.Value });
		});

		var md = new MetadataTester(dm);
		md.IsAllKeys("TRoot,TDocument,TRow,TAgent,TProduct,TSeries,TUnit");
		md.HasAllProperties("TRoot", "Document");
		md.HasAllProperties("TDocument", "Id,No,Date,Agent,Company,Rows1,Rows2");
		md.HasAllProperties("TRow", "Id,Product,Qty,Price,Sum,Series1");
		md.HasAllProperties("TProduct", "Id,Name,Unit");
		md.HasAllProperties("TUnit", "Id,Name");

		var docT = new DataTester(dm, "Document");
		docT.AreValueEqual((Int64)123, "Id");
		docT.AreValueEqual("DocNo", "No");

		var agentT = new DataTester(dm, "Document.Agent");
		agentT.AreValueEqual((Int64)512, "Id");
		agentT.AreValueEqual("Agent 512", "Name");
		agentT.AreValueEqual("Code 512", "Code");
        agentT.IsNull("Null");

		agentT = new DataTester(dm, "Document.Company");
		agentT.AreValueEqual((Int64)512, "Id");
		agentT.AreValueEqual("Agent 512", "Name");
		agentT.AreValueEqual("Code 512", "Code");

		var row1T = new DataTester(dm, "Document.Rows1");
		row1T.IsArray(1);
		row1T.AreArrayValueEqual(78, 0, "Id");
		row1T.AreArrayValueEqual(4.0, 0, "Qty");

		var row2T = new DataTester(dm, "Document.Rows2");
		row2T.IsArray(1);
		row2T.AreArrayValueEqual(79, 0, "Id");
		row2T.AreArrayValueEqual(7.0, 0, "Qty");

		var row1Obj = new DataTester(dm, "Document.Rows1[0]");
		row1Obj.AreValueEqual(78, "Id");
		row1Obj.AreValueEqual(4.0, "Qty");
		row1Obj.AllProperties("Id,Qty,Price,Sum,Product,Series1");

		var prodObj = new DataTester(dm, "Document.Rows1[0].Product");
		prodObj.AreValueEqual(782, "Id");
		prodObj.AreValueEqual("Product 782", "Name");
		prodObj.AllProperties("Id,Name,Unit");
		var unitObj = new DataTester(dm, "Document.Rows1[0].Product.Unit");
		unitObj.AreValueEqual(7, "Id");
		unitObj.AreValueEqual("Unit7", "Name");
		unitObj.AllProperties("Id,Name");

		prodObj = new DataTester(dm, "Document.Rows2[0].Product");
		prodObj.AreValueEqual(785, "Id");
		prodObj.AreValueEqual("Product 785", "Name");
		unitObj = new DataTester(dm, "Document.Rows2[0].Product.Unit");
		unitObj.AreValueEqual(8, "Id");
		unitObj.AreValueEqual("Unit8", "Name");

		var seriesObj = new DataTester(dm, "Document.Rows1[0].Series1");
		seriesObj.IsArray(1);
		seriesObj.AreArrayValueEqual(500, 0, "Id");
		seriesObj.AreArrayValueEqual(5.0, 0, "Price");

		seriesObj = new DataTester(dm, "Document.Rows2[0].Series1");
		seriesObj.IsArray(1);
		seriesObj.AreArrayValueEqual(501, 0, "Id");
		seriesObj.AreArrayValueEqual(10.0, 0, "Price");
	}
}
