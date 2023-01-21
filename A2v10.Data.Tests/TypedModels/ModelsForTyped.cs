using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.Data.Tests;

//[TypeName("TRow")]
public record Row
{	
	public Int64 Id { get; set; }
}

public record Document
{
	public Int64 Id { get; set; }
	public String? No { get; set; }

	public List<Row> Rows { get; init; } = new();
}

//[TypeName("TDocument")]
public record LoadedDocument
{
	public Document Document { get; init; } = new();
}
