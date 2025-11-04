// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using System.Threading.Tasks;

using A2v10.Data.Core.Extensions;
using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests;

[TestCategory("Static DB Context")]
[TestClass]
public class TestStaticDbContext
{
	readonly IStaticDbContext _staticDbContext;

    public TestStaticDbContext()
	{
		_staticDbContext = Starter.CreateStatic();

    }

	[TestMethod]
	public async Task ExecAsync()
	{
        var dt = DateTime.Today;
		await _staticDbContext.ExecuteNonQueryAsync(null, "a2test.StaticNonQuery", (prms) =>
		{
			_staticDbContext.ParameterBuilder(prms).AddBigInt("@Id", 10)
				.AddString("@Text", "Test Text")
                .AddInt("@Int", 73)
                .AddDate("@Date", dt)
                .AddBit("@Bit", true);
        });

		await _staticDbContext.ExecuteReaderAsync(null, "a2test.[StaticNonQuery.Load]", (prms) =>
		{
            _staticDbContext.ParameterBuilder(prms).AddBigInt("@Id", 10);
		}, 
		(rdrNo, rdr) =>
		{
			Assert.AreEqual("Test Text", rdr.GetString(0));
            Assert.AreEqual(73, rdr.GetInt32(1));
            Assert.AreEqual(dt, rdr.GetDateTime(2));
            Assert.IsTrue(rdr.GetBoolean(3));
        });
    }

    [TestMethod]
    public async Task ExecFromQueryAsync()
    {
        var dt = DateTime.Today;
        var query = new ExpandoBuilder()
        .Add("Text", "Test Text")
        .Add("Int", 73)
        .Add("Date", dt)
        .Add("Bit", true)
        .Build();

        await _staticDbContext.ExecuteNonQueryAsync(null, "a2test.StaticNonQuery", (prms) =>
        {
            _staticDbContext.ParameterBuilder(prms).AddBigInt("@Id", 10)
                .AddStringFromQuery("@Text", query)
                .AddFromQuery("@Int", query)
                .AddFromQuery("@Date", query)
                .AddFromQuery("@Bit", query);
        });

        await _staticDbContext.ExecuteReaderAsync(null, "a2test.[StaticNonQuery.Load]", (prms) =>
        {
            _staticDbContext.ParameterBuilder(prms).AddBigInt("@Id", 10);
        },
        (rdrNo, rdr) =>
        {
            Assert.AreEqual("Test Text", rdr.GetString(0));
            Assert.AreEqual(73, rdr.GetInt32(1));
            Assert.AreEqual(dt, rdr.GetDateTime(2));
            Assert.IsTrue(rdr.GetBoolean(3));
        });
    }

    [TestMethod]
    public void ExecNonAsync()
    {
        _staticDbContext.ExecuteNonQuery(null, "a2test.StaticNonQuery", (prms) =>
        {
            _staticDbContext.ParameterBuilder(prms).AddBigInt("@Id", 10)
                .AddString("@Text", "Test Text", 50)
                .AddInt("@Int", null)
                .AddDate("@Date", null)
                .AddBit("@Bit", null);
        });

        _staticDbContext.ExecuteReader(null, "a2test.[StaticNonQuery.Load]", (prms) =>
        {
            _staticDbContext.ParameterBuilder(prms).AddBigInt("@Id", 10);
        },
        (rdrNo, rdr) =>
        {
            Assert.AreEqual("Test Text", rdr.GetString(0));
            Assert.IsTrue(rdr.IsDBNull(1));
            Assert.IsTrue(rdr.IsDBNull(2));
            Assert.IsTrue(rdr.IsDBNull(3));
        });
    }


    [TestMethod]
    public async Task ExecSqlAsync()
    {
        await _staticDbContext.ExecuteNonQuerySqlAsync(null, "update a2test.[STATIC] set [Text] = @Text where Id = @Id", (prms) =>
        {
            _staticDbContext.ParameterBuilder(prms)
                .AddBigInt("@Id", 10)
                .AddString("@Text", "Test Text SQL");
        });

        await _staticDbContext.ExecuteReaderSqlAsync(null, "select [Text] from a2test.[STATIC] where [Id] = @Id", (prms) =>
        {
            _staticDbContext.ParameterBuilder(prms).AddBigInt("@Id", 10);
        },
        (rdrNo, rdr) =>
        {
            Assert.AreEqual("Test Text SQL", rdr.GetString(0));
        });
    }

    [TestMethod]
    public void ExecSqlSync()
    {

        var dt = DateTime.Today;
        var query = new ExpandoBuilder()
        .Add("Text", "Test Text")
        .Add("Int", 73)
        .Add("Date", dt)
        .Add("Bit", true)
        .Build();

        _staticDbContext.ExecuteNonQuerySql(null, "update a2test.[STATIC] set [Text] = @Text, [Date] = @Date where Id = @Id", (prms) =>
        {
            prms.AddBigInt("@Id", 10)
                .AddString("@Text", "Test Text SQL")
                .AddDateFromQuery("@Date", query, "Date");
        });

        _staticDbContext.ExecuteReaderSql(null, "select [Text], [Date] from a2test.[STATIC] where [Id] = @Id", (prms) =>
        {
            prms.AddBigInt("@Id", 10);
        },
        (rdrNo, rdr) =>
        {
            Assert.AreEqual("Test Text SQL", rdr.GetString(0));
            Assert.AreEqual(dt, rdr.GetDateTime(1));
        });
    }
}
