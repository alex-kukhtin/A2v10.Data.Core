// Copyright © 2015-2025 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;
using System.Linq.Expressions;

using A2v10.Data.Dynamic;

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
		Assert.IsFalse(result as Boolean?);
		result = CalcSimpleExpression("!!true");
		Assert.IsTrue(result as Boolean?);
		result = CalcSimpleExpression("!!'a'");
		Assert.IsTrue(result as Boolean?);

		result = CalcSimpleExpression("!''");
		Assert.IsTrue(result as Boolean?);
	}

	[TestMethod]
	public void MultiplicativeOperations()
	{
		var result = CalcSimpleExpression("2 * 2");
		Assert.AreEqual(4M, result); // as decimal

		result = CalcSimpleExpression("2 * '2'");
		Assert.AreEqual(4M, result); // as decimal

		result = CalcSimpleExpression("'3'* 2");
		Assert.AreEqual(6M, result); // as decimal

		result = CalcSimpleExpression("2 * true");
		Assert.AreEqual(2M, result); // as decimal

		result = CalcSimpleExpression("4 / 2");
		Assert.AreEqual(2M, result); // as decimal

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
		Assert.IsTrue(result as Boolean?); // as decimal

		result = CalcSimpleExpression("'aaa' && 'bbb'");
		Assert.IsTrue(result as Boolean?); // as decimal

		result = CalcSimpleExpression("false || 2 !== 2");
		Assert.IsFalse(result as Boolean?); // as decimal

		result = CalcSimpleExpression("false || 2 == 2");
		Assert.IsTrue(result as Boolean?); // as decimal

		result = CalcSimpleExpression("(3 == 3 && 2 == 2) || false");
		Assert.IsTrue(result as Boolean?); // as decimal

		result = CalcSimpleExpression("'' || 'a'");
		Assert.IsTrue(result as Boolean?); // as decimal
	}

	[TestMethod]
	public void AdditiveOperations()
	{
		var result = CalcSimpleExpression("2 + 2");
		Assert.AreEqual(4M, result); // as decimal

		result = CalcSimpleExpression("'2' + 4");
		Assert.AreEqual("24", result);

		result = CalcSimpleExpression("2 + '4'");
		Assert.AreEqual("24", result);

		result = CalcSimpleExpression("'aaa' + 'bbb'");
		Assert.AreEqual("aaabbb", result);

		result = CalcSimpleExpression("5 - '3'");
		Assert.AreEqual(2M, result);

		result = CalcSimpleExpression("'5' - - 8");
		Assert.AreEqual(13M, result);

		result = CalcSimpleExpression("'s' - 23");
		Assert.IsTrue(NaN.IsNaN(result));
	}

	[TestMethod]
	public void TernaryOperation()
	{
		var result = CalcSimpleExpression("2 === 2 ? 'yes' : 'no'");
		Assert.AreEqual("yes", result);

		result = CalcSimpleExpression("2 == 2 ? true : false");
		Assert.IsTrue(result as Boolean?);

		result = CalcSimpleExpression("2 !== 2 ? 'yes' : 'no'");
		Assert.AreEqual("no", result);

		result = CalcSimpleExpression("2 !== 2 ? 'yes' : null");
		Assert.IsNull(result);

		result = CalcSimpleExpression("2 != 2 ? 3 != 3 ? '1' : '2' : '3'");
		Assert.AreEqual("3", result);

		result = CalcSimpleExpression("2 == 2 ? 3 != 3 ? '1' : '2' : '3'");
		Assert.AreEqual("2", result);

		result = CalcSimpleExpression("2 == 2 ? 3 == 3 ? '1' : '2' : '3'");
		Assert.AreEqual("1", result);

		result = CalcSimpleExpression("'t' ? 'yes' : 'no'");
		Assert.AreEqual("yes", result);
	}


	[TestMethod]
	public void ComparisonOperation()
	{
		var result = CalcSimpleExpression("3 > 2");
		Assert.IsTrue(result as Boolean?);

		result = CalcSimpleExpression("2 >= 2");
		Assert.IsTrue(result as Boolean?);

		result = CalcSimpleExpression("2 > 3");
		Assert.IsFalse(result as Boolean?);

		result = CalcSimpleExpression("2 < 3");
		Assert.IsTrue(result as Boolean?);

		result = CalcSimpleExpression("2 <= 3");
		Assert.IsTrue(result as Boolean?);

		result = CalcSimpleExpression("'aaa' < 'bbb'");
		Assert.IsTrue(result as Boolean?);
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
		Assert.AreEqual("agent name", result);

		result = CalcExpression("Agent.$dollar", "Agent", agent);
		Assert.AreEqual("$", result);

		result = CalcExpression("Agent._underscore", "Agent", agent);
		Assert.AreEqual("_", result);

		result = CalcExpression("Agent.Address.Text", "Agent", agent);
		Assert.AreEqual("text", result);

		result = CalcExpression("Agent.Array[0].Value", "Agent", agent);
		Assert.AreEqual(3, result);

		result = CalcExpression("Agent['Array'][0].Value", "Agent", agent);
		Assert.AreEqual(3, result);

		result = CalcExpression("Agent['Array'][2 - 2].Value", "Agent", agent);
		Assert.AreEqual(3, result);

		var root = new ExpandoObject
		{
			{ "Agent", agent }
		};

		result = CalcExpression("Root.Agent.Name", "Root", root);
		Assert.AreEqual("agent name", result);

		result = CalcExpression("Agent.Name", "Root", root);
		Assert.AreEqual("agent name", result);
	}
}
