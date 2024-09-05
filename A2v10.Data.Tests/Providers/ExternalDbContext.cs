// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Providers;

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
		var ed = new ExternalDataContext();
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
			Assert.AreEqual(2, rows.Count);
			if (rows[0] is not ExpandoObject row1)
                throw new InvalidCastException();
			Assert.AreEqual(10, row1.Eval<Decimal>("Id"));
            Assert.AreEqual("S1", row1.Eval<String>("S1"));
        }
    }


	[TestMethod]
	public async Task WriteCsvWithEmptyFields()
	{
		var ed = new ExternalDataContext();
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
			Assert.AreEqual(3, rows.Count);
		}
	}
}
