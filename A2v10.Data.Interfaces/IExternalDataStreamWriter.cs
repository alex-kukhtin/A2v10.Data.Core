// Copyright © 2026 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.Data.Interfaces;

public enum ExternalDateFormat
{
	Date,      // yyyy-MM-dd
	DateTime,  // yyyy-MM-dd HH:mm:ss
	ByType     // SQL date → yyyy-MM-dd, other date types → yyyy-MM-dd HH:mm:ss
}

public record ExternalStreamOptions
{
	public Encoding Encoding { get; init; } = Encoding.UTF8;
	public Char Delimiter { get; init; } = ';';
	public Boolean SkipHeader { get; init; }
	// \r\n, \r, \n → one space each
	public Boolean FlattenLineBreaks { get; init; }
	public ExternalDateFormat DateFormat { get; init; }
	// UTF-8 only; any other encoding requires false
	public Boolean WritePreamble { get; init; } = true;
}

// Delegates for IDbContext.LoadStream(Sql)Async; FlushAsync after the load completes
public interface IExternalDataStreamWriter
{
	Task OnMetadata(IDataReader reader);
	Task OnRecord(IDataReader reader);
	Task FlushAsync();
}
