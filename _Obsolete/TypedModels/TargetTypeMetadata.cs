// Copyright © 2022-2023 Oleksandr Kukhtin. All rights reserved.

using System.Reflection;

namespace A2v10.Data;

internal class TargetFieldMetadata
{
    private readonly PropertyInfo _propertyInfo;
    private readonly ConstructorInfo? _constructorInfo;
    private readonly MethodInfo? _addMethodInfo;

    public TargetFieldMetadata(PropertyInfo propertyInfo)
    {
        _propertyInfo = propertyInfo;
    }
    public TargetFieldMetadata(PropertyInfo propertyInfo, Type type, MethodInfo? addMethod = null)
    {
        _propertyInfo = propertyInfo;
        _constructorInfo = type.GetConstructor(BindingFlags.Public| BindingFlags.Instance, new Type[] { });
        _addMethodInfo = addMethod;
    }

    public Object CreateObject()
    {
        if (_constructorInfo == null)
            throw new InvalidOperationException("Ctor not found");
        return _constructorInfo.Invoke(null);
    }
    public void AddToArray(Object array, Object obj)
    {
        if (_addMethodInfo == null)
            throw new InvalidOperationException("Add method not found");
        _addMethodInfo.Invoke(array, new Object[] { obj });
    }

    public Object GetOrCreateObject(Object data)
    {
        if (_constructorInfo == null)
            throw new InvalidOperationException("Ctor not found");
        var val = _propertyInfo.GetValue(data);
        if (val != null)
            return val;
        val =_constructorInfo.Invoke(null);
        _propertyInfo.SetValue(data, val);
        return val;
    }
    public Boolean IsString => _propertyInfo.PropertyType == typeof(String);
    public Object? GetValue(Object data) 
    {
        return _propertyInfo.GetValue(data);
    }
    public void SetValue(Object data, Object? value)
    {
        _propertyInfo.SetValue(data, value);    
    }
}

internal class ObjectMetadata
{
    public ObjectMetadata(Type type)
    {
        Type = type;
    }
    public Type Type { get; }
    private readonly Dictionary<String, TargetFieldMetadata> _fields = new();

    public TargetFieldMetadata? GetFieldMetadata(String name)
    {
        if (_fields.TryGetValue(name, out TargetFieldMetadata? field))
            return field;
        return null;
    }
    public TargetFieldMetadata AddField(PropertyInfo pi)
    {
        var fm = new TargetFieldMetadata(pi);
        _fields.Add(pi.Name, fm);
        return fm;
    }
    public TargetFieldMetadata AddField(PropertyInfo pi, ObjectMetadata obj, MethodInfo? addMethod = null)
    {
        var fm = new TargetFieldMetadata(pi, obj.Type, addMethod);
        _fields.Add(pi.Name, fm);
        return fm;
    }
}

internal class TargetTypeMetadata
{
    public void Build(Type type)
    {
        Create(type);
    }

    private readonly Dictionary<Type, ObjectMetadata> _types = new();
    public TargetFieldMetadata? GetFieldMetadata(Type tp, String name)
    {
        return GetObjectMetadata(tp).GetFieldMetadata(name);
    }

    public ObjectMetadata GetObjectMetadata(Type tp)
    {
        if (!_types.TryGetValue(tp, out ObjectMetadata? objMeta))
            throw new InvalidOperationException($"Metadata not found {tp}");
        return objMeta;
    }

    public Object? GetOrCreateElement(Object data, String name)
    {
        var t = data.GetType(); 
        var fm = GetFieldMetadata(t, name);
        if (fm == null)
            return null;
        return fm.GetOrCreateObject(data);
    }

    ObjectMetadata Create(Type type)
    {
        if (_types.TryGetValue(type, out ObjectMetadata? objMeta)) 
            return objMeta;
        var om = new ObjectMetadata(type);
        _types.Add(type, om);   
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            var pt = prop.PropertyType;
            if (pt.IsClass)
            {
                if (pt == typeof(String))
                    om.AddField(prop);
                else if (pt.IsGenericType)
                {
                    var mi = pt.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
                    foreach (var gt in pt.GenericTypeArguments)
                    {
                        om.AddField(prop, Create(gt), mi);
                    }
                }
                else
                    om.AddField(prop, Create(prop.PropertyType));
            }
            else
                om.AddField(prop);
        }
        return om;
    }
}
