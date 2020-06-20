// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Dynamic;

using A2v10.Data.Interfaces;

namespace A2v10.Data.Tests
{
	public class DataTester
	{
		IDataModel _dataModel;
		readonly ExpandoObject _instance;
		IList<ExpandoObject> _instanceArray;
		public DataTester(IDataModel dataModel, String expression)
		{
			_dataModel = dataModel;
			_instance = dataModel.Eval<ExpandoObject>(expression);
			_instanceArray = dataModel.Eval<IList<ExpandoObject>>(expression);
			Assert.IsTrue(_instance != null || _instanceArray != null, $"Could not evaluate expression '{expression}'");
		}

		public DataTester(IDataModel dataModel)
		{
			_dataModel = dataModel;
			_instance = _dataModel.Root;
			Assert.IsTrue(_instance != null, "Could not evaluate expression 'Root'");
		}

		public void AllProperties(String props)
		{
			var propArray = props.Split(',');
			var dict = _instance as IDictionary<String, Object>;
			foreach (var prop in propArray)
				Assert.IsTrue(dict.ContainsKey(prop), $"Property {prop} not found");
			Assert.AreEqual(propArray.Length, dict.Count, $"invalid length for '{props}'");
		}

		public void AreValueEqual<T>(T expected, String property)
		{
			var obj = _instance as IDictionary<String, Object>;
			Assert.AreEqual(expected, obj[property]);
		}

		public void IsNull(String property)
		{
			var obj = _instance as IDictionary<String, Object>;
			Assert.IsNull(obj[property]);
		}

		public void IsArray(Int32 length = -1)
		{
			Assert.IsTrue(_instanceArray != null && _instance == null);
			if (length != -1)
				Assert.AreEqual(length, _instanceArray.Count);
		}

		public void AreArrayValueEqual<T>(T expected, Int32 index, String property)
		{
			IsArray();
			var obj = _instanceArray[index] as IDictionary<String, Object>;
			Assert.AreEqual(expected, obj[property]);
		}

		public T GetArrayValue<T>(Int32 index, String property)
		{
			IsArray();
			var obj = _instanceArray[index] as ExpandoObject;
			return obj.Get<T>(property);
		}

		public T GetValue<T>(String property)
		{
			return _instance.Get<T>(property);
		}
	}
}
