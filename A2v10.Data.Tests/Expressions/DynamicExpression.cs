﻿// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.

using A2v10.Data.Dynamic;
using System.Dynamic;
using System.Linq.Expressions;

/*TODO:
 * test invalid cases
 */

namespace A2v10.Data.Tests;

[TestClass]
[TestCategory("Expressions")]
public class Expressions
{
	static Object? CalcSimpleExpression(String expression)
	{
		var lexpr = DynamicParser.ParseLambda(null, expression);
		var lambda = lexpr.Compile();
		return lambda.DynamicInvoke();
	}

	static Object? CalcExpression(String expression, String prm, Object value)
	{
		var prms = new ParameterExpression[] {
			Expression.Parameter(typeof(Object), prm)
		};
		var lexpr = DynamicParser.ParseLambda(prms, expression);
		var lambda = lexpr.Compile();
		return lambda.DynamicInvoke(value);
	}

	[TestMethod]
	public void UnaryOperator()
	{
		var result = CalcSimpleExpression("+ + 2");
		Assert.AreEqual(2M, result); // as decimal
		result = CalcSimpleExpression("+ 2");
		Assert.AreEqual(2M, result); // as decimal
		result = CalcSimpleExpression("- 2");
		Assert.AreEqual(-2M, result); // as decimal
		result = CalcSimpleExpression("-'2'");
		Assert.AreEqual(-2M, result); // as decimal
		result = CalcSimpleExpression("+'2'");
		Assert.AreEqual(2M, result); // as decimal
		result = CalcSimpleExpression("-'a'");
		Assert.IsTrue(NaN.IsNaN(result));
		result = CalcSimpleExpression("-true");
		Assert.AreEqual(-1M, result); // as decimal
		result = CalcSimpleExpression("+false");
		Assert.AreEqual(0M, result); // as decimal
		result = CalcSimpleExpression("!'aaa'");
		Assert.AreEqual(false, result);
		result = CalcSimpleExpression("!!true");
		Assert.AreEqual(true, result);
		result = CalcSimpleExpression("!!'a'");
		Assert.AreEqual(true, result);

		result = CalcSimpleExpression("!''");
		Assert.AreEqual(true, result);
	}

	[TestMethod]
	public void MultiplicativeOperations()
	{
		var result = CalcSimpleExpression("2 * 2");
		Assert.AreEqual(result, 4M); // as decimal

		result = CalcSimpleExpression("2 * '2'");
		Assert.AreEqual(result, 4M); // as decimal

		result = CalcSimpleExpression("'3'* 2");
		Assert.AreEqual(result, 6M); // as decimal

		result = CalcSimpleExpression("2 * true");
		Assert.AreEqual(result, 2M); // as decimal

		result = CalcSimpleExpression("4 / 2");
		Assert.AreEqual(result, 2M); // as decimal

		result = CalcSimpleExpression("4 / 0");
		Assert.IsTrue(Infinity.IsInfinity(result));

		result = CalcSimpleExpression("4 / false");
		Assert.IsTrue(Infinity.IsInfinity(result));

		result = CalcSimpleExpression("2 / true");
		Assert.AreEqual(2M, result); // as decimal
	}

	[TestMethod]
	public void LogicalOperations()
	{
		var result = CalcSimpleExpression("2 == 2 && 3 == 3");
		Assert.AreEqual(true, result); // as decimal

		result = CalcSimpleExpression("'aaa' && 'bbb'");
		Assert.AreEqual(true, result); // as decimal

		result = CalcSimpleExpression("false || 2 !== 2");
		Assert.AreEqual(false, result); // as decimal

		result = CalcSimpleExpression("false || 2 == 2");
		Assert.AreEqual(true, result); // as decimal

		result = CalcSimpleExpression("(3 == 3 && 2 == 2) || false");
		Assert.AreEqual(true, result); // as decimal

		result = CalcSimpleExpression("'' || 'a'");
		Assert.AreEqual(true, result); // as decimal
	}

	[TestMethod]
	public void AdditiveOperations()
	{
		var result = CalcSimpleExpression("2 + 2");
		Assert.AreEqual(result, 4M); // as decimal

		result = CalcSimpleExpression("'2' + 4");
		Assert.AreEqual(result, "24");

		result = CalcSimpleExpression("2 + '4'");
		Assert.AreEqual(result, "24");

		result = CalcSimpleExpression("'aaa' + 'bbb'");
		Assert.AreEqual(result, "aaabbb");

		result = CalcSimpleExpression("5 - '3'");
		Assert.AreEqual(result, 2M);

		result = CalcSimpleExpression("'5' - - 8");
		Assert.AreEqual(result, 13M);

		result = CalcSimpleExpression("'s' - 23");
		Assert.IsTrue(NaN.IsNaN(result));
	}

	[TestMethod]
	public void TernaryOperation()
	{
		var result = CalcSimpleExpression("2 === 2 ? 'yes' : 'no'");
		Assert.AreEqual(result, "yes");

		result = CalcSimpleExpression("2 == 2 ? true : false");
		Assert.AreEqual(result, true);

		result = CalcSimpleExpression("2 !== 2 ? 'yes' : 'no'");
		Assert.AreEqual(result, "no");

		result = CalcSimpleExpression("2 !== 2 ? 'yes' : null");
		Assert.AreEqual(result, null);

		result = CalcSimpleExpression("2 != 2 ? 3 != 3 ? '1' : '2' : '3'");
		Assert.AreEqual(result, "3");

		result = CalcSimpleExpression("2 == 2 ? 3 != 3 ? '1' : '2' : '3'");
		Assert.AreEqual(result, "2");

		result = CalcSimpleExpression("2 == 2 ? 3 == 3 ? '1' : '2' : '3'");
		Assert.AreEqual(result, "1");

		result = CalcSimpleExpression("'t' ? 'yes' : 'no'");
		Assert.AreEqual(result, "yes");
	}


	[TestMethod]
	public void ComparisonOperation()
	{
		var result = CalcSimpleExpression("3 > 2");
		Assert.AreEqual(result, true);

		result = CalcSimpleExpression("2 >= 2");
		Assert.AreEqual(result, true);

		result = CalcSimpleExpression("2 > 3");
		Assert.AreEqual(result, false);

		result = CalcSimpleExpression("2 < 3");
		Assert.AreEqual(result, true);

		result = CalcSimpleExpression("2 <= 3");
		Assert.AreEqual(result, true);

		result = CalcSimpleExpression("'aaa' < 'bbb'");
		Assert.AreEqual(result, true);
	}

	[TestMethod]
	public void MemberAccess()
	{

		var agent = new ExpandoObject
		{
			{ "Name", "agent name" },
			{ "$dollar", "$"},
			{ "_underscore", "_" }
		};

		var addr = new ExpandoObject();
		addr.Set("Text", "text");
		agent.Set("Address", addr);
		var arr = new List<Object>();
		var elem = new ExpandoObject
		{
			{ "Value", 3 }
		};
		arr.Add(elem);
		agent.Set("Array", arr);

		var result = CalcExpression("Agent.Name", "Agent", agent);
		Assert.AreEqual(result, "agent name");

		result = CalcExpression("Agent.$dollar", "Agent", agent);
		Assert.AreEqual(result, "$");

		result = CalcExpression("Agent._underscore", "Agent", agent);
		Assert.AreEqual(result, "_");

		result = CalcExpression("Agent.Address.Text", "Agent", agent);
		Assert.AreEqual(result, "text");

		result = CalcExpression("Agent.Array[0].Value", "Agent", agent);
		Assert.AreEqual(result, 3);

		result = CalcExpression("Agent['Array'][0].Value", "Agent", agent);
		Assert.AreEqual(result, 3);

		result = CalcExpression("Agent['Array'][2 - 2].Value", "Agent", agent);
		Assert.AreEqual(result, 3);

		var root = new ExpandoObject
		{
			{ "Agent", agent }
		};

		result = CalcExpression("Root.Agent.Name", "Root", root);
		Assert.AreEqual(result, "agent name");

		result = CalcExpression("Agent.Name", "Root", root);
		Assert.AreEqual(result, "agent name");
	}
}
