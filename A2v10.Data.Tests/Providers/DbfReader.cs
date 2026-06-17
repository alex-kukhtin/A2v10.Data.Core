// Copyright © 2015-2026 Oleksandr Kukhtin. All rights reserved.

using System.IO;
using System.Text;
using A2v10.Data.Providers;
using A2v10.Data.Providers.Dbf;
using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests.Providers
{
	[TestClass]
	[TestCategory("Providers")]
	public class DbfReaderTest
	{
		[TestInitialize]
		public void Setup()
		{
			Starter.Init();
		}

		[TestMethod]
		public void DbfReadSimpleFile()
		{
			var f = new DataFile()
			{
				Encoding = Encoding.GetEncoding(866)
			};
			var rdr = new DbfReader(f);

			using (var file = File.Open("testfiles/simple.dbf", FileMode.Open))
			{
				rdr.Read(file);
			}

			var wrt = new DbfWriter(f);

			using (var file = File.Create("testfiles/output.dbf"))
			{
				wrt.Write(file);
			}

			ProviderTools.CompareFiles("testfiles/simple.dbf", "testfiles/output.dbf");
		}

		[TestMethod]
		public void DbfReadAutoEncoding()
		{
			var f = new DataFile();

			var rdr = new DbfReader(f);

			using (var file = File.Open("testfiles/ENCODING.dbf", FileMode.Open))
			{
				rdr.Read(file);
			}

			Assert.AreEqual("ID|FTEXT|FNUM|FDATE|FBOOL", String.Join('|', f.FieldNames));

			var wrt = new DbfWriter(f);

			using (var file = File.Create("testfiles/output.dbf"))
			{
				wrt.Write(file);
			}

			ProviderTools.CompareFiles("testfiles/ENCODING.dbf", "testfiles/output.dbf");
		}

	}
}
