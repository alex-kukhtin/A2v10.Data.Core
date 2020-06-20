// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using A2v10.Data.Interfaces;

namespace A2v10.Data.Tests
{
	public class MetadataTester
	{
		public IDictionary<String, IDataMetadata> _meta;

		public MetadataTester(IDataModel dataModel)
		{
			_meta = dataModel.Metadata as IDictionary<String, IDataMetadata>;
			Assert.IsNotNull(_meta);
		}

		public void IsAllKeys(String props)
		{
			var propArray = props.Split(',');
			foreach (var prop in propArray)
				Assert.IsTrue(_meta.ContainsKey(prop));
			Assert.AreEqual(propArray.Length, _meta.Count, $"invalid length for '{props}'");
		}

		public void HasAllProperties(String key, String props)
		{
			var data = _meta[key] as ElementMetadata;
			var propArray = props.Split(',');
			foreach (var prop in propArray)
				Assert.IsTrue(data.ContainsField(prop), $"'{prop}' not found");
			Assert.AreEqual(propArray.Length, data.FieldCount, $"invalid length for '{props}' properties");
		}

		public void IsId(String key, String prop)
		{
			var data = _meta[key] as ElementMetadata;
			Assert.AreEqual(data.Id, prop);
		}

		public void IsKey(String key, String prop)
		{
			var data = _meta[key] as ElementMetadata;
			Assert.AreEqual(data.Key, prop);
		}

		public void IsName(String key, String prop)
		{
			var data = _meta[key] as ElementMetadata;
			Assert.AreEqual(data.Name, prop);
		}

		public void IsType(String key, String propName, DataType dt)
		{
			var data = _meta[key] as ElementMetadata;
			Assert.IsTrue(data.ContainsField(propName));
			var fp = data.GetField(propName);
			Assert.AreEqual(fp.DataType, dt);
		}

		public void IsItemType(String key, String propName, FieldType ft)
		{
			var data = _meta[key] as ElementMetadata;
			Assert.IsTrue(data.ContainsField(propName), $"Invalid item type for {key}.{propName}");
			var fp = data.GetField(propName);
			Assert.AreEqual(fp.ItemType, ft);
		}

		public void IsItemRefObject(String key, String propName, String refObject, FieldType ft)
		{
			var data = _meta[key] as ElementMetadata;
			Assert.IsTrue(data.ContainsField(propName));
			var fp = data.GetField(propName);
			Assert.AreEqual(fp.RefObject, refObject);
			Assert.AreEqual(fp.ItemType, ft);
		}

		public void IsItemIsArrayLike(String key, String propName)
		{
			var data = _meta[key] as ElementMetadata;
			Assert.IsTrue(data.ContainsField(propName));
			var fp = data.GetField(propName);
			Assert.IsTrue(fp.IsArrayLike);
		}
	}
}
