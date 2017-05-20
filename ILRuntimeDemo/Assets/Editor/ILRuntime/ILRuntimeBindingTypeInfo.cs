
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using UnityEngine;

public class ILRuntimeBindingTypeInfo
{
    public CLRType DeclaringType;

    public List<FieldInfo> Fields;
    public List<PropertyInfo> Propertys;
    public List<CLRMethod> Constructors;
    public List<CLRMethod> Methods;

    public ILRuntimeBindingTypeInfo(CLRType declaringType)
    {
        DeclaringType = declaringType;
        Fields = new List<FieldInfo>();
        Propertys = new List<PropertyInfo>();
        Constructors = new List<CLRMethod>();
        Methods = new List<CLRMethod>();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        try
        {
            sb.AppendLine(DeclaringType.FullName);
            foreach (var fieldInfo in Fields)
            {
                sb.AppendLine(fieldInfo.ToString());
            }
            foreach (var propertyInfo in Propertys)
            {
                sb.AppendLine(propertyInfo.ToString());
            }
            foreach (var constructorInfo in Constructors)
            {
                sb.AppendLine(constructorInfo.ToString());
            }
            foreach (var methodInfo in Methods)
            {
                sb.AppendLine(methodInfo.ToString());
            }
        }
        catch (Exception e)
        {
            Debug.LogError(DeclaringType.FullName);

            throw e;
        }

        return sb.ToString();
    }
}
