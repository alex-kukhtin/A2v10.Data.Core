// Copyright © 2012-2024 Oleksandr Kukhtin. All rights reserved.

using System.Text.RegularExpressions;

namespace A2v10.Data;

public partial struct FieldInfo
{
	public String PropertyName { get; private set; }
	public String TypeName { get; }
	public FieldType FieldType { get; }
	public SpecType SpecType { get; }
	public Boolean IsComplexField { get; }
	public Boolean IsLazy { get; }
	public Boolean IsMain { get; set; }
	public List<String>? MapFields { get; }

	public FieldInfo(String name)
	{
		PropertyName = String.Empty;
		TypeName = String.Empty;
		FieldType = FieldType.Scalar;
		SpecType = SpecType.Unknown;
		IsLazy = false;
		IsMain = false;
		MapFields = null;
		var x = name.Split('!');
		if (x.Length > 0)
			PropertyName = x[0];
		CheckField(x);
		if (x.Length > 1)
		{
			TypeName = x[1];
			FieldType = FieldType.Object;
		}
		if (x.Length > 2)
		{
			FieldType = InternalHelpers.TypeName2FieldType(x[2]);
			if (FieldType == FieldType.Scalar || FieldType == FieldType.Array || FieldType == FieldType.Json)
				SpecType = InternalHelpers.TypeName2SpecType(x[2]);
			IsLazy = x[2].Contains("Lazy");
			IsMain = x[2].Contains("Main");
		}
		if (x.Length == 4)
		{
			FieldType = FieldType.MapObject;
			MapFields = [.. x[3].Split(':')];
		}
		IsComplexField = PropertyName.Contains('.');
		CheckReservedWords();
	}

	public FieldInfo(String name, String targetType)
	{
		PropertyName = name;
		TypeName = targetType;
		FieldType = FieldType.Object;
		SpecType = SpecType.Unknown;
		IsLazy = false;
		IsMain = false;
		MapFields = null;
		IsComplexField = false;
	}

	static readonly HashSet<String> _reservedWords =
    [
        "Parent",
		"Root",
		"Context",
		"ParentId",
		"CurrentyKey",
		"ParentRowNumber",
		"ParentKey",
		"ParentGUID"
	];

    readonly void CheckReservedWords()
	{
		if (_reservedWords.Contains(PropertyName))
		{
			throw new DataLoaderException($"PropertyName '{PropertyName}' is a reserved word");
		}
	}

	const String PATTERN = @"^[\p{L}_\$][\p{L}0-9_\$]*$";
#if NET7_0_OR_GREATER
	[GeneratedRegex(PATTERN, RegexOptions.Singleline | RegexOptions.IgnoreCase, "en-US")]
	private static partial Regex IderRegex();
#else
	private static Regex IDERREGEX => new(PATTERN, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase );
	private static Regex IderRegex() => IDERREGEX;
#endif

	public readonly void CheckTypeName()
	{
		if (String.IsNullOrEmpty(TypeName))
			return;
		if (!IderRegex().IsMatch(TypeName))
			throw new DataLoaderException($"TypeName '{TypeName}' must be an identifier");
	}


	public readonly void CheckValid()
	{
		if (!String.IsNullOrEmpty(PropertyName))
		{
			if (FieldType == FieldType.Json)
				return;
			if (String.IsNullOrEmpty(TypeName))
				throw new DataLoaderException($"If a property name ('{PropertyName}') is specified, then type name is required");
		}
	}

	public FieldInfo(FieldInfo source, String name)
	{
		// for complex fields only
		PropertyName = name;
		TypeName = String.Empty;
		FieldType = FieldType.Scalar;
		SpecType = source.SpecType;
		IsComplexField = false;
		IsLazy = false;
		IsMain = false;
		MapFields = null;
	}


	public readonly Boolean IsVisible { get { return !String.IsNullOrEmpty(PropertyName); } }

	public readonly Boolean IsArray => FieldType == FieldType.Array;
	public readonly Boolean IsObject => FieldType == FieldType.Object;
	public readonly Boolean IsMap => FieldType == FieldType.Map;
	public readonly Boolean IsMapObject => FieldType == FieldType.MapObject;
	public readonly Boolean IsSheet => FieldType == FieldType.Sheet;
	public readonly Boolean IsTree => FieldType == FieldType.Tree;
	public readonly Boolean IsRows => FieldType == FieldType.Rows;
	public readonly Boolean IsColumns => FieldType == FieldType.Columns;
	public readonly Boolean IsCells => FieldType == FieldType.Cells;
	public readonly Boolean IsGroup => FieldType == FieldType.Group;
	public readonly Boolean IsCrossArray => FieldType == FieldType.CrossArray;
	public readonly Boolean IsCrossObject => FieldType == FieldType.CrossObject;
	public readonly Boolean IsCross => IsCrossArray || IsCrossObject;
	public readonly Boolean IsLookup => FieldType == FieldType.Lookup;
	public readonly Boolean IsObjectLike => IsArray || IsObject || IsTree || IsGroup || IsMap || IsMapObject || IsCrossArray || IsCrossObject || IsLookup || IsSheet;
	public readonly Boolean IsNestedType => IsRefId || IsArray || IsCrossArray || IsCrossObject || IsTree;
	public readonly Boolean IsRefId => SpecType == SpecType.RefId;
	public readonly Boolean IsParentId => SpecType == SpecType.ParentId;
	public readonly Boolean IsColumnId => SpecType == SpecType.ColumnId;
	public readonly Boolean IsId => SpecType == SpecType.Id;
	public readonly Boolean IsKey => SpecType == SpecType.Key;
	public readonly Boolean IsProp => SpecType == SpecType.Prop;
	public readonly Boolean IsIndex => SpecType == SpecType.Index;
	public readonly Boolean IsToken => SpecType == SpecType.Token;
	public readonly Boolean IsRowCount => SpecType == SpecType.RowCount;
	public readonly Boolean IsItems => SpecType == SpecType.Items;
	public readonly Boolean IsGroupMarker => SpecType == SpecType.GroupMarker;
	public readonly Boolean IsJson => SpecType == SpecType.Json;
	public readonly Boolean IsPermissions => SpecType == SpecType.Permissions;
	public readonly Boolean IsUtc => SpecType == SpecType.Utc;

	public readonly Boolean IsParentIdSelf(FieldInfo root)
	{
		return IsParentId && TypeName.StartsWith(root.TypeName);
	}

	private static void CheckField(String[] parts)
	{
		if (parts.Length == 2)
		{
			var p1 = parts[1];
			if (SpecType.TryParse(p1, out SpecType st))
			{
				// A special type is specified, but there are only two parts in the field name
				throw new DataLoaderException($"Invalid field name '{String.Join("!", parts)}'. Extra modifier '!{st}' is specified");
			}
		}
	}

	public void CheckPermissionsName()
	{
		if (String.IsNullOrEmpty(PropertyName))
			PropertyName = "__permissions";
	}

	public override Boolean Equals(Object? obj)
	{
		throw new NotImplementedException();
	}

	public override Int32 GetHashCode()
	{
		throw new NotImplementedException();
	}

	public static Boolean operator ==(FieldInfo left, FieldInfo right)
	{
		return left.Equals(right);
	}

	public static Boolean operator !=(FieldInfo left, FieldInfo right)
	{
		return !(left == right);
	}
}
