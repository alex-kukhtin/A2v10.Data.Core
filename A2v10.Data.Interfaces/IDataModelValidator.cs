// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Data.Interfaces;
public interface IDataModelValidator
{
	void ValidateType(String name, IDataMetadata typeMetadata);
}

