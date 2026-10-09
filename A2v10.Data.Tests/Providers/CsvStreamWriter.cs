// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using A2v10.Data.Providers;
using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests.Providers;

[TestClass]
[TestCategory("Providers")]
public class CsvStreamWriterTest
{
	static readonly String NL = Environment.NewLine;
	static readonly ExternalStreamOptions NoBom = new() { WritePreamble = false };

	[TestInitialize]
	public void Setup()
	{
		Starter.Init();
	}

	// Kestrel response body: not seekable, sync IO throws
	sealed class AsyncOnlyStream(Stream inner) : Stream
	{
		public override Boolean CanRead => false;
		public override Boolean CanSeek => false;
		public override Boolean CanWrite => true;
		public override Int64 Length => throw new NotSupportedException();
		public override Int64 Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
		public override void Flush() => throw new InvalidOperationException("Synchronous IO is disallowed");
		public override void Write(Byte[] buffer, Int32 offset, Int32 count) => throw new InvalidOperationException("Synchronous IO is disallowed");
		public override Task FlushAsync(CancellationToken cancellationToken) => inner.FlushAsync(cancellationToken);
		public override Task WriteAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken) => inner.WriteAsync(buffer, offset, count, cancellationToken);
		public override ValueTask WriteAsync(ReadOnlyMemory<Byte> buffer, CancellationToken cancellationToken = default) => inner.WriteAsync(buffer, cancellationToken);
		public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count) => throw new NotSupportedException();
		public override Int64 Seek(Int64 offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(Int64 value) => throw new NotSupportedException();
	}

	static async Task<Byte[]> WriteBytes(DataTable table, ExternalStreamOptions? options, Boolean asyncOnly = false, IDataLocalizer? localizer = null)
	{
		using var ms = new MemoryStream();
		Stream target = asyncOnly ? new AsyncOnlyStream(ms) : ms;
		var writer = new ExternalDataContext(localizer ?? new NullDataLocalizer()).GetStreamWriter("csv", target, options);
		using var rdr = table.CreateDataReader();
		await writer.OnMetadata(rdr);
		while (await rdr.ReadAsync())
			await writer.OnRecord(rdr);
		await writer.FlushAsync();
		return ms.ToArray();
	}

	static async Task<String> WriteText(DataTable table, ExternalStreamOptions? options = null, IDataLocalizer? localizer = null)
	{
		var bytes = await WriteBytes(table, options ?? NoBom, localizer: localizer);
		return Encoding.UTF8.GetString(bytes);
	}

	static DataTable StringTable(params String?[] values)
	{
		var table = new DataTable();
		table.Columns.Add("S", typeof(String));
		foreach (var v in values)
			table.Rows.Add(v == null ? DBNull.Value : v);
		return table;
	}

	[TestMethod]
	public async Task AllTypes()
	{
		var table = new DataTable();
		table.Columns.Add("String", typeof(String));
		table.Columns.Add("DateTime", typeof(DateTime));
		table.Columns.Add("Int16", typeof(Int16));
		table.Columns.Add("Int32", typeof(Int32));
		table.Columns.Add("Int64", typeof(Int64));
		table.Columns.Add("Decimal", typeof(Decimal));
		table.Columns.Add("Double", typeof(Double));
		table.Columns.Add("Boolean", typeof(Boolean));
		table.Columns.Add("Byte", typeof(Byte));
		table.Columns.Add("Single", typeof(Single));
		table.Columns.Add("Guid", typeof(Guid));
		table.Columns.Add("TimeSpan", typeof(TimeSpan));
		table.Columns.Add("DateTimeOffset", typeof(DateTimeOffset));
		table.Columns.Add("Blob", typeof(Byte[]));
		table.Rows.Add("abc", new DateTime(2026, 10, 9, 14, 30, 15), (Int16)(-5), 42, 1234567890123L, 1234.50M, 1.5D, true,
			(Byte)7, 2.5F, new Guid("01234567-89ab-cdef-0123-456789abcdef"), new TimeSpan(1, 2, 3),
			new DateTimeOffset(2026, 10, 9, 14, 30, 15, TimeSpan.FromHours(3)), new Byte[] { 1, 2 });

		var saved = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = new CultureInfo("uk-UA"); // decimal comma
		try
		{
			var text = await WriteText(table);
			Assert.AreEqual(
				$"String;DateTime;Int16;Int32;Int64;Decimal;Double;Boolean;Byte;Single;Guid;TimeSpan;DateTimeOffset;Blob{NL}" +
				"abc;2026-10-09;-5;42;1234567890123;1234.50;1.5;true;7;2.5;01234567-89ab-cdef-0123-456789abcdef;01:02:03;2026-10-09;(blob)",
				text);
		}
		finally
		{
			CultureInfo.CurrentCulture = saved;
		}
	}

	[TestMethod]
	public async Task DateTimeFormat()
	{
		var table = new DataTable();
		table.Columns.Add("D", typeof(DateTime));
		table.Columns.Add("O", typeof(DateTimeOffset));
		table.Rows.Add(new DateTime(2026, 10, 9, 14, 30, 15), new DateTimeOffset(2026, 10, 9, 14, 30, 15, TimeSpan.FromHours(3)));

		Assert.AreEqual($"D;O{NL}2026-10-09;2026-10-09", await WriteText(table));
		var text = await WriteText(table, NoBom with { DateFormat = ExternalDateFormat.DateTime });
		Assert.AreEqual($"D;O{NL}2026-10-09 14:30:15;2026-10-09 14:30:15", text);
	}

	[TestMethod]
	public async Task NullValues()
	{
		var table = new DataTable();
		table.Columns.Add("S", typeof(String));
		table.Columns.Add("I", typeof(Int32));
		table.Columns.Add("D", typeof(DateTime));
		table.Rows.Add(DBNull.Value, DBNull.Value, DBNull.Value);
		Assert.AreEqual($"S;I;D{NL};;", await WriteText(table));
	}

	[TestMethod]
	public async Task Escaping()
	{
		var table = StringTable("a;b", "say \"hi\"", "l1\nl2", "l1\r\nl2", "a,b");
		Assert.AreEqual($"S{NL}\"a;b\"{NL}\"say \"\"hi\"\"\"{NL}\"l1\nl2\"{NL}\"l1\r\nl2\"{NL}a,b", await WriteText(table));
	}

	[TestMethod]
	public async Task Delimiter()
	{
		var table = StringTable("a;b", "a,b");
		Assert.AreEqual($"S{NL}a;b{NL}\"a,b\"", await WriteText(table, NoBom with { Delimiter = ',' }));
	}

	[TestMethod]
	public async Task FlattenLineBreaks()
	{
		var table = new DataTable();
		table.Columns.Add("Two\nLines", typeof(String));
		table.Rows.Add("a\r\nb\rc\nd");
		table.Rows.Add("say \"hi\"\r\nthere");
		var text = await WriteText(table, NoBom with { FlattenLineBreaks = true });
		Assert.AreEqual($"Two Lines{NL}a b c d{NL}\"say \"\"hi\"\" there\"", text);
	}

	[TestMethod]
	public async Task SkipHeader()
	{
		var options = NoBom with { SkipHeader = true };
		Assert.AreEqual($"r1{NL}r2", await WriteText(StringTable("r1", "r2"), options));
		Assert.AreEqual("", await WriteText(StringTable(), options));
	}

	[TestMethod]
	public async Task Localization()
	{
		var table = new DataTable();
		table.Columns.Add("@[Item1]", typeof(String));
		table.Columns.Add("I", typeof(Int32));
		table.Rows.Add("@[Item1]", 1);
		table.Rows.Add("Item1", 2);
		// values only: the header is written as is
		Assert.AreEqual($"@[Item1];I{NL}Item 1;1{NL}Item1;2", await WriteText(table, localizer: new TestLocalizer()));
		Assert.AreEqual($"@[Item1];I{NL}@[Item1];1{NL}Item1;2", await WriteText(table));
	}

	[TestMethod]
	public async Task HeaderOnly()
	{
		Assert.AreEqual("S", await WriteText(StringTable()));
	}

	[TestMethod]
	public async Task Preamble()
	{
		Byte[] bom = [0xEF, 0xBB, 0xBF];
		Byte[] body = Encoding.UTF8.GetBytes($"S{NL}r1");
		foreach (var asyncOnly in new[] { false, true })
		{
			CollectionAssert.AreEqual(bom.Concat(body).ToArray(), await WriteBytes(StringTable("r1"), null, asyncOnly));
			CollectionAssert.AreEqual(body, await WriteBytes(StringTable("r1"), NoBom, asyncOnly));
		}
	}

	[TestMethod]
	public async Task Encoding1251()
	{
		var enc = Encoding.GetEncoding(1251);
		var bytes = await WriteBytes(StringTable("Привіт"), new ExternalStreamOptions() { Encoding = enc, WritePreamble = false });
		CollectionAssert.AreEqual(enc.GetBytes($"S{NL}Привіт"), bytes);
	}

	[TestMethod]
	public void InvalidEncoding()
	{
		var ed = new ExternalDataContext(new NullDataLocalizer());
		using var ms = new MemoryStream();
		Assert.ThrowsExactly<ExternalDataException>(() =>
			ed.GetStreamWriter("csv", ms, new ExternalStreamOptions() { Encoding = Encoding.GetEncoding(1251) }));
		Assert.ThrowsExactly<ExternalDataException>(() =>
			ed.GetStreamWriter("csv", ms, new ExternalStreamOptions() { Encoding = Encoding.Unicode, WritePreamble = false }));
	}

	[TestMethod]
	public void InvalidFormat()
	{
		using var ms = new MemoryStream();
		Assert.ThrowsExactly<ExternalDataException>(() => new ExternalDataContext(new NullDataLocalizer()).GetStreamWriter("dbf", ms));
	}
}
