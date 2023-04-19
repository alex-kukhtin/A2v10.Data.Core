using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2v10.Data.Tests;

public record Unit
{
    public Int64 Id { get; set; }
    public String? Name { get; set; }
}

public record Product
{
    public Int64 Id { get; set; }
    public String? Name { get; set; }
	public Unit? Unit { get; set; }	
}

public record Row
{	
	public Int64 Id { get; set; }
	public Double Qty { get; set; }
    public Decimal Price { get; set; }
    public Decimal Sum { get; set; }
    public Product? Product { get; set; }
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
	public List<Row> Rows { get; init; } = new();
}

public record LoadedDocument
{
	public Document Document { get; init; } = new();
}
