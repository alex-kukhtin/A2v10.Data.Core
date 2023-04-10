// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System.Data;
using System.Dynamic;
using System.Threading.Tasks;
using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests;


public class Info1
{
	public Guid? GuidValue;
	public String? StringValue;
	public Int32 IntValue;
}

[TestClass]
[TestCategory("Raw Recordsets")]
public class RawRecordset
{
	private readonly IDbContext _dbContext;
	public RawRecordset()
	{
		_dbContext = Starter.Create();
	}

	static Info1 ReadInfo(IDataReader rdr)
	{
		var info = new Info1();
		info.GuidValue = !rdr.IsDBNull(0) ? rdr.GetGuid(0) : null;
		info.IntValue = rdr.GetInt32(1);
		info.StringValue = rdr.GetString(2);
		return info;
	}

	[TestMethod]
	public void LoadRaw()
	{
		Info1? info1 = new();
		var list = new List<Info1>();

		Guid id = Guid.NewGuid();
		var prms = new ExpandoObject()
		{
			{ "Id", id }
		};

		_dbContext.LoadRaw(null, "a2test.[RawData.Load]", prms,
			(rno, rdr) =>
			{
				if (rno == 0)
					info1 = ReadInfo(rdr);
				else if (rno == 1)
					list.Add(ReadInfo(rdr));
			});
		Assert.AreEqual(id, info1.GuidValue);
		Assert.AreEqual(23, info1.IntValue);
		Assert.AreEqual("string", info1.StringValue);
		Assert.AreEqual(2, list.Count);
		Assert.AreEqual(id, list[0].GuidValue);
		Assert.AreEqual(77, list[0].IntValue);
		Assert.AreEqual(99, list[1].IntValue);
	}

	[TestMethod]
	public async Task LoadRawAsync()
	{
		Info1? info1 = new();
		var list = new List<Info1>();

		Guid id = Guid.NewGuid();
		var prms = new ExpandoObject()
		{
			{ "Id", id }
		};

		await _dbContext.LoadRawAsync(null, "a2test.[RawData.Load]", prms,
			(rno, rdr) =>
			{
				if (rno == 0)
					info1 = ReadInfo(rdr);
				else if (rno == 1)
					list.Add(ReadInfo(rdr));
			});
		Assert.AreEqual(id, info1.GuidValue);
		Assert.AreEqual(23, info1.IntValue);
		Assert.AreEqual("string", info1.StringValue);
		Assert.AreEqual(2, list.Count);
		Assert.AreEqual(id, list[0].GuidValue);
		Assert.AreEqual(77, list[0].IntValue);
		Assert.AreEqual(99, list[1].IntValue);
	}
}
