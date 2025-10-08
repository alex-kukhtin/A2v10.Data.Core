// Copyright © 2015-2018 Oleksandr Kukhtin. All rights reserved.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace A2v10.Data.Tests.Providers;

internal class ProviderTools
{
	internal static void CompareFiles(String file1, String file2)
	{
		var b1 = File.ReadAllBytes(file1);
		var b2 = File.ReadAllBytes(file2);
		Assert.HasCount(b1.Length, b2);
		for (Int32 i = 0; i < b1.Length; i++)
		{
			if (b1[i] != b2[i])
				Assert.AreEqual(b1[i], b2[i]);
		}
	}
}
