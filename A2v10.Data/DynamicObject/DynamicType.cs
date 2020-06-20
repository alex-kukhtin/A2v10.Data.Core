// Copyright © 2015-2018 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace A2v10.Data
{
	public abstract class DynamicClass
	{
		public override String ToString()
		{
			PropertyInfo[] props = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			StringBuilder sb = new StringBuilder();
			sb.Append("{");
			for (Int32 i = 0; i < props.Length; i++)
			{
				if (i > 0) sb.Append(", ");
				sb.Append(props[i].Name);
				sb.Append("=");
				sb.Append(props[i].GetValue(this, null));
			}
			sb.Append("}");
			return sb.ToString();
		}
	}

	public class DynamicProperty
	{
		readonly String _name;
		readonly Type _type;

		public DynamicProperty(String name, Type type)
		{
			_name = name ?? throw new ArgumentNullException(nameof(name));
			_type = type ?? throw new ArgumentNullException(nameof(type));
		}

		public String Name => _name;
		public Type Type => _type;
	}

	internal class Signature : IEquatable<Signature>
	{
		public DynamicProperty[] properties;
		public Int32 hashCode;

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
			this.properties = properties.ToArray();
			hashCode = 0;
			foreach (DynamicProperty p in properties)
			{
				hashCode ^= p.Name.GetHashCode() ^ p.Type.GetHashCode();
			}
		}

		List<DynamicProperty> GetProperties(Object obj)
		{
			var props = new List<DynamicProperty>();
			var d = obj as IDictionary<String, Object>;
			foreach (var itm in d)
			{
				if (itm.Value is IList<ExpandoObject>)
					props.Add(new DynamicProperty(itm.Key, typeof(IList<Object>)));
				else if (itm.Value is ExpandoObject)
					props.Add(new DynamicProperty(itm.Key, typeof(Object)));
				else if (itm.Value == null)
					props.Add(new DynamicProperty(itm.Key, typeof(Object)));
				else
					props.Add(new DynamicProperty(itm.Key, itm.Value.GetType()));
			}
			return props;
		}

		public override Int32 GetHashCode()
		{
			return hashCode;
		}

		public override Boolean Equals(Object obj)
		{
			return obj is Signature ? Equals((Signature)obj) : false;
		}

		public Boolean Equals(Signature other)
		{
			if (properties.Length != other.properties.Length) return false;
			for (Int32 i = 0; i < properties.Length; i++)
			{
				if (properties[i].Name != other.properties[i].Name ||
					properties[i].Type != other.properties[i].Type) return false;
			}
			return true;
		}
	}
	public class ClassFactory
	{
		public static readonly ClassFactory Instance = new ClassFactory();

		ReaderWriterLock rwLock;
		Dictionary<Signature, Type> classes;
		Int32 classCount;
		readonly ModuleBuilder module;
		static ClassFactory() { }  // Trigger lazy initialization of static fields

		private ClassFactory()
		{
			AssemblyName name = new AssemblyName("DynamicClasses");
			AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
			module = assembly.DefineDynamicModule("Module");
			classes = new Dictionary<Signature, Type>();
			rwLock = new ReaderWriterLock();
		}

		public Type GetDynamicClass(IEnumerable<DynamicProperty> properties)
		{
			rwLock.AcquireReaderLock(Timeout.Infinite);
			try
			{
				Signature signature = new Signature(properties);
				if (!classes.TryGetValue(signature, out Type type))
				{
					type = CreateDynamicClass(signature.properties);
					classes.Add(signature, type);
				}
				return type;
			}
			finally
			{
				rwLock.ReleaseReaderLock();
			}
		}

		Type CreateDynamicClass(DynamicProperty[] properties)
		{
			LockCookie cookie = rwLock.UpgradeToWriterLock(Timeout.Infinite);
			try
			{
				String typeName = "DynamicClass" + (classCount + 1);
				TypeBuilder tb = this.module.DefineType(typeName, TypeAttributes.Class |
					TypeAttributes.Public, typeof(DynamicClass));
				System.Reflection.FieldInfo[] fields = GenerateProperties(tb, properties);
				Type result = tb.CreateType();
				classCount++;
				return result;
			}
			finally
			{
				rwLock.DowngradeFromWriterLock(ref cookie);
			}
		}

		System.Reflection.FieldInfo[] GenerateProperties(TypeBuilder tb, DynamicProperty[] properties)
		{
			System.Reflection.FieldInfo[] fields = new FieldBuilder[properties.Length];
			for (Int32 i = 0; i < properties.Length; i++)
			{
				DynamicProperty dp = properties[i];
				FieldBuilder fb = tb.DefineField("_" + dp.Name, dp.Type, FieldAttributes.Private);
				PropertyBuilder pb = tb.DefineProperty(dp.Name, PropertyAttributes.HasDefault, dp.Type, null);
				MethodBuilder mbGet = tb.DefineMethod("get_" + dp.Name,
					MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
					dp.Type, Type.EmptyTypes);
				ILGenerator genGet = mbGet.GetILGenerator();
				genGet.Emit(OpCodes.Ldarg_0);
				genGet.Emit(OpCodes.Ldfld, fb);
				genGet.Emit(OpCodes.Ret);
				MethodBuilder mbSet = tb.DefineMethod("set_" + dp.Name,
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
