
using System;
using System.Collections.Generic;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using Mono.Cecil;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public static class ILRuntimeBindingHelper
{
    public static ModuleDefinition ReadModule(string path)
    {
        // 最好不要用 Editor 生成的 IL
        return ModuleDefinition.ReadModule(path);
    }

    public static Dictionary<CLRType, List<MemberReference>> GetAllMonoCecilReference(AppDomain domain, ModuleDefinition moduleDef)
    {
        var dict = new Dictionary<CLRType, List<MemberReference>>();
        foreach (var memberReference in moduleDef.GetMemberReferences())
        {
            var clrType = GetCLRType(domain, memberReference.DeclaringType);
            if (!dict.ContainsKey(clrType))
            {
                dict.Add(clrType, new List<MemberReference>());
            }
            dict[clrType].Add(memberReference);

//            Debug.Log(memberReference.DeclaringType + "\t" + memberReference);
        }
        return dict;
    }

    public static List<ILRuntimeBindingTypeInfo> GetAllReference(AppDomain domain,
        Dictionary<CLRType, List<MemberReference>> monocecilDict)
    {
        var list = new List<ILRuntimeBindingTypeInfo>();
        foreach (KeyValuePair<CLRType, List<MemberReference>> pair in monocecilDict)
        {
            var type = pair.Key;
            var typeInfo = new ILRuntimeBindingTypeInfo(type);
            foreach (var memberReference in pair.Value)
            {
                if (memberReference is FieldReference)
                {
                    typeInfo.Fields.AddIfWithout(type.GetField(type.GetFieldIndex(memberReference)));
                }
                else if (memberReference is MethodReference)
                {
                    var methodRef = memberReference as MethodReference;
                    var typeList = new List<IType>();
                    foreach (var parameterDefinition in methodRef.Parameters)
                    {
                        typeList.Add(GetCLRType(domain, parameterDefinition.ParameterType, type));
                    }
                    if (IsConstructor(methodRef.Name))
                    {
                        var method = type.GetConstructor(typeList) as CLRMethod;
                        if (method != null)
                        {
                            typeInfo.Constructors.AddIfWithout(method);
                        }
                        else
                        {
                            // 有可能没有定义
                        }
                    }
                    else
                    {
                        if (!methodRef.HasGenericParameters)
                        {
                            typeInfo.Methods.AddIfWithout(type.GetMethod(methodRef.Name, typeList, null) as CLRMethod);
                        }
                        else
                        {
                            typeInfo.Methods.AddIfWithout(type.GetMethod(methodRef.Name, typeList.Count) as CLRMethod);
                        }
                    }
                }
            }

            list.Add(typeInfo);

//            Debug.Log(typeInfo);
        }
        return list;
    }

    private static CLRType GetCLRType(AppDomain domain, TypeReference typeRef, IType contextType = null, IMethod contextMethod = null)
    {
        return domain.GetType(typeRef, contextType, contextMethod) as CLRType;
//        return domain.GetType(string.Format("{0}, {1}", typeRef.FullName, typeRef.Scope.Name)) as CLRType;
    }

    private static bool IsConstructor(string methodName)
    {
        return methodName == ".ctor";
    }
}
