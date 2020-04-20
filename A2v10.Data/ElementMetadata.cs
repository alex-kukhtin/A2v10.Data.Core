﻿// Copyright © 2012-2019 Alex Kukhtin. All rights reserved.

using A2v10.Data.Interfaces;
using System;
using System.Collections.Generic;

namespace A2v10.Data
{
	public class ElementMetadata : IDataMetadata
	{
		private readonly IDictionary<String, IDataFieldMetadata> _fields = new Dictionary<String, IDataFieldMetadata>();

		public IDictionary<String, IList<String>> _cross = null;

		public String Id { get; private set; }
		public String Key { get; private set; }
		public String Name { get; private set; }
		public String RowNumber { get; private set; }
		public String HasChildren { get; private set; }
		public String Permissions { get; set; }
		public String Items { get; set; }
		public String MapItemType { get; set; }
		public String MainObject { get; set; }

		public Boolean IsArrayType { get; set; }
		public Boolean IsRowCount { get; set; }
		public Boolean IsGroup { get; set; }
		public Boolean HasCross => _cross != null;

		public SortedList<String, Tuple<Int32, String>> Groups { get; private set; }

		public IDictionary<String, IDataFieldMetadata> Fields => _fields;
		public IDictionary<String, IList<String>> Cross => _cross;

		public String FindPropertyByType(String typeName)
		{
			foreach (var f in Fields)
				if (f.Value.RefObject == typeName)
					return f.Key;
			return null;
		}

		public FieldMetadata AddField(FieldInfo field, DataType type, Int32 fieldLen = 0)
		{
			if (!field.IsVisible)
				return null;
			if (IsFieldExists(field.PropertyName, type, out FieldMetadata fm))
				return fm;
			fm = new FieldMetadata(field, type, fieldLen);
			_fields.Add(field.PropertyName, fm);
			switch (field.SpecType)
			{
				case SpecType.Id:
					Id = field.PropertyName;
					break;
				case SpecType.Key:
					Key = field.PropertyName;
					break;
				case SpecType.Name:
					Name = field.PropertyName;
					break;
				case SpecType.RowNumber:
					RowNumber = field.PropertyName;
					break;
				case SpecType.RowCount:
					IsRowCount = true;
					break;
				case SpecType.HasChildren:
					HasChildren = field.PropertyName;
					break;
				case SpecType.Permissions:
					Permissions = field.PropertyName;
					break;
				case SpecType.Items:
					Items = field.PropertyName;
					break;
			}
			return fm;
		}

		public void SetCrossObject(String key, String typeName)
		{
			if (_fields.TryGetValue(key, out IDataFieldMetadata iFM))
			{
				var fm = iFM as FieldMetadata;
				fm.SetType(typeName);
			}
		}

		public void AddCross(String key, IList<String> cross)
		{
			if (_cross == null)
				_cross = new Dictionary<String, IList<String>>();
			_cross.Add(key, cross);
		}

		public Int32 FieldCount { get { return _fields.Count; } }

		public Boolean ContainsField(String field)
		{
			return _fields.ContainsKey(field);
		}

		Boolean IsFieldExists(String name, DataType dataType, out FieldMetadata fm)
		{
			fm = null;
			if (_fields.TryGetValue(name, out IDataFieldMetadata ifm))
			{
				fm = ifm as FieldMetadata;
				if (fm.DataType != dataType)
					throw new DataLoaderException($"Invalid property '{name}'. Type mismatch. ({fm.DataType} <> {dataType})");
				return true;
			}
			return false;
		}

		public FieldMetadata GetField(String name)
		{
			if (_fields.TryGetValue(name, out IDataFieldMetadata fm))
			{
				return fm as FieldMetadata;
			}
			throw new DataLoaderException($"Field '{name}' not found.");
		}
	}
}
