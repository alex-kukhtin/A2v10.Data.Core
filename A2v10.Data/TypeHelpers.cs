// Copyright © 2015-2023 Oleksandr  Kukhtin. All rights reserved.


namespace A2v10.Data;
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

