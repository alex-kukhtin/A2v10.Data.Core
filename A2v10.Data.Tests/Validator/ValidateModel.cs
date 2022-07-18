// Copyright © 2015-2019 Alex Kukhtin. All rights reserved.

using A2v10.Data.Tests.Configuration;
using System.Threading.Tasks;

namespace A2v10.Data.Validator;

[TestClass]
[TestCategory("Validation")]
public class ValidateModel
{
	readonly IDbContext _dbContext;
	public ValidateModel()
	{
		_dbContext = Starter.Create();
	}

	[TestMethod]
	public async Task ValidateComplexModel()
	{
		IDataModel dm = await _dbContext.LoadModelAsync(null, "a2test.ComplexModel");
		String fileName = "testfiles/_data.defs.json";
		var validator = JsonValidator.FromFile(fileName);
		dm.Validate(validator.CreateValidator("ComplexModel"));
	}
}
