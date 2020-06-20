// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Data.Providers.Csv;
using A2v10.Data.Tests.Providers;
using A2v10.Data.Providers.Xml;
using System.Text;

namespace A2v10.Data.Providers
{
	[TestClass]
	[TestCategory("Providers")]
	public class XmlReaderTest
	{
		[TestMethod]
		public void XmlReadSimpleFile()
		{
			var f = new DataFile();
			var rdr = new XmlReader(f);

			using (var file = File.Open("../../testfiles/simple.xml", FileMode.Open))
			{
				rdr.Read(file);
			}

			var wrt = new XmlWriter(f);
			f.Encoding = Encoding.GetEncoding(1251);
			wrt.RootElement = "ROWDATA";
			wrt.RowElement = "ROW";

			using (var file = File.Create("../../testfiles/output.xml"))
			{
				wrt.Write(file);
			}

			ProviderTools.CompareFiles("../../testfiles/simple.xml", "../../testfiles/output.xml");
		}
	}
}
