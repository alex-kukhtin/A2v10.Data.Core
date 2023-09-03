// Copyright © 2012-2023 Oleksandr  Kukhtin. All rights reserved.


namespace A2v10.Data;

public sealed class DataLoaderException : Exception
{
	public DataLoaderException(String message)
		: base(message)
	{
	}
}

public sealed class DataWriterException : Exception
{
	public DataWriterException(String message)
		: base(message)
	{
	}
}

public sealed class DataDynamicException : Exception
{
	public DataDynamicException(String message)
		: base(message)
	{
	}
}

public sealed class DataValidationException : Exception
{
	public DataValidationException(String message)
		: base(message)
	{
	}
}

