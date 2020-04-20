// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Data.DynamicExpression
{
	public static class TypeHelpers
	{
		public static Boolean IsNullableType(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		public static Type GetNonNullableType(this Type type)
		{
			return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
		}
	}
}
