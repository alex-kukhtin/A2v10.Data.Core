// Copyright © 2012-2019 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data
{
	public class DataLoaderException : Exception
	{
		public DataLoaderException(String message)
			: base(message)
		{
		}

		public DataLoaderException()
		{
		}

		public DataLoaderException(String message, Exception innerException) : base(message, innerException)
		{
		}
	}

	public class DataWriterException : Exception
	{
		public DataWriterException(String message)
			: base(message)
		{
		}

		public DataWriterException()
		{
		}

		public DataWriterException(String message, Exception innerException) 
			: base(message, innerException)
		{
		}

		protected DataWriterException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
		{
			throw new NotImplementedException();
		}
	}

	public class DataDynamicException : Exception
	{
		public DataDynamicException(String message)
			: base(message)
		{
		}

		public DataDynamicException()
		{
		}

		public DataDynamicException(String message, Exception innerException) : base(message, innerException)
		{
		}

		protected DataDynamicException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
		{
			throw new NotImplementedException();
		}
	}

	public class DataValidationException: Exception
	{
		public DataValidationException(String message)
			: base(message)
		{
		}

		public DataValidationException()
		{
		}

		public DataValidationException(String message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
