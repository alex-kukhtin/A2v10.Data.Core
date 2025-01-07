// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;
using System.IO;
using A2v10.Data.Providers;
using A2v10.Data.Providers.Csv;
using A2v10.Data.Tests.Configuration;


namespace A2v10.Data.Tests.Providers;

[TestClass]
[TestCategory("Providers")]
public class CsvReaderTest
{
	[TestInitialize]
	public void Setup()
	{
		Starter.Init();
	}

	[TestMethod]
	public void CsvReadSimpleFile()
	{
		var f = new DataFile();
		var rdr = new CsvReader(f);

		using (var file = File.Open("testfiles/simple.csv", FileMode.Open))
		{
			rdr.Read(file);
		}

		var wrt = new CsvWriter(f);
		using (var file = File.Create("testfiles/output.csv"))
		{
			wrt.Write(file);
		}

		ProviderTools.CompareFiles("testfiles/simple.csv", "testfiles/output.csv");
	}

	[TestMethod]
	public void CsvReadSomeRecordsFile()
	{
		var f = new DataFile();
		var rdr = new CsvReader(f);

		using (var file = File.Open("testfiles/records.csv", FileMode.Open))
		{
			rdr.Read(file);
		}

		var wrt = new CsvWriter(f);
		using (var file = File.Create("testfiles/recordsout.csv"))
		{
			wrt.Write(file);
		}

		var nf = new DataFile();
		var nrdr = new CsvReader(nf);
		using (var file = File.Open("testfiles/recordsout.csv", FileMode.Open))
		{
			nrdr.Read(file);
		}
		Assert.AreEqual(f.FieldCount, nf.FieldCount);
		Assert.AreEqual(f.NumRecords, nf.NumRecords);
		for (var c = 0; c < f.FieldCount; c++)
		{
			var f1 = f.GetField(c);
			var f2 = nf.GetField(c);
			Assert.AreEqual(f1.Name, f2.Name);
		}

		for (var r = 0; r < f.NumRecords; r++)
		{
			var r1 = f.GetRecord(r);
			var r2 = nf.GetRecord(r);
			for (var c = 0; c < f.FieldCount; c++)
			{
				var v1 = r1.DataFields[c];
				var v2 = r2.DataFields[c];
				Assert.AreEqual(v1.StringValue, v2.StringValue);
			}
		}
	}


	[TestMethod]
	public void CsvReadExternalFile()
	{
		/*
		var f = new DataFile();
		var rdr = new CsvReader(f);

		using (var file = File.Open("testfiles/external.csv", FileMode.Open))
		{
			rdr.Read(file);
		}
		var wrt = new CsvWriter(f);
		using (var file = File.Create("testfiles/extenral_output.csv"))
		{
			wrt.Write(file);
		}
		*/
	}

    [TestMethod]
    public void CsvReadZeroSpaceFile()
    {
        var f = new DataFile();
        var rdr = new CsvReader(f);

		using (var file = File.Open("testfiles/zerospace.csv", FileMode.Open))
		{
			rdr.Read(file);
			Assert.AreEqual(1, f.Records.Count());
			Assert.AreEqual(13, f.FieldCount);

			var r1 = f.GetRecord(0);
			var v1 = r1.FieldValue(2)?.ToString();
            Assert.AreEqual("UA9999999999999999", v1);
			Assert.AreEqual(18, v1?.Length);

            v1 = r1.FieldValue(0)?.ToString();
			Assert.AreEqual("BO122222", v1);
            Assert.AreEqual(8, v1?.Length);
        }
    }
}
