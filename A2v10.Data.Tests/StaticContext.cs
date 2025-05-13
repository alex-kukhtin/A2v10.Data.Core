// Copyright © 2025 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data.Core.Extensions;
using A2v10.Data.Tests.Configuration;
using System.Threading.Tasks;

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
		await _staticDbContext.ExecuteNonQueryAsync(null, "a2test.StaticNonQuery", (prms) =>
		{
			prms.AddBigInt("@Id", 10)
				.AddString("@Text", "Test Text");
        });

		await _staticDbContext.ExecuteReaderAsync(null, "a2test.[StaticNonQuery.Load]", (prms) =>
		{
			prms.AddBigInt("@Id", 10);
		}, 
		(rdrNo, rdr) =>
		{
			Assert.AreEqual("Test Text", rdr.GetString(0));	
        });
    }

    [TestMethod]
    public void ExecNonAsync()
    {
        _staticDbContext.ExecuteNonQuery(null, "a2test.StaticNonQuery", (prms) =>
        {
            prms.AddBigInt("@Id", 10)
                .AddString("@Text", "Test Text");
        });

        _staticDbContext.ExecuteReader(null, "a2test.[StaticNonQuery.Load]", (prms) =>
        {
            prms.AddBigInt("@Id", 10);
        },
        (rdrNo, rdr) =>
        {
            Assert.AreEqual("Test Text", rdr.GetString(0));
        });
    }


    [TestMethod]
    public async Task ExecSqlAsync()
    {
        await _staticDbContext.ExecuteNonQuerySqlAsync(null, "update a2test.[STATIC] set [Text] = @Text where Id = @Id", (prms) =>
        {
            prms.AddBigInt("@Id", 10)
                .AddString("@Text", "Test Text SQL");
        });

        await _staticDbContext.ExecuteReaderSqlAsync(null, "select [Text] from a2test.[STATIC] where [Id] = @Id", (prms) =>
        {
            prms.AddBigInt("@Id", 10);
        },
        (rdrNo, rdr) =>
        {
            Assert.AreEqual("Test Text SQL", rdr.GetString(0));
        });
    }

    [TestMethod]
    public void ExecSqlSync()
    {
        _staticDbContext.ExecuteNonQuerySql(null, "update a2test.[STATIC] set [Text] = @Text where Id = @Id", (prms) =>
        {
            prms.AddBigInt("@Id", 10)
                .AddString("@Text", "Test Text SQL");
        });

        _staticDbContext.ExecuteReaderSql(null, "select [Text] from a2test.[STATIC] where [Id] = @Id", (prms) =>
        {
            prms.AddBigInt("@Id", 10);
        },
        (rdrNo, rdr) =>
        {
            Assert.AreEqual("Test Text SQL", rdr.GetString(0));
        });
    }
}
