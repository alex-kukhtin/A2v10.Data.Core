// Copyright © 2015-2026 Oleksandr  Kukhtin. All rights reserved.

using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace A2v10.Data;
public abstract class DynamicClass
{
	public override String ToString()
	{
		var props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
		var strProps = String.Join(", ", props.Select(p => $"{p.Name}={p.GetValue(this, null)}"));
		return $"{{{strProps}}}";
	}
}

public record DynamicProperty(String Name, Type Type)
{
    public String Name { get; } = Name ?? throw new ArgumentNullException(nameof(Name));
    public Type Type { get; } = Type ?? throw new ArgumentNullException(nameof(Type));
}

internal class Signature : IEquatable<Signature>
{
	private DynamicProperty[]? _properties = null;
	public Int32 _hashCode = 0;

	public Signature(ExpandoObject obj)
	{
		Init(GetProperties(obj));
	}

	public Signature(IEnumerable<DynamicProperty> properties)
	{
		Init(properties);
	}

	void Init(IEnumerable<DynamicProperty> properties)
	{
		_properties = properties.ToArray();
		_hashCode = 0;

		foreach (DynamicProperty p in properties)
			_hashCode ^= p.Name.GetHashCode() ^ p.Type.GetHashCode();
	}

	public DynamicProperty[] Properties => _properties ?? [];

	static List<DynamicProperty> GetProperties(ExpandoObject obj)
	{
		var props = new List<DynamicProperty>();
		var d = obj as IDictionary<String, Object>;
		foreach (var itm in d)
		{
			switch (itm.Value)
			{
				case IList<ExpandoObject> _:
					props.Add(new DynamicProperty(itm.Key, typeof(IList<Object>)));
					break;
				case ExpandoObject _:
				case null:
					props.Add(new DynamicProperty(itm.Key, typeof(Object)));
					break;
				default:
					props.Add(new DynamicProperty(itm.Key, itm.Value.GetType()));
					break;
			}
		}
		return props;
	}

	public override Int32 GetHashCode()
	{
		return _hashCode;
	}

	public override Boolean Equals(Object? obj)
	{
		return obj is Signature signature && Equals(signature);
	}

	public Boolean Equals(Signature? other)
	{
		if (other == null)
			return false;
		if (_properties?.Length != other._properties?.Length)
			return false;
		if (other._properties == null)
			return false;
		for (Int32 i = 0; i < _properties?.Length; i++)
		{
			if (!_properties[i].Equals(other._properties[i]))
				return false;
		}
		return true;
	}
}
public class ClassFactory
{
	public static readonly ClassFactory Instance = new();

	// guards _classes, _module and _classCount together: lookup and creation are one step
	private readonly Object _lock = new();
	private readonly Dictionary<Signature, Type> _classes;
	private readonly ModuleBuilder _module;
	Int32 _classCount;

	static ClassFactory() { }  // Trigger lazy initialization of static fields

	private ClassFactory()
	{
		AssemblyName name = new("DynamicClasses");
		AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
		_module = assembly.DefineDynamicModule("Module");
		_classes = [];
	}

	public Type GetDynamicClass(IEnumerable<DynamicProperty> properties)
	{
		Signature signature = new(properties);
		lock (_lock)
		{
			if (!_classes.TryGetValue(signature, out Type? type))
			{
				type = CreateDynamicClass(signature.Properties);
				_classes.Add(signature, type);
			}
			return type;
		}
	}

	Type CreateDynamicClass(DynamicProperty[] properties)
	{
		var typeName = $"DynamicClass{_classCount + 1}";
		var tb = _module.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public, typeof(DynamicClass));
		GenerateProperties(tb, properties);
		var result = tb.CreateType()!;
		_classCount++;
		return result;
	}

	private static System.Reflection.FieldInfo[] GenerateProperties(TypeBuilder tb, DynamicProperty[] properties)
	{
		var fields = new FieldBuilder[properties.Length];
		for (Int32 i = 0; i < properties.Length; i++)
		{
			DynamicProperty dp = properties[i];
			var fb = tb.DefineField("_" + dp.Name, dp.Type, FieldAttributes.Private);
			var pb = tb.DefineProperty(dp.Name, PropertyAttributes.HasDefault, dp.Type, null);
			var mbGet = tb.DefineMethod("get_" + dp.Name,
				MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
				dp.Type, Type.EmptyTypes);
			ILGenerator genGet = mbGet.GetILGenerator();
			genGet.Emit(OpCodes.Ldarg_0);
			genGet.Emit(OpCodes.Ldfld, fb);
			genGet.Emit(OpCodes.Ret);
			var mbSet = tb.DefineMethod("set_" + dp.Name,
				MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
				null, [dp.Type]);
			ILGenerator genSet = mbSet.GetILGenerator();
			genSet.Emit(OpCodes.Ldarg_0);
			genSet.Emit(OpCodes.Ldarg_1);
			genSet.Emit(OpCodes.Stfld, fb);
			genSet.Emit(OpCodes.Ret);
			pb.SetGetMethod(mbGet);
			pb.SetSetMethod(mbSet);
			fields[i] = fb;
		}
		return fields;
	}

	public static Type CreateClass(IEnumerable<DynamicProperty> properties)
	{
		return ClassFactory.Instance.GetDynamicClass(properties);
	}
}

