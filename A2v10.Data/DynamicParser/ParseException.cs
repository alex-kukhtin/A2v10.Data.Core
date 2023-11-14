// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Data.DynamicExpression;
public sealed class ParseException(String message, Int32 position) : Exception(message)
{
	readonly Int32 position = position;

    public Int32 Position => position;

	public override String ToString()
	{
		return String.Format(Res.ParseExceptionFormat, Message, position);
	}
}

