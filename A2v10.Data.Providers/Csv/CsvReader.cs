// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using A2v10.Data.Interfaces;

namespace A2v10.Data.Providers.Csv
{
	public class CsvReader :  IExternalDataReader
	{
		private readonly DataFile _file;

		const Char QUOTE = '"';

		Char _backwardChar = '\0';

		public CsvReader(DataFile file)
		{
			_file = file;
		}

		public IExternalDataFile Read(Stream stream)
		{
			// FindEncoding & delimiter
			FindEncoding(stream);
			using (StreamReader rdr = new StreamReader(stream, _file.Encoding))
			{
				ReadHeader(rdr);
				Read(rdr);
			}
			return _file; 
		}

		public ExpandoObject ParseFile(Stream stream, ITableDescription table)
		{
			// extension
			return this.ParseFlatFile(stream, table);
		}

		public ExpandoObject CreateDataModel(Stream stream)
		{
			// extension
			return this.CreateExpandoObject(stream);
		}

		void FindEncoding(Stream stream)
		{
			using (var br = new BinaryReader(stream, Encoding.Default, true))
			{
				var bytes = br.ReadBytes(2048);
				_file.Encoding = _file.FindDecoding(bytes);
			}
			stream.Seek(0, SeekOrigin.Begin);
		}

		void ReadHeader(StreamReader rdr)
		{
			String header = ReadLine(rdr);
			ParseHeader(header);
		}

		String ReadLine(StreamReader rdr)
		{
			StringBuilder sb = new StringBuilder();
			if (_backwardChar != '\0')
			{
				sb.Append(_backwardChar);
				_backwardChar = '\0';
			}

			Char readString(Char current)
			{
				while (!rdr.EndOfStream)
				{
					Char ch = (Char)rdr.Read();
					if (ch == QUOTE)
					{
						sb.Append(ch);
						Char next = (Char)rdr.Read();
						if (next == QUOTE)
						{
							sb.Append(next);
						}
						else
						{
							return next;
						}
					}
					else
					{
						sb.Append(ch);
					}
				}
				return '\0';
			}

			while (!rdr.EndOfStream)
			{
				Char ch = (Char) rdr.Read();
				if (ch == QUOTE)
				{
					sb.Append(ch);
					ch = readString(ch);
				}
				// continue
				if (ch == '\n' || ch == '\r')
				{
					Char next = (Char)rdr.Read();
					if (next != '\n' && next != '\r' && !rdr.EndOfStream)
					{
						_backwardChar = next;
					}
					break;
				}
				else
				{
					sb.Append(ch);
				}
				
			}
			return sb.ToString();
		}

		void Read(StreamReader rdr)
		{
			while (!rdr.EndOfStream)
			{
				String line = ReadLine(rdr);
				var items = ParseLine(line);
				var r = _file.CreateRecord();
				for (var i = 0; i < items.Count; i++)
					r.DataFields.Add(new FieldData() { StringValue = items[i] });
			}
		}

		void ParseHeader(String header)
		{
			// find delimiters
			var delims = new Dictionary<Char, Int32>()
			{
				{ ';',  0 },
				{ ',',  0 },
				{ '\t', 0 },
				{ '|',  0 },
			};
			for (Int32 i=0; i<header.Length; i++)
			{
				Char ch = header[i];
				if (delims.TryGetValue(ch, out Int32 cnt))
					delims[ch] = cnt + 1;
			}
			var list = delims.ToList();
			list.Sort((v1, v2) => v2.Value.CompareTo(v1.Value)); // desc
			_file.Delimiter = list[0].Key;
			var fields = ParseLine(header);
			for (var i = 0; i < fields.Count; i++) {
				var f = _file.CreateField();
				f.Name = fields[i];
			}
			_file.MapFields();
		}

		IList<String> ParseLine(String line)
		{
			// very simple tokenizer
			Int32 ix = 0;
			Int32 len = line.Length;
			StringBuilder token = new StringBuilder();
			Char ch;
			var retval = new List<String>();

			Char _nextChar()
			{
				if (ix >= len)
					return '\0';
				Char currChar = line[ix];
				ix++;
				return currChar;
			}

			void _addToken()
			{
				retval.Add(token.ToString());
				token.Clear();
			}

			void _readString()
			{
				Char sch;
				token.Clear();
				while (true)
				{
					sch = _nextChar();
					if (sch == '\0')
						break;
					else if (sch == QUOTE)
					{
						var nextStrChar = _nextChar();
						if (nextStrChar == QUOTE)
							token.Append(nextStrChar);
						else
						{
							_addToken();
							break;
						}
					}
					else
					{
						token.Append(sch);
					}
				}
			}

			while (true)
			{
				ch = _nextChar();
				if (ch == '\0')
				{
					if (token.Length > 0)
						retval.Add(token.ToString());
					return retval;
				}
				if (ch == _file.Delimiter)
				{
					_addToken();
				}
				else if (ch == QUOTE)
				{
					if (token.Length == 0)
						_readString();
					else
						token.Append(ch); // inside string
				}
				else
				{
					token.Append(ch);
				}
			}
		}
	}
}
