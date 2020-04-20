// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.IO;

namespace A2v10.Data.Interfaces
{
	public interface IExternalDataReader
	{
		IExternalDataFile Read(Stream stream);
	}
}
