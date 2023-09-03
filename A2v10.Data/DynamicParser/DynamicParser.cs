
// Copyright © Microsoft Corporation.  All rights reserved.
// Copyright © 2015-2023 Oleksandr  Kukhtin. All rights reserved.

using System.Linq.Expressions;

namespace A2v10.Data.DynamicExpression;

public static class DynamicParser
{
	public static Expression Parse(String expression)
	{
		ExpressionParser parser = new(null, expression);
		return parser.Parse();
	}

	public static LambdaExpression ParseLambda(ParameterExpression[]? parameters, String expression)
	{
		ExpressionParser parser = new(parameters, expression);
		return Expression.Lambda(parser.Parse(), parameters);
	}
}


internal class ExpressionParser
{
	struct Token
	{
		public TokenId id;
		public String text;
		public Int32 pos;
	}

	enum TokenId
	{
		Unknown,
		End,
		Identifier,
		StringLiteral,
		NumberLiteral,
		Exclamation,
		Percent,
		Amphersand,
		OpenParen,
		CloseParen,
		Asterisk,
		Plus,
		Comma,
		Minus,
		Dot,
		Slash,
		Colon,
		LessThan,
		Equal,
		GreaterThan,
		Question,
		OpenBracket,
		CloseBracket,
		Bar,
		ExclamationEqual,
		ExclamationDoubleEqual,
		DoubleAmphersand,
		LessThanEqual,
		DoubleEqual,
		GreaterThanEqual,
		DoubleBar
	}

	static readonly Expression trueLiteral = Expression.Convert(Expression.Constant(true), typeof(Object));
	static readonly Expression falseLiteral = Expression.Convert(Expression.Constant(false), typeof(Object));
	static readonly Expression nullLiteral = Expression.Constant(null);


	static readonly Dictionary<String, Object> keywords = CreateKeywords();

	private readonly Dictionary<String, Object> symbols;
	private readonly Dictionary<Expression, String> literals;

	ParameterExpression? it;
	private readonly String text;
	Int32 textPos;
	readonly Int32 textLen;
	Char ch;
	Token token;

	public ExpressionParser(ParameterExpression[]? parameters, String expression)
	{
		text = expression ?? throw new ArgumentNullException(nameof(expression));
		symbols = new Dictionary<String, Object>();
		literals = new Dictionary<Expression, String>();
		if (parameters != null)
			ProcessParameters(parameters);
		textLen = text.Length;
		SetTextPos(0);
		NextToken();
	}

	void ProcessParameters(ParameterExpression[] parameters)
	{
		foreach (ParameterExpression pe in parameters)
			if (!String.IsNullOrEmpty(pe.Name))
				AddSymbol(pe.Name, pe);
		if (parameters.Length == 1 && String.IsNullOrEmpty(parameters[0].Name))
			it = parameters[0];
	}

	void AddSymbol(String name, Expression value)
	{
		if (symbols.ContainsKey(name))
			throw ParseError(Res.DuplicateIdentifier, name);
		symbols.Add(name, value);
	}

	public Expression Parse()
	{
		//Int32 exprPos = token.pos;
		Expression expr = ParseExpression();
		ValidateToken(TokenId.End, Res.SyntaxError);
		return expr;
	}

	// ?: operator
	Expression ParseExpression()
	{
		Expression expr = ParseLogicalOr();
		if (token.id == TokenId.Question)
		{
			NextToken();
			Expression expr1 = ParseExpression();
			ValidateToken(TokenId.Colon, Res.ColonExpected);
			NextToken();
			Expression expr2 = ParseExpression();
			expr = GenerateConditional(expr, expr1, expr2);
		}
		return expr;
	}

	static Expression PromoteLogical(Expression expr)
	{
		if (expr.Type != typeof(Boolean))
			return Expression.Call(typeof(DynamicRuntimeHelper), "ConvertToBoolean", null, expr);
		return expr;
	}

	static Expression UnaryPlus(Expression expr)
	{
		return Expression.Call(typeof(DynamicRuntimeHelper), "UnaryPlus", null, expr);
	}

	static Expression UnaryMinus(Expression expr)
	{
		return Expression.Call(typeof(DynamicRuntimeHelper), "UnaryMinus", null, expr);
	}

	// ||, or operator
	Expression ParseLogicalOr()
	{
		Expression left = ParseLogicalAnd();
		while (token.id == TokenId.DoubleBar)
		{
			//Token op = token;
			NextToken();
			Expression right = ParseLogicalAnd();
			left = Expression.OrElse(PromoteLogical(left), PromoteLogical(right));
		}
		return left;
	}

	// &&, and operator
	Expression ParseLogicalAnd()
	{
		Expression left = ParseComparison();
		while (token.id == TokenId.DoubleAmphersand)
		{
			//Token op = token;
			NextToken();
			Expression right = ParseComparison();
			left = Expression.AndAlso(PromoteLogical(left), PromoteLogical(right));
		}
		return left;
	}

	// =, ==, !=, <>, >, >=, <, <= operators
	Expression ParseComparison()
	{
		Expression left = ParseAdditive();
		while (token.id == TokenId.DoubleEqual ||
			token.id == TokenId.ExclamationEqual || token.id == TokenId.ExclamationDoubleEqual ||
			token.id == TokenId.GreaterThan || token.id == TokenId.GreaterThanEqual ||
			token.id == TokenId.LessThan || token.id == TokenId.LessThanEqual)
		{
			Token op = token;
			NextToken();
			Expression right = ParseAdditive();
			switch (op.id)
			{
				case TokenId.DoubleEqual:
					return Expression.Call(typeof(DynamicRuntimeHelper), "EqualOperation", null, left, right);
				case TokenId.ExclamationEqual:
				case TokenId.ExclamationDoubleEqual:
					return Expression.Call(typeof(DynamicRuntimeHelper), "NotEqualOperation", null, left, right);
				case TokenId.GreaterThan:
					return Expression.Call(typeof(DynamicRuntimeHelper), "GreaterThen", null, left, right);
				case TokenId.GreaterThanEqual:
					return Expression.Call(typeof(DynamicRuntimeHelper), "GreaterThenEqual", null, left, right);
				case TokenId.LessThan:
					return Expression.Call(typeof(DynamicRuntimeHelper), "LessThen", null, left, right);
				case TokenId.LessThanEqual:
					return Expression.Call(typeof(DynamicRuntimeHelper), "LessThenEqual", null, left, right);
			}
		}
		return left;
	}

	// +, -, & operators
	Expression ParseAdditive()
	{
		Expression left = ParseMultiplicative();
		while (token.id == TokenId.Plus || token.id == TokenId.Minus)
		{
			Token op = token;
			NextToken();
			Expression right = ParseMultiplicative();
			switch (op.id)
			{
				case TokenId.Plus:
					left = Expression.Call(typeof(DynamicRuntimeHelper), "PlusOperation", null, left, right);
					break;
				case TokenId.Minus:
					left = Expression.Call(typeof(DynamicRuntimeHelper), "MinusOperation", null, left, right);
					break;
			}
		}
		return left;
	}

	// *, /, %, operators
	Expression ParseMultiplicative()
	{
		Expression left = ParseUnary();
		while (token.id == TokenId.Asterisk || token.id == TokenId.Slash || token.id == TokenId.Percent)
		{
			Token op = token;
			NextToken();
			Expression right = ParseUnary();
			switch (op.id)
			{
				case TokenId.Asterisk:
					left = Expression.Call(typeof(DynamicRuntimeHelper), "MultiplyOperation", null, left, right);
					break;
				case TokenId.Slash:
					left = Expression.Call(typeof(DynamicRuntimeHelper), "DivideOperation", null, left, right);
					break;
				case TokenId.Percent:
					left = Expression.Modulo(left, right);
					break;
			}
		}
		return left;
	}

	// -, ! unary operators
	Expression ParseUnary()
	{
		if (token.id == TokenId.Minus || token.id == TokenId.Exclamation || token.id == TokenId.Plus)
		{
			Token op = token;
			NextToken();
			Expression expr = ParsePrimary();
			if (op.id == TokenId.Minus)
				expr = UnaryMinus(expr);
			else if (op.id == TokenId.Plus)
				expr = UnaryPlus(expr);
			else if (op.id == TokenId.Exclamation)
				expr = Expression.Not(PromoteLogical(expr));
			return expr;
		}

		return ParsePrimary();
	}

	Expression ParsePrimary()
	{
		Expression expr = ParsePrimaryStart();
		while (true)
		{
			if (token.id == TokenId.Dot)
			{
				NextToken();
				expr = ParseMemberAccess(expr);
			}
			else if (token.id == TokenId.OpenBracket)
			{
				expr = ParseElementAccess(expr);
			}
			else
			{
				break;
			}
		}
		return expr;
	}

	Expression ParsePrimaryStart()
	{
		return token.id switch
		{
			TokenId.Identifier => ParseIdentifier(),
			TokenId.StringLiteral => ParseStringLiteral(),
			TokenId.NumberLiteral => ParseNumberLiteral(),
			TokenId.OpenParen => ParseParenExpression(),
			_ => ParseUnary(),
		};
	}

	Expression ParseStringLiteral()
	{
		ValidateToken(TokenId.StringLiteral);
		Char quote = token.text[0];
		String s = token.text[1..^1];
		Int32 start = 0;
		while (true)
		{
			Int32 i = s.IndexOf(quote, start);
			if (i < 0)
				break;
			s = s.Remove(i, 1);
			start = i + 1;
		}
		NextToken();
		return CreateLiteral(s, s);
	}

	Expression ParseNumberLiteral()
	{
		ValidateToken(TokenId.NumberLiteral);
		String text = token.text;
		Object? value = null;
		if (Decimal.TryParse(text, out Decimal d))
			value = d;
		if (value == null)
			throw ParseError(Res.InvalidNumberLiteral, text);
		NextToken();
		return CreateLiteral(value, text);
	}

	Expression CreateLiteral(Object value, String text)
	{
		ConstantExpression expr = Expression.Constant(value, typeof(Object));
		literals.Add(expr, text);
		return expr;
	}

	Expression ParseParenExpression()
	{
		ValidateToken(TokenId.OpenParen, Res.OpenParenExpected);
		NextToken();
		Expression e = ParseExpression();
		ValidateToken(TokenId.CloseParen, Res.CloseParenOrOperatorExpected);
		NextToken();
		return e;
	}

	Expression ParseIdentifier()
	{
		ValidateToken(TokenId.Identifier);
		if (keywords.TryGetValue(token.text, out Object? value))
		{
			NextToken();
			return (Expression) value;
		}
		if (symbols.TryGetValue(token.text, out value))
		{
			if (value is not Expression expr)
			{
				expr = Expression.Constant(value);
			}
			else
			{
				if (expr is LambdaExpression)
					throw ParseError(textPos, Res.UnknownIdentifier, token.text);
			}
			NextToken();
			return expr;
		}
		if (it != null)
			return ParseMemberAccess(it);
		if (symbols.TryGetValue("Root", out Object? root))
		{
			var argId = Expression.Constant(token.text, typeof(String));
			if (root is Expression rootExpr) { 
				var expr = Expression.Call(typeof(DynamicRuntimeHelper), "MemberOperation", null, rootExpr, argId);
				NextToken();
				return expr;
			}
		}
		throw ParseError(Res.UnknownIdentifier, token.text);
	}

	static Expression GenerateConditional(Expression test, Expression expr1, Expression expr2)
	{
		test = PromoteLogical(test);
		return Expression.Condition(test, expr1, expr2);
	}


	Expression ParseMemberAccess(Expression instance)
	{
		//Int32 errorPos = token.pos;
		String id = GetIdentifier();
		NextToken();
		var argId = Expression.Constant(id, typeof(String));
		return Expression.Call(typeof(DynamicRuntimeHelper), "MemberOperation", null, instance, argId);
	}

	Expression[] ParseArguments()
	{
		List<Expression> argList = new();
		while (true)
		{
			argList.Add(ParseExpression());
			if (token.id != TokenId.Comma) break;
			NextToken();
		}
		return argList.ToArray();
	}

	Expression ParseElementAccess(Expression expr)
	{
		Int32 errorPos = token.pos;
		ValidateToken(TokenId.OpenBracket, Res.OpenParenExpected);
		NextToken();
		Expression[] args = ParseArguments();
		ValidateToken(TokenId.CloseBracket, Res.CloseBracketOrCommaExpected);
		NextToken();
		if (args.Length != 1)
			throw ParseError(errorPos, Res.CannotIndexMultiDimArray);
		Expression index = args[0];
		return Expression.Call(typeof(DynamicRuntimeHelper), "ElementAccess", null, expr, index);
	}

	void SetTextPos(Int32 pos)
	{
		textPos = pos;
		ch = textPos < textLen ? text[textPos] : '\0';
	}

	void NextChar()
	{
		if (textPos < textLen) textPos++;
		ch = textPos < textLen ? text[textPos] : '\0';
	}

	void NextToken()
	{
		while (Char.IsWhiteSpace(ch))
			NextChar();
		TokenId t;
		Int32 tokenPos = textPos;
		switch (ch)
		{
			case '!':
				NextChar();
				if (ch == '=')
				{
					NextChar();
					t = TokenId.ExclamationEqual;
					if (ch == '=')
					{
						// !==
						t = TokenId.ExclamationDoubleEqual;
						NextChar();
					}
				}
				else
				{
					t = TokenId.Exclamation;
				}
				break;
			case '%':
				NextChar();
				t = TokenId.Percent;
				break;
			case '&':
				NextChar();
				if (ch == '&')
				{
					NextChar();
					t = TokenId.DoubleAmphersand;
				}
				else
				{
					t = TokenId.Amphersand;
				}
				break;
			case '(':
				NextChar();
				t = TokenId.OpenParen;
				break;
			case ')':
				NextChar();
				t = TokenId.CloseParen;
				break;
			case '*':
				NextChar();
				t = TokenId.Asterisk;
				break;
			case '+':
				NextChar();
				t = TokenId.Plus;
				break;
			case ',':
				NextChar();
				t = TokenId.Comma;
				break;
			case '-':
				NextChar();
				t = TokenId.Minus;
				break;
			case '.':
				NextChar();
				t = TokenId.Dot;
				break;
			case '/':
				NextChar();
				t = TokenId.Slash;
				break;
			case ':':
				NextChar();
				t = TokenId.Colon;
				break;
			case '<':
				NextChar();
				if (ch == '=')
				{
					NextChar();
					t = TokenId.LessThanEqual;
				}
				else
				{
					t = TokenId.LessThan;
				}
				break;
			case '=':
				NextChar();
				if (ch == '=')
				{
					NextChar();
					t = TokenId.DoubleEqual;
					if (ch == '=')
						NextChar(); // ===
				}
				else
				{
					t = TokenId.Equal;
				}
				break;
			case '>':
				NextChar();
				if (ch == '=')
				{
					NextChar();
					t = TokenId.GreaterThanEqual;
				}
				else
				{
					t = TokenId.GreaterThan;
				}
				break;
			case '?':
				NextChar();
				t = TokenId.Question;
				break;
			case '[':
				NextChar();
				t = TokenId.OpenBracket;
				break;
			case ']':
				NextChar();
				t = TokenId.CloseBracket;
				break;
			case '|':
				NextChar();
				if (ch == '|')
				{
					NextChar();
					t = TokenId.DoubleBar;
				}
				else
				{
					t = TokenId.Bar;
				}
				break;
			case '"':
			case '\'':
			case '`':
				Char quote = ch;
				do
				{
					NextChar();
					while (textPos < textLen && ch != quote) NextChar();
					if (textPos == textLen)
						throw ParseError(textPos, Res.UnterminatedStringLiteral);
					NextChar();
				} while (ch == quote);
				t = TokenId.StringLiteral;
				break;
			default:
				if (Char.IsLetter(ch) || ch == '_' || ch == '$')
				{
					do
					{
						NextChar();
					} while (Char.IsLetterOrDigit(ch) || ch == '_' || ch == '$');
					t = TokenId.Identifier;
					break;
				}
				if (Char.IsDigit(ch))
				{
					t = TokenId.NumberLiteral;
					do
					{
						NextChar();
					} while (Char.IsDigit(ch));

					if (ch == '.')
					{
						NextChar();
						ValidateDigit();
						do
						{
							NextChar();
						} while (Char.IsDigit(ch));
					}
					break;
				}
				if (textPos == textLen)
				{
					t = TokenId.End;
					break;
				}
				throw ParseError(textPos, Res.InvalidCharacter, ch);
		}
		token.id = t;
		token.text = text[tokenPos..textPos];
		token.pos = tokenPos;
	}


	String GetIdentifier()
	{
		ValidateToken(TokenId.Identifier, Res.IdentifierExpected);
		String id = token.text;
		if (id.Length > 1 && id[0] == '@') id = id[1..];
		return id;
	}

	void ValidateDigit()
	{
		if (!Char.IsDigit(ch)) throw ParseError(textPos, Res.DigitExpected);
	}

	void ValidateToken(TokenId t, String errorMessage)
	{
		if (token.id != t)
			throw ParseError(errorMessage);
	}

	void ValidateToken(TokenId t)
	{
		if (token.id != t)
			throw ParseError(Res.SyntaxError);
	}

	Exception ParseError(String format, params Object[] args)
	{
		return ParseError(token.pos, format, args);
	}

	static Exception ParseError(Int32 pos, String format, params Object[] args)
	{
		return new ParseException(String.Format(System.Globalization.CultureInfo.CurrentCulture, format, args), pos);
	}

	static Dictionary<String, Object> CreateKeywords()
	{
		var d = new Dictionary<String, Object>
		{
			{ "true", trueLiteral },
			{ "false", falseLiteral },
			{ "null", nullLiteral }
		};
		return d;
	}
}

static class Res
{
	public const String DuplicateIdentifier = "The identifier '{0}' was defined more than once";
	public const String ExpressionExpected = "Expression expected";
	public const String InvalidNumberLiteral = "Invalid number literal '{0}'";
	public const String UnknownIdentifier = "Unknown identifier '{0}'";
	public const String CannotIndexMultiDimArray = "Indexing of multi-dimensional arrays is not supported";
	public const String UnterminatedStringLiteral = "Unterminated string literal";
	public const String InvalidCharacter = "Syntax error '{0}'";
	public const String DigitExpected = "Digit expected";
	public const String SyntaxError = "Syntax error";
	public const String ParseExceptionFormat = "{0} (at index {1})";
	public const String ColonExpected = "':' expected";
	public const String OpenParenExpected = "'(' expected";
	public const String CloseParenOrOperatorExpected = "')' or operator expected";
	public const String CloseParenOrCommaExpected = "')' or ',' expected";
	public const String CloseBracketOrCommaExpected = "']' or ',' expected";
	public const String IdentifierExpected = "Identifier expected";
}

