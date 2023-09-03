// Copyright © 2015-2023 Oleksandr  Kukhtin. All rights reserved.


namespace A2v10.Data.DynamicExpression;
public struct NaN
{
	public static NaN Value => new();
	public static Boolean IsNaN(Object? test)
	{
		return test is NaN;
	}

	public override Boolean Equals(Object? obj)
	{
		return obj is NaN;
	}

	public override Int32 GetHashCode() => -1;

	public static Boolean operator ==(NaN left, NaN right) => true;
	public static Boolean operator !=(NaN left, NaN right) => false;
}

public struct Undefined
{
	public static Undefined Value => new();
	public static Boolean IsUndefined(Object test)
	{
		return test is Undefined;
	}

	public override Boolean Equals(Object? obj)
	{
		return obj is Undefined;
	}

	public override Int32 GetHashCode() => -1;

	public static Boolean operator ==(Undefined left, Undefined right) => true;
	public static Boolean operator !=(Undefined left, Undefined right) => false;
}

public struct Infinity
{
	public static Infinity Value => new();
	public static Boolean IsInfinity(Object? test)
	{
		return test is Infinity;
	}

	public override Boolean Equals(Object? obj)
	{
		return obj is Infinity;
	}

	public override Int32 GetHashCode() => -1;

	public static Boolean operator ==(Infinity left, Infinity right) => true;
	public static Boolean operator !=(Infinity left, Infinity right) => false;
}

