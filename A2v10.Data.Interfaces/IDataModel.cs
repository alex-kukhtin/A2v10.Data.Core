// Copyright © 2015-2024 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Data.Interfaces;
public struct DataElementInfo
{
	public IDataMetadata? Metadata { get; set; }
	public Object? Element { get; set; }
	public Object? Id { get; set; }

	public override Boolean Equals(Object? obj)
	{
		throw new NotImplementedException();
	}

	public override Int32 GetHashCode()
	{
		throw new NotImplementedException();
	}

	public static Boolean operator ==(DataElementInfo left, DataElementInfo right)
	{
		return left.Equals(right);
	}

	public static Boolean operator !=(DataElementInfo left, DataElementInfo right)
	{
		return !(left == right);
	}
}

public interface IDataModel
{
	ExpandoObject Root { get; }
	ExpandoObject? System { get; }
	IDictionary<String, IDataMetadata> Metadata { get; }

	IDataHelper DataHelper { get; }

	DataElementInfo MainElement { get; }
	Boolean IsReadOnly { get; }
	Boolean IsEmpty { get; }
	void SetReadOnly();
	void MakeCopy();

	T? Eval<T>(String expression);
	T? Eval<T>(ExpandoObject root, String expression);

	T? CalcExpression<T>(String expression);
	T? CalcExpression<T>(ExpandoObject root, String expression);
	String? Resolve(String? source);

	void Merge(IDataModel src);

	String CreateScript(IDataScripter scripter);
	void AddRuntimeProperties();
	IDictionary<String, dynamic> GetDynamic();

	void Validate(IDataModelValidator validator);
}

