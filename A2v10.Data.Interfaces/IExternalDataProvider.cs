// Copyright © 2015-2026 Oleksandr Kukhtin. All rights reserved.

using System.IO;
using System.Text;

namespace A2v10.Data.Interfaces;
public interface IExternalDataProvider
{
	IExternalDataReader GetReader(String format, Encoding? enc, String? fileName);
	IExternalDataWriter GetWriter(IDataModel model, String format, Encoding enc);
	IExternalDataStreamWriter GetStreamWriter(String format, Stream stream, ExternalStreamOptions? options = null);
}

