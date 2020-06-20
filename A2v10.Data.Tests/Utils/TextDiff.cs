// Copyright © 2018 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace A2v10.Data.Tests
{
	public static class StringTools
	{
		public static String StringDiff(String s1, String s2)
		{
			List<String> diff;
			IEnumerable<String> set1 = s1.Split(' ').Distinct();
			IEnumerable<String> set2 = s2.Split(' ').Distinct();

			if (set2.Count() > set1.Count())
			{
				diff = set2.Except(set1).ToList();
			}
			else
			{
				diff = set1.Except(set2).ToList();
			}
			return String.Join(" ", diff);
		}
	}
}
