// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Text;

namespace A2v10.Data.Interfaces
{

	public interface IExternalDataProvider
	{
		IExternalDataReader GetReader(String format, Encoding enc, String fileName);
		IExternalDataWriter GetWriter(IDataModel model, String format, Encoding enc);
	}
}
