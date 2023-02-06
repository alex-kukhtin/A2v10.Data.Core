// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.


using System.IO;
using System.Text;

using A2v10.Data.Providers.Csv;
using A2v10.Data.Providers.Xml;
using A2v10.Data.Tests.Configuration;
using A2v10.Data.Tests.Providers;

namespace A2v10.Data.Providers
{
	[TestClass]
	[TestCategory("Providers")]
	public class XmlReaderTest
	{

		[TestInitialize]
		public void Setup()
		{
			Starter.Init();
		}

		[TestMethod]
		public void XmlReadSimpleFile()
		{
			var f = new DataFile(Encoding.GetEncoding(1251));
			var rdr = new XmlReader(f);

			using (var file = File.Open("testfiles/simple.xml", FileMode.Open))
			{
				rdr.Read(file);
			}

			var wrt = new XmlWriter(f, "ROWDATA", "ROW");

			using (var file = File.Create("testfiles/output.xml"))
			{
				wrt.Write(file);
			}

			ProviderTools.CompareFiles("testfiles/simple.xml", "testfiles/output.xml");
		}
	}
}
