// Copyright © 2012-2019 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data.Interfaces
{
	public interface IDataFieldMetadata
	{
		Boolean IsLazy { get; }
		Boolean IsJson { get; }

		String RefObject { get; }
		Int32 Length { get; }

		String GetObjectType(String fieldName);

		String TypeForValidate { get; }
	}
}
