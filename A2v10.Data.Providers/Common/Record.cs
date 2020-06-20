// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using A2v10.Data.Interfaces;

namespace A2v10.Data.Providers
{
	public class Record : IExternalDataRecord
	{
		public List<FieldData> DataFields;
		private IDictionary<String, Int32> _fieldMap;

		public Record(IDictionary<String, Int32> fields)
		{
			DataFields = new List<FieldData>();
			_fieldMap = fields ?? throw new ArgumentNullException(nameof(fields));
		}

		public Record(List<FieldData> dat, IDictionary<String, Int32> fields)
		{
			DataFields = dat;
			_fieldMap = fields ?? throw new ArgumentNullException(nameof(fields));
		}

		public Object FieldValue(String name)
		{
			name = FindFieldName(name);
			if (name == null)
				return null;
			if (_fieldMap.TryGetValue(name, out Int32 fieldNo))
			{
				if (fieldNo >= 0 && fieldNo < DataFields.Count)
					return DataFields[fieldNo].Value;
			}
			throw new ExternalDataException($"Invalid field name: '{name}'");
		}

		public Object FieldValue(Int32 index)
		{
			if (index >= 0 && index < DataFields.Count)
				return DataFields[index].Value;
			throw new ExternalDataException($"Invalid field index: {index}");
		}

		public String StringFieldValueByIndex(Int32 index)
		{
			if (index < DataFields.Count)
				return DataFields[index].StringValue;
			return null;
		}

		public void SetFieldValueString(Int32 index, String name, String value)
		{
			while (DataFields.Count <= index)
				DataFields.Add(new FieldData());
			if (!_fieldMap.ContainsKey(name))
				_fieldMap.Add(name, index);
			DataFields[index].StringValue = value;
		}

		public Boolean FieldExists(String name)
		{
			return _fieldMap.ContainsKey(name);
		}

		public String FindFieldName(String name)
		{
			if (!name.Contains("|"))
				return name;
			String retName = name;
			foreach (var f in name.Split('|'))
			{
				if (String.IsNullOrWhiteSpace(f))
					return null;
				if (_fieldMap.ContainsKey(f))
				{
					retName = f;
					// try to get all variants
					if (!IsFieldEmptyInternal(f))
						return f;
				}
			}
			return retName;
		}

		public Boolean IsFieldEmptyInternal(String name)
		{
			if (_fieldMap.TryGetValue(name, out Int32 fieldNo))
			{
				if (fieldNo >= 0 && fieldNo < DataFields.Count)
					return DataFields[fieldNo].IsEmpty;
			}
			return true;
		}

		public Boolean IsFieldEmpty(String name)
		{
			name = FindFieldName(name);
			if (name == null)
				return true;
			if (_fieldMap.TryGetValue(name, out Int32 fieldNo))
			{
				if (fieldNo >= 0 && fieldNo < DataFields.Count)
					return DataFields[fieldNo].IsEmpty;
			}
			throw new ExternalDataException($"Invalid field name: {name}");
		}

		public Boolean IsEmpty
		{
			get
			{
				foreach (var d in DataFields)
					if (!d.IsEmpty)
						return false;
				return true;
			}
		}
	}
}
