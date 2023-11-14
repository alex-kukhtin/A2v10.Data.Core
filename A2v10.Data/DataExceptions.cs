// Copyright © 2012-2023 Oleksandr  Kukhtin. All rights reserved.


namespace A2v10.Data;

public sealed class DataLoaderException(String message) : Exception(message)
{
}

public sealed class DataWriterException(String message) : Exception(message)
{
}

public sealed class DataDynamicException(String message) : Exception(message)
{
}

public sealed class DataValidationException(String message) : Exception(message)
{
}

