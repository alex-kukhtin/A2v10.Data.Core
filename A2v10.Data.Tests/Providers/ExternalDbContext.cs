// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using A2v10.Data.Tests.Configuration;
using A2v10.Data.Providers;

namespace A2v10.Data.Tests.Providers;

[TestClass]
[TestCategory("Providers")]
public class ExternalDbContext
{

    readonly IDbContext _dbContext;
    public ExternalDbContext()
    {
        _dbContext = Starter.Create();
    }

    [TestInitialize]
    public void Setup()
	{
		Starter.Init();
    }

	[TestMethod]
	public async Task FillModel()
	{
		var ed = new ExternalDataContext(new NullDataLocalizer());
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[CrossModel.Load]");
		var writer = ed.GetWriter(dm, "dbf", Encoding.GetEncoding(866));

		using (var file = File.Open("testfiles/fillmodel.dbf", FileMode.Create)) { 
			writer.Write(file);
		}
		using (var file = File.Open("testfiles/fillmodel.dbf", FileMode.Open))
		{
			var rdr = ed.GetReader("auto", Encoding.GetEncoding(866), "testfiles/fillmodel.dbf");
			var eo = rdr.CreateDataModel(file);
			var rows = eo.Eval<List<Object>>("Rows")
				?? throw new InvalidCastException();	
			Assert.HasCount(2, rows);
			if (rows[0] is not ExpandoObject row1)
                throw new InvalidCastException();
			Assert.AreEqual(10, row1.Eval<Decimal>("Id"));
            Assert.AreEqual("S1", row1.Eval<String>("S1"));
        }
    }


	[TestMethod]
	public async Task WriteCsvWithEmptyFields()
	{
		var ed = new ExternalDataContext(new NullDataLocalizer());
		var dm = await _dbContext.LoadModelAsync(null, "a2test.[CsvEmptyFields.Load]");
		var writer = ed.GetWriter(dm, "csv", Encoding.GetEncoding(866));

		using (var file = File.Open("testfiles/emptyfields.csv", FileMode.Create))
		{
			writer.Write(file);
		}
		using (var file = File.Open("testfiles/emptyfields.csv", FileMode.Open))
		{
			var rdr = ed.GetReader("auto", Encoding.GetEncoding(866), "testfiles/emptyfields.csv");
			var eo = rdr.CreateDataModel(file);
			var rows = eo.Eval<List<Object>>("Rows")
				?? throw new InvalidCastException();
			Assert.HasCount(3, rows);
		}
	}

	static readonly String NL = Environment.NewLine;

	[TestMethod]
	public async Task LoadStreamSqlCsv()
	{
		const String sql = """
		select Id = @Id, [Name] = N'a;b', D = cast('2026-10-09' as date),
			DT = cast('2026-10-09T14:30:15' as datetime), DT2 = cast('2026-10-09T14:30:15' as datetime2(0))
		union all
		select 2, null, null, null, null;
		""";
		using var ms = new MemoryStream();
		var options = new ExternalStreamOptions() { DateFormat = ExternalDateFormat.ByType, WritePreamble = false };
		var writer = new ExternalDataContext(new NullDataLocalizer()).GetStreamWriter("csv", ms, options);
		await _dbContext.LoadStreamSqlAsync(null, sql, new { Id = 1 }, writer.OnMetadata, writer.OnRecord);
		await writer.FlushAsync();
		Assert.AreEqual($"Id;Name;D;DT;DT2{NL}1;\"a;b\";2026-10-09;2026-10-09 14:30:15;2026-10-09 14:30:15{NL}2;;;;",
			Encoding.UTF8.GetString(ms.ToArray()));
	}

	[TestMethod]
	public async Task LoadStreamCsv()
	{
		using var ms = new MemoryStream();
		var writer = new ExternalDataContext(new NullDataLocalizer()).GetStreamWriter("csv", ms, new ExternalStreamOptions() { WritePreamble = false });
		await _dbContext.LoadStreamAsync(null, "a2test.[CsvEmptyFields.Load]", null, writer.OnMetadata, writer.OnRecord);
		await writer.FlushAsync();
		// no model markers parsing: the marker column is written as is
		Assert.AreEqual($"Rows!TRow!Array;F1;F2;F3;F4{NL};;;1;3{NL};;2;4;6{NL};3;4;7;8",
			Encoding.UTF8.GetString(ms.ToArray()));
	}

	[TestMethod]
	public async Task LoadStreamException()
	{
		const String sql = "select top(100000) a.[object_id] from sys.all_objects a cross join sys.all_objects b;";
		var count = 0;
		await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
			_dbContext.LoadStreamSqlAsync(null, sql, null,
				rdr => Task.CompletedTask,
				rdr => ++count == 10 ? throw new InvalidOperationException("stop") : Task.CompletedTask));
		Assert.AreEqual(10, count);
	}
}
