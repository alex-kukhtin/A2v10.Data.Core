// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.Data.Providers.Csv;

public class CsvStreamWriter : IExternalDataStreamWriter
{
	private readonly Stream _stream;
	private readonly StreamWriter _writer;
	private readonly DataFile _file;
	private readonly CsvWriter _csv;
	private readonly ExternalStreamOptions _options;
	private Boolean _lineWritten;

	public CsvStreamWriter(Stream stream, ExternalStreamOptions options, IDataLocalizer localizer)
	{
		var enc = options.Encoding;
		var utf8 = enc.CodePage == Encoding.UTF8.CodePage;
		if (!utf8 && (options.WritePreamble || enc.GetPreamble().Length > 0))
			throw new ExternalDataException($"Encoding '{enc.WebName}'. Stream writer supports UTF-8, or an encoding without a preamble with WritePreamble = false");
		_stream = stream;
		_options = options;
		// The preamble is written by WriteHeader, never by StreamWriter: it decides by CanSeek/Position.
		// Never disposed: Dispose flushes the stream synchronously, Kestrel throws on sync IO.
		_writer = new StreamWriter(stream, utf8 ? new UTF8Encoding(false) : enc, 65536, leaveOpen: true);
		_file = new DataFile(enc)
		{
			Format = DataFileFormat.csv,
			Delimiter = options.Delimiter,
			Localizer = localizer,
			OnHeader = WriteHeader,
			OnRecord = WriteRecord
		};
		_csv = new CsvWriter(_file);
	}

	public Task OnMetadata(IDataReader reader) => _file.ReadMetadataAsync(reader, _options.DateFormat);
	public Task OnRecord(IDataReader reader) => _file.ReadRecordAsync(reader);
	public Task FlushAsync() => _writer.FlushAsync();

	async Task WriteHeader()
	{
		if (_options.WritePreamble)
			await _stream.WriteAsync(Encoding.UTF8.GetPreamble());
		if (_options.SkipHeader)
			return;
		if (_options.FlattenLineBreaks)
			foreach (var f in _file.Fields)
				f.Name = Flatten(f.Name);
		await _csv.WriteHeaderAsync(_writer);
		_lineWritten = true;
	}

	async Task WriteRecord(Record record)
	{
		if (_options.FlattenLineBreaks)
			foreach (var fd in record.DataFields)
				if (fd.StringValue != null)
					fd.StringValue = Flatten(fd.StringValue);
		if (_lineWritten)
			await _writer.WriteAsync(_writer.NewLine);
		await _csv.WriteRecordAsync(_writer, record);
		_lineWritten = true;
	}

	static String Flatten(String str) => str.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ');
}
