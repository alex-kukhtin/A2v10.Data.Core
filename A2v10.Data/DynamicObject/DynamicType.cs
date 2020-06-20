// Copyright © 2015-2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace A2v10.Data
{
	public abstract class DynamicClass
	{
		public override String ToString()
		{
			var props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			var strProps = String.Join(", ", props.Select(p => $"{p.Name}={p.GetValue(this, null)}"));
			return $"{{{strProps}}}";
		}
	}

	public class DynamicProperty
	{
		public String Name { get; }
		public Type Type { get; }

		public DynamicProperty(String name, Type type)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Type = type ?? throw new ArgumentNullException(nameof(type));
		}

		public Boolean Equals(DynamicProperty other)
		{
			return Name != other.Name && Type == other.Type;
		}
	}

	internal class Signature : IEquatable<Signature>
	{
		private DynamicProperty[] _properties;
		public Int32 _hashCode;

		public Signature(Object obj)
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

		public DynamicProperty[] Properties => _properties;

		List<DynamicProperty> GetProperties(Object obj)
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

		public override Boolean Equals(Object obj)
		{
			return obj is Signature signature && Equals(signature);
		}

		public Boolean Equals(Signature other)
		{
			if (_properties.Length != other._properties.Length) 
				return false;
			for (Int32 i = 0; i < _properties.Length; i++)
			{
				if (!_properties[i].Equals(other._properties[i]))
					return false;
			}
			return true;
		}
	}
	public class ClassFactory
	{
		public static readonly ClassFactory Instance = new ClassFactory();

		private readonly ReaderWriterLock _rwLock;
		private readonly Dictionary<Signature, Type> _classes;
		private readonly ModuleBuilder _module;
		Int32 _classCount;

		static ClassFactory() { }  // Trigger lazy initialization of static fields

		private ClassFactory()
		{
			AssemblyName name = new AssemblyName("DynamicClasses");
			AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
			_module = assembly.DefineDynamicModule("Module");
			_classes = new Dictionary<Signature, Type>();
			_rwLock = new ReaderWriterLock();
		}

		public Type GetDynamicClass(IEnumerable<DynamicProperty> properties)
		{
			_rwLock.AcquireReaderLock(Timeout.Infinite);
			try
			{
				Signature signature = new Signature(properties);
				if (!_classes.TryGetValue(signature, out Type type))
				{
					type = CreateDynamicClass(signature.Properties);
					_classes.Add(signature, type);
				}
				return type;
			}
			finally
			{
				_rwLock.ReleaseReaderLock();
			}
		}

		Type CreateDynamicClass(DynamicProperty[] properties)
		{
			LockCookie cookie = _rwLock.UpgradeToWriterLock(Timeout.Infinite);
			try
			{
				var typeName = $"DynamicClass{_classCount + 1}";
				var tb = _module.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public, typeof(DynamicClass));
				GenerateProperties(tb, properties);
				var result = tb.CreateType();
				_classCount++;
				return result;
			}
			finally
			{
				_rwLock.DowngradeFromWriterLock(ref cookie);
			}
		}

		System.Reflection.FieldInfo[] GenerateProperties(TypeBuilder tb, DynamicProperty[] properties)
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
					null, new Type[] { dp.Type });
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
}
