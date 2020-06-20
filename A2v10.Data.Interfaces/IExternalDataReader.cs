// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System.Dynamic;
using System.IO;

namespace A2v10.Data.Interfaces
{
	public interface IExternalDataReader
	{
		IExternalDataFile Read(Stream stream);
		ExpandoObject ParseFile(Stream stream, ITableDescription table);
		ExpandoObject CreateDataModel(Stream stream);
	}
}
