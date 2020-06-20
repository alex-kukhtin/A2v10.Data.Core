// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace A2v10.Data.Tests.Providers
{
	internal class ProviderTools
	{
		internal static void CompareFiles(String file1, String file2)
		{
			var b1 = File.ReadAllBytes(file1);
			var b2 = File.ReadAllBytes(file2);
			Assert.AreEqual(b1.Length, b2.Length);
			for (Int32 i = 0; i < b1.Length; i++)
			{
				if (b1[i] != b2[i])
					Assert.IsTrue(b1[i] == b2[i]);
			}
		}
	}
}
