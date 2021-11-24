// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.


namespace A2v10.Data.Interfaces;
public interface IDataModelValidator
{
	void ValidateType(String name, IDataMetadata typeMetadata);
}

