// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data.Providers
{
	[Serializable]
	public class ExternalDataException : Exception
	{
		public ExternalDataException(String message)
			: base(message)
		{

		}
	}
}
