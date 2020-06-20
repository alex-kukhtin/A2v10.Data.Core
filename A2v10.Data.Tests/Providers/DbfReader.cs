// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System.IO;
using System.Text;

using A2v10.Data.Providers.Dbf;
using A2v10.Data.Tests.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.Data.Providers
{
	[TestClass]
	[TestCategory("Providers")]
	public class DbfReaderTest
	{
		[TestMethod]
		public void DbfReadSimpleFile()
		{
			var f = new DataFile()
			{
				Encoding = Encoding.GetEncoding(866)
			};
			var rdr = new DbfReader(f);

			using (var file = File.Open("../../testfiles/simple.dbf", FileMode.Open))
			{
				rdr.Read(file);
			}

			var wrt = new DbfWriter(f);
			using (var file = File.Open("../../testfiles/output.dbf", FileMode.OpenOrCreate|FileMode.Truncate))
			{
				wrt.Write(file);
			}

			ProviderTools.CompareFiles("../../testfiles/simple.dbf", "../../testfiles/output.dbf");
		}

		[TestMethod]
		public void DbfReadAutoEncoding()
		{
			var f = new DataFile()
			{
				Encoding = null // AUTO
			};

			var rdr = new DbfReader(f);

			using (var file = File.Open("../../testfiles/ENCODING.dbf", FileMode.Open))
			{
				rdr.Read(file);
			}

			var wrt = new DbfWriter(f);
			using (var file = File.Open("../../testfiles/output.dbf", FileMode.OpenOrCreate | FileMode.Truncate))
			{
				wrt.Write(file);
			}

			ProviderTools.CompareFiles("../../testfiles/ENCODING.dbf", "../../testfiles/output.dbf");
		}

	}
}
