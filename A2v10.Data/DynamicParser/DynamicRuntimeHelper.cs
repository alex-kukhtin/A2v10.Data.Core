// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;

namespace A2v10.Data.DynamicExpression
{
	public static class DynamicRuntimeHelper
	{
		public static Decimal Object2Number(Object elem)
		{
			if (elem is Boolean boolVal1)
				return boolVal1 ? 1M : 0M;
			else if (elem is String strVal)
			{
				if (Decimal.TryParse(strVal, NumberStyles.None, CultureInfo.InvariantCulture, out Decimal decVal))
					return decVal;
				throw new InvalidCastException($"Can't convert {elem} to Decimal");
			}
			else if (elem is Decimal decVal)
				return decVal;
			throw new InvalidCastException($"Can't convert {elem} to Decimal");
		}

		public static Object MemberOperation(Object source, String prop)
		{
			if (!(source is IDictionary<String, Object> src))
				return null;
			if (src.TryGetValue(prop, out Object result))
				return result;
			return null;
		}

		public static Object PlusOperation(Object elem1, Object elem2)
		{
			// TODO: JS number/string rules
			if ((elem1 is String) || (elem2 is String))
				return elem1?.ToString() + elem2?.ToString();
			return Decimal.Parse(elem1.ToString()) + Decimal.Parse(elem2.ToString());
		}

		public static Object MinusOperation(Object elem1, Object elem2)
		{
			try
			{
				var d1 = Object2Number(elem1);
				var d2 = Object2Number(elem2);
				return d1 - d2;
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (InvalidCastException /*ex*/)
			{
				return NaN.Value;
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}

		public static Object MultiplyOperation(Object elem1, Object elem2)
		{
			try
			{
				var d1 = Object2Number(elem1);
				var d2 = Object2Number(elem2);
				return d1 * d2;
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (InvalidCastException /*ex*/)
			{
				return NaN.Value;
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}

		public static Object DivideOperation(Object elem1, Object elem2)
		{
			try
			{
				var d1 = Object2Number(elem1);
				var d2 = Object2Number(elem2);
				if (d2 == 0M)
					return Infinity.Value;
				return d1 / d2;
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (InvalidCastException /*ex*/)
			{
				return NaN.Value;
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}

		public static Boolean EqualOperation(Object elem1, Object elem2)
		{
			if ((elem1 == null) && (elem2 == null))
				return true;
			else if ((elem1 == null) || (elem2 == null))
				return false;
			return elem1.ToString() == elem2.ToString();
		}

		public static Int32 Compare(Object elem1, Object elem2)
		{
			if (elem1 == elem2)
				return 0;
			else if ((elem1 is String str1) && (elem2 is String str2))
				return str1.CompareTo(str2);
			if ((elem1 is Decimal dec1) && (elem2 is Decimal dec2))
				return dec1.CompareTo(dec2);
			return 0;
		}

		public static Boolean GreaterThen(Object elem1, Object elem2)
		{
			Int32 result = Compare(elem1, elem2);
			return result > 0;
		}

		public static Boolean GreaterThenEqual(Object elem1, Object elem2)
		{
			Int32 result = Compare(elem1, elem2);
			return result >= 0;
		}

		public static Boolean LessThen(Object elem1, Object elem2)
		{
			Int32 result = Compare(elem1, elem2);
			return result < 0;
		}

		public static Boolean LessThenEqual(Object elem1, Object elem2)
		{
			Int32 result = Compare(elem1, elem2);
			return result <= 0;
		}

		public static Boolean NotEqualOperation(Object elem1, Object elem2)
		{
			return !EqualOperation(elem1, elem2);
		}

		public static Boolean ConvertToBoolean(Object elem)
		{
			if (elem == null)
				return false;
			if (elem is String strVal)
				return !String.IsNullOrEmpty(strVal);
			var elemType = elem.GetType();
			if (elemType.IsClass)
				return elem != null;
			else if (elemType == typeof(Boolean))
				return (Boolean)elem;
			else if (elemType == typeof(Char))
				return (Char)elem != '\0';
			else if (elemType.IsPrimitive)
				return elem.ToString() != "0";
			else
				return !String.IsNullOrEmpty(elem.ToString());
		}

		public static Object UnaryPlus(Object elem)
		{
			if (elem == null)
				return 0M;
			else if (elem is Decimal dec)
				return dec;
			else if (elem is String str)
			{
				if (Decimal.TryParse(str, out Decimal decVal))
					return decVal;
				return NaN.Value;
			}
			else if (elem is Boolean boolVal)
				return boolVal ? 1M : 0M;
			return NaN.Value;
		}

		public static Object UnaryMinus(Object elem)
		{
			if (elem == null)
				return 0M;
			else if (elem is Decimal dec)
				return -dec;
			else if (elem is String str)
			{
				if (Decimal.TryParse(str, out Decimal decVal))
					return -decVal;
				return NaN.Value;
			}
			else if (elem is Boolean boolVal)
				return boolVal ? -1M : -0M;
			return NaN.Value;
		}

		public static Object ElementAccess(Object elem, Object arg)
		{
			if (arg == null)
				throw new ArgumentNullException(nameof(arg));
			if (elem is List<Object> list)
			{
				Int32 index = Convert.ToInt32(arg);
				if (index < 0 || index >= list.Count)
					throw new IndexOutOfRangeException();
				return list[index];
			}
			else if (elem is ExpandoObject expObj)
			{
				return expObj.Get<Object>(arg.ToString());
			}
			return null;
		}
	}
}
