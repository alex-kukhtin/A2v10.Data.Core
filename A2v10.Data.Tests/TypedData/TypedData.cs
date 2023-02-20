// Copyright © 2015-2022 Alex Kukhtin. All rights reserved.

using System.IO;
using System.Security.Cryptography;
using System.Threading;
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
}
