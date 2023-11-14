// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using A2v10.Data.Tests.Configuration;

namespace A2v10.Data.Tests;

public record BlobUpdateInfo
{
    public Int32? TenantId { get; set; }
    public Int64? CompanyId { get; set; }
    public Int64 UserId { get; set; }
    public String? Mime { get; set; }
    public String? Name { get; set; }
    public Stream? Stream { get; set; }
    public String? BlobName { get; set; }
    public String? Key { get; set; }
    public Object? Id { get; set; }
}
public record BlobUpdateOutput
{
    public Object? Id { get; set; }
    public Guid? Token { get; set; }
    public String? Mime { get; set; }
    public String? Name { get; set; }
    public Byte[]? Stream { get; set; }
}

[TestClass]
[TestCategory("Typed Data")]
public class TypedDataTests
{
	readonly IDbContext _dbContext;
	public TypedDataTests()
	{
		_dbContext = Starter.CreateWithTenants();
	}

	[TestMethod]
	public async Task UpdateStream()
	{
        var bi = new BlobUpdateInfo
        {
            Name = "test_name",
            Mime = "test_mime",
            UserId = 99
        };
        var bytes = RandomNumberGenerator.GetBytes(100);
		bi.Stream = new MemoryStream(bytes) as Stream;

		var output = await _dbContext.ExecuteAndLoadAsync<BlobUpdateInfo, BlobUpdateOutput>("", "a2test.[Blob.Update]", bi);
		Assert.IsNotNull(output);
		Assert.AreEqual((Int64) 123, output.Id);
		Assert.AreEqual("test_name", output.Name);
        Assert.AreEqual("test_mime", output.Mime);
		Assert.IsNotNull(output.Token);
		for (int i=0; i< bytes.Length; i++)
		{
			Assert.AreEqual(bytes[i], output.Stream![i]);
		}
    }


    public record ScheduledCommand
    {
        public ScheduledCommand()
        {
            Command = String.Empty;
        }
        public ScheduledCommand(String command, String? data = null, DateTime? utcRunAt = null)
        {
            Command = command;
            Data = data;
            UtcRunAt = utcRunAt;
        }
        public String Command { get; init; }
        public String? Data { get; init; }
        public DateTime? UtcRunAt { get; init; }
    }

    [TestMethod]
    public async Task SaveListAsync()
    {
        var dt = DateTime.Today;
        var sc = new List<ScheduledCommand>()
        {
            new("Test1", "Data"),
            new("Test2"),
            new("Test3", "Sample data", dt)
        };
        await _dbContext.SaveListAsync<ScheduledCommand>("", "a2test.[List.Save]", null, sc);
        var list = await _dbContext.LoadListAsync<ScheduledCommand>(null, "a2test.[List.Load]", null);
        Assert.IsNotNull(list);
        Assert.AreEqual(3, list.Count);

        var c0 = list[0];
        var c1 = list[1];
        var c2 = list[2];

        Assert.AreEqual("Test1", c0.Command);
        Assert.AreEqual("Data", c0.Data);
        Assert.IsNull(c0.UtcRunAt);

        Assert.AreEqual("Test2", c1.Command);
        Assert.IsNull(c1.Data);
        Assert.IsNull(c1.UtcRunAt);

        Assert.AreEqual("Test3", c2.Command);
        Assert.AreEqual("Sample data", c2.Data);
        Assert.AreEqual(dt, c2.UtcRunAt);
    }
}
