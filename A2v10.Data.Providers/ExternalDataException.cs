// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data.Providers
{
	public class ExternalDataException : Exception
	{
		public ExternalDataException()
			: base()
		{

		}

		public ExternalDataException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public ExternalDataException(String message)
			: base(message)
		{

		}

	}
}
