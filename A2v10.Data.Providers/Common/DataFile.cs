// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;

using A2v10.Data.Interfaces;

namespace A2v10.Data.Providers
{
	public enum DataFileFormat
	{
		dbf,
		csv
	}

	public class DataFile : IExternalDataFile
	{
		readonly IList<Field> _fields;
		readonly IList<Record> _records;

		private readonly Byte[] byteCodes1251 = new Byte[] { 0x81, 0x83, 0xA0, 0xA1, 0xA2, 0xA5, 0xA8, 0xAA, 0xAF, 0xB2, 0xB3, 0xB4, 0xB9, 0xBA, 0xBF, 0xBB, 0xAB };

		public DateTime LastModifedDate { get; set; }

		public DataFile()
		{
			_fields = new List<Field>();
			_records = new List<Record>();
			LastModifedDate = DateTime.Today;
			Encoding = null; // automatic
		}

		public Encoding Encoding { get; set; }
		public Char Delimiter { get; set; }
		public DataFileFormat Format { get; set; }

		public static Boolean IsNormalString(String str)
		{
			var arr = str.ToCharArray();
			Int32 normalCharsCount = 0;
			foreach (var ch in str.ToCharArray())
			{
				if (Char.IsLetterOrDigit(ch) || Char.IsWhiteSpace(ch) || Char.IsPunctuation(ch) || Char.IsSymbol(ch))
					normalCharsCount += 1;
			}
			return normalCharsCount == arr.Length;
		}

		public Encoding FindDecoding(Byte[] chars)
		{
			if (Encoding != null)
				return Encoding;
			// TODO: Get BOM bytes

			Int32 countASCII = 0;
			Int32 count866 = 0;
			Int32 count1251 = 0;
			for (Int32 i = 0; i < chars.Length; i++)
			{
				Byte ch = chars[i];
				if (ch < 0x80)
				{
					countASCII += 1;
					continue;
				}
				Boolean b1251 = false;
				Boolean b866 = false;
				if (ch >= 0xC0 && ch <= 0xFF || Array.IndexOf(byteCodes1251, ch) != -1)
				{
					count1251 += 1;
					b1251 = true;
				}
				if (ch >= 0x80 && ch <= 0xAF || ch >= 0xE0 && ch <= 0xF7)
				{
					count866 += 1;
					b866 = true;
				}
				if (!b1251 && !b866)
				{
					// invalid symbol ?
				}
				/*
				if (!b1251)
				{
				}
				if (!b866) {
				}
				*/
			}
			if (countASCII == chars.Length)
			{
				// do not save!
				return Encoding.ASCII;
			}
			count1251 += countASCII;
			count866 += countASCII;
			var totalCount = chars.Length;
			if (count1251 == totalCount && count866 < totalCount)
			{
				this.Encoding = Encoding.GetEncoding(1251);
				return this.Encoding;
			}
			else if (count866 == totalCount && count1251 < totalCount)
			{
				this.Encoding = Encoding.GetEncoding(866);
				return this.Encoding;
			}
			else
			{
				// try UTF-8
				String str = Encoding.UTF8.GetString(chars);
				if (IsNormalString(str))
				{
					this.Encoding = Encoding.UTF8;
					return this.Encoding;
				}
			}

			this.Encoding = Encoding.ASCII;
			return this.Encoding;
		}

		public Int32 FieldCount => _fields.Count;
		public Int32 NumRecords => _records.Count;

		public Field CreateField()
		{
			var f = new Field();
			_fields.Add(f);
			return f;
		}

		public Field CreateField(String name, SqlDataType dataType)
		{
			var f = new Field()
			{
				Name = name
			};
			switch (Format)
			{
				case DataFileFormat.dbf:
					f.SetFieldTypeDbf(dataType);
					break;
				case DataFileFormat.csv:
					f.SetFieldTypeCsv(dataType);
					break;
			}
			_fields.Add(f);
			return f;
		}

		public Field GetField(Int32 index)
		{
			if (index < 0 || index >= _fields.Count)
				throw new InvalidOperationException();
			return _fields[index];
		}

		public String FieldName(Int32 index)
		{
			return GetField(index).Name;
		}

		public Int32 GetOrCreateField(String name)
		{
			MapFields();
			if (_fieldMap.TryGetValue(name, out Int32 index))
				return index;
			_fields.Add(new Field() { Name = name, Type = FieldType.Char });
			Int32 ix = _fields.Count - 1;
			_fieldMap.Add(name, ix);
			return ix;
		}

		public IEnumerable<Field> Fields => _fields;

		private IDictionary<String, Int32> _fieldMap;

		internal void MapFields()
		{
			if (_fieldMap != null)
				return;
			_fieldMap = new Dictionary<String, Int32>();
			for (Int32 f = 0; f < _fields.Count; f++)
			{
				var name = _fields[f].Name;
				int md = 1;
				while (_fieldMap.ContainsKey(name))
				{
					name += $"_{md++}";
				}
				_fields[f].Name = name; // maybe changed
				_fieldMap.Add(name, f);
			}
		}

		public Record CreateRecord()
		{
			MapFields(); // ensure exists
			var r = new Record(_fieldMap);
			_records.Add(r);
			return r;
		}

		public Record GetRecord(Int32 index)
		{
			if (index < 0 || index >= _records.Count)
				throw new InvalidOperationException();
			return _records[index];
		}
		public IEnumerable<IExternalDataRecord> Records => _records;


		public void FillModel(IDataModel model)
		{
			if (model.Metadata == null || !model.Metadata.ContainsKey("TRoot"))
				throw new ExternalDataException("External data. Empty model");
			var r = model.Metadata["TRoot"];
			if (r?.Fields?.Count != 1)
				throw new ExternalDataException("External data. The model must contain one array");
			foreach (var prop in r.Fields.Keys)
			{
				var f = r.Fields[prop];
				CreateStructure(model.Metadata[f.RefObject]);
				var array = model.Eval<IList<ExpandoObject>>(prop);
				if (array == null)
					throw new ExternalDataException($"External data. '{prop}' field must be an array");
				FillData(array);
				FitStringFields();
				return;
			}
		}

		void CreateStructure(IDataMetadata meta)
		{
			foreach (var m in meta.Fields)
			{
				CreateField(m.Key, m.Value.SqlDataType);
			}
		}

		void FillData(IList<ExpandoObject> list)
		{
			foreach (var elem in list)
			{
				var de = elem as IDictionary<String, Object>;
				var r = CreateRecord();
				for (var i = 0; i < _fields.Count; i++)
				{
					var f = _fields[i];
					var val = de[f.Name];
					var fd = new FieldData(Format, f.Type, val);
					r.DataFields.Add(fd);
				}
			}
		}

		void FitStringFields()
		{
			if (Format != DataFileFormat.dbf)
				return;
			// sets the string field sizes accodring to values
			for (var i = 0; i < _fields.Count; i++)
			{
				var f = _fields[i];
				if (f.Type != FieldType.Char)
					continue;
				var len = 0;
				foreach (var r in Records)
				{
					var fv = r.FieldValue(i);
					if (fv == null)
						continue;
					var sv = fv.ToString();
					len = Math.Max(len, sv.Length);
				}
				f.Size = len + 1;
			}
		}

	}
}
