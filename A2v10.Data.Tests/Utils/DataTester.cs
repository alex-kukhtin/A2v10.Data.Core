// Copyright © 2015-2023 Oleksandr Kukhtin. All rights reserved.

using System.Dynamic;

namespace A2v10.Data.Tests;

public class DataTester
{
	private readonly IDataModel _dataModel;

	private readonly ExpandoObject? _instance;
	private readonly IList<ExpandoObject>? _instanceArray;

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
		Assert.IsNotNull(_instance, "Could not evaluate expression 'Root'");
	}

	public void AllProperties(String props)
	{
		var propArray = props.Split(',');
		var dict = _instance as IDictionary<String, Object?>;
		Assert.IsNotNull(dict);
		foreach (var prop in propArray)
			Assert.IsTrue(dict.ContainsKey(prop), $"Property {prop} not found");
		Assert.HasCount(propArray.Length, dict, $"invalid length for '{props}'");
	}

	public void AreValueEqual<T>(T? expected, String property)
	{
		var obj = _instance as IDictionary<String, Object?>;
		Assert.IsNotNull(obj);
		Assert.AreEqual(expected, obj[property]);
	}

	public void IsNull(String property)
	{
		if (_instance is not IDictionary<String, Object?> obj)
			throw new InvalidProgramException(nameof(_instance));
		Assert.IsNull(obj[property]);
	}

	public void IsArray(Int32 length = -1)
	{
		Assert.IsTrue(_instanceArray != null && _instance == null);
		if (length != -1)
			Assert.HasCount(length, _instanceArray);
	}

	public void AreArrayValueEqual<T>(T? expected, Int32 index, String property)
	{
		IsArray();
		Assert.IsNotNull(_instanceArray);
		var obj = _instanceArray[index] as IDictionary<String, Object>;
		Assert.AreEqual(expected, obj[property]);
	}
    public void IsArrayValueNull(Int32 index, String property)
    {
        IsArray();
        Assert.IsNotNull(_instanceArray);
        var obj = _instanceArray[index] as IDictionary<String, Object>;
        Assert.IsNull(obj[property]);
    }

    public T? GetArrayValue<T>(Int32 index, String property)
	{
		IsArray();
		Assert.IsNotNull(_instanceArray);
		var obj = _instanceArray[index] as ExpandoObject;
		return obj.Get<T>(property);
	}

	public T? GetValue<T>(String property)
	{
		Assert.IsNotNull(_instance);
		return _instance.Get<T>(property);
	}
}

