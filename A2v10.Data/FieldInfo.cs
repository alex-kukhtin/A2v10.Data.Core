// Copyright © 2012-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace A2v10.Data
{
	public struct FieldInfo
	{
		public String PropertyName { get; private set; }
		public String TypeName { get; }
		public FieldType FieldType { get; }
		public SpecType SpecType { get; }
		public Boolean IsComplexField { get; }
		public Boolean IsLazy { get; }
		public Boolean IsMain { get; set; }
		public List<String> MapFields { get; }

		public FieldInfo(String name)
		{
			PropertyName = null;
			TypeName = null;
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
				FieldType = DataHelpers.TypeName2FieldType(x[2]);
				if (FieldType == FieldType.Scalar || FieldType == FieldType.Array || FieldType == FieldType.Json)
					SpecType = DataHelpers.TypeName2SpecType(x[2]);
				IsLazy = x[2].Contains("Lazy");
				IsMain = x[2].Contains("Main");
			}
			if (x.Length == 4)
			{
				FieldType = FieldType.MapObject;
				MapFields = x[3].Split(':').ToList();
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

		static readonly HashSet<String> _reservedWords = new HashSet<String>()
			{
				"Parent",
				"Root",
				"Context",
				"ParentId",
				"CurrentyKey",
				"ParentRowNumber",
				"ParentKey",
				"ParentGUID"
			};

		void CheckReservedWords()
		{
			if (_reservedWords.Contains(PropertyName))
			{
				throw new DataLoaderException($"PropertyName '{PropertyName}' is a reserved word");
			}
		}

		static readonly Regex _ider = new Regex(@"^[a-z_\$][a-z0-9_\$]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

		public void CheckTypeName()
		{
			if (String.IsNullOrEmpty(TypeName))
				return;
			if (!_ider.IsMatch(TypeName))
				throw new DataLoaderException($"TypeName '{TypeName}' must be an identifier");
		}


		public void CheckValid()
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
			TypeName = null;
			FieldType = FieldType.Scalar;
			SpecType = source.SpecType;
			IsComplexField = false;
			IsLazy = false;
			IsMain = false;
			MapFields = null;
		}


		public Boolean IsVisible { get { return !String.IsNullOrEmpty(PropertyName); } }

		public Boolean IsArray => FieldType == FieldType.Array;
		public Boolean IsObject => FieldType == FieldType.Object;
		public Boolean IsMap => FieldType == FieldType.Map;
		public Boolean IsMapObject => FieldType == FieldType.MapObject;
		public Boolean IsTree => FieldType == FieldType.Tree;
		public Boolean IsGroup => FieldType == FieldType.Group;
		public Boolean IsCrossArray => FieldType == FieldType.CrossArray;
		public Boolean IsCrossObject => FieldType == FieldType.CrossObject;
		public Boolean IsCross => IsCrossArray || IsCrossObject;

		public Boolean IsObjectLike => IsArray || IsObject || IsTree || IsGroup || IsMap || IsMapObject || IsCrossArray || IsCrossObject;
		public Boolean IsNestedType => IsRefId || IsArray || IsCrossArray || IsCrossObject;
		public Boolean IsRefId => SpecType == SpecType.RefId;
		public Boolean IsParentId => SpecType == SpecType.ParentId;
		public Boolean IsId => SpecType == SpecType.Id;
		public Boolean IsKey => SpecType == SpecType.Key;
		public Boolean IsRowCount => SpecType == SpecType.RowCount;
		public Boolean IsItems => SpecType == SpecType.Items;
		public Boolean IsGroupMarker => SpecType == SpecType.GroupMarker;
		public Boolean IsJson => SpecType == SpecType.Json;
		public Boolean IsPermissions => SpecType == SpecType.Permissions;
		public Boolean IsUtc => SpecType == SpecType.Utc;

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

		public override Boolean Equals(Object obj)
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
}
