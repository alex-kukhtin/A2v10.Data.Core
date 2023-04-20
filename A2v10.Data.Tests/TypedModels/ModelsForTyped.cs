using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.Data.Tests;

public record Series
{
    public Int64 Id { get; set; }
    public Double Price { get; set; }
}

public record Product
{
    public Int64 Id { get; set; }
    public String? Name { get; set; }
}

public record Row
{	
	public Int64 Id { get; set; }
	public Double Qty { get; set; }
    public Decimal Price { get; set; }
    public Decimal Sum { get; set; }
    public Product? Product { get; set; }
    public List<Series> Series1 { get; init; } = new();
}

public record Agent
{    
	public Int64 Id { get; set; }
	public String? Name { get; set; }
    public String? Code { get; set; }
}

public record Document
{
	public Int64 Id { get; set; }
	public String? No { get; set; }
	public DateTime Date { get; set; }
	public Agent? Agent { get; set; }
	public Agent? Company { get; set; }
	public List<Row> Rows1 { get; init; } = new();
    public List<Row> Rows2 { get; init; } = new();
}

public record LoadedDocument
{
	public Document? Document { get; set; }
	public List<Agent> Agents { get; init; } = new();
	public List<Product> Products { get; init; } = new();
}

public record LoadedDocuments
{
    public List<Document> Documents { get; set; } = new();
    public List<Agent> Agents { get; init; } = new();
    public List<Product> Products { get; init; } = new();
}

public record RMethodData
{
    public Int64 Id { get; set; }
    public String? Code { get; set; }

}
public record RMethod
{
	public List<RMethodData> Data { get; set; } = new();
    public Int32 Id { get; set; }
    public String? Name { get; set; }
}
public record RRow
{
    public Int32? Id { get; set; }
	public Dictionary<String, RMethod> Methods { get; set; } = new();
}
public record RDocument
{
	public Int32 Id { get; set; }
	public String? Name { get; set; }
	public List<RRow> Rows { get; init; } = new();
}
public record RowsMethods
{
	public RDocument? Document { get; set; }
}
