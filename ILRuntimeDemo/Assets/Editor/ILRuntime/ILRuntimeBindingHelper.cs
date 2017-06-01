
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using Mono.Cecil;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public static class ILRuntimeBindingHelper
{
    #region 获取依赖
    private static Dictionary<string, AssemblyDefinition> _referenceDict = new Dictionary<string, AssemblyDefinition>();

    private static AssemblyDefinition AssemblyResolver_ResolveFailure(object sender, AssemblyNameReference reference)
    {
        if (!_referenceDict.ContainsKey(reference.FullName))
        {
            // 这里也偷懒用了 Editor 的
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == reference.Name)
                {
                    FileHelper.ReadFileStream(assembly.Location, FileMode.Open, FileAccess.Read, stream =>
                    {
                        _referenceDict[reference.FullName] = AssemblyDefinition.ReadAssembly(stream);
                    });
                }
            }
        }

        return _referenceDict[reference.FullName];
    }

    public static ModuleDefinition ReadModule(string path)
    {
        // 最好不要用 Editor 生成的 IL
        var def = ModuleDefinition.ReadModule(path);
        //        var def = AssemblyDefinition.ReadAssembly(path, new ReaderParameters(ReadingMode.Immediate)).MainModule;
        def.AssemblyResolver.ResolveFailure += AssemblyResolver_ResolveFailure;
        return def;
    }

    public static Dictionary<CLRType, List<MemberReference>> GetAllMonoCecilReference(AppDomain domain, ModuleDefinition moduleDef)
    {
        var dict = new Dictionary<CLRType, List<MemberReference>>();
        foreach (var memberReference in moduleDef.GetMemberReferences())
        {
            var clrType = GetIType(domain, memberReference.DeclaringType) as CLRType;
            if (!dict.ContainsKey(clrType))
            {
                dict.Add(clrType, new List<MemberReference>());
            }
            dict[clrType].AddIfWithout(memberReference);

            //            Debug.Log(memberReference.DeclaringType + "\t" + memberReference);
        }
        return dict;
    }

    public static List<ILRuntimeBindingTypeInfo> GetAllReference(AppDomain domain, ModuleDefinition moduleDef,
        Dictionary<CLRType, List<MemberReference>> monocecilDict)
    {
        var list = new List<ILRuntimeBindingTypeInfo>();
        var genericDict = new Dictionary<string, ILRuntimeBindingTypeInfo>();
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
                        typeList.Add(GetIType(domain, parameterDefinition.ParameterType, type));
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
                            genericDict[methodRef.FullName] = typeInfo;
                        }
                    }
                }
            }

            list.Add(typeInfo);
        }

        var hasCheckGenericList = new List<string>();
        foreach (var typeDefinition in moduleDef.GetTypes())
        {
            CheckCrossingAdaptor(typeDefinition, domain);

            foreach (var methodDefinition in typeDefinition.Methods)
            {
                if (methodDefinition.HasBody)
                {
                    foreach (var instruction in methodDefinition.Body.Instructions)
                    {
                        if (instruction.Operand is GenericInstanceMethod)
                        {
                            var genericInstanceMethod = instruction.Operand as GenericInstanceMethod;
                            var fullName = genericInstanceMethod.ElementMethod.FullName;
                            if (genericDict.ContainsKey(fullName))
                            {
                                var ilType = GetIType(domain, typeDefinition) as ILType;
                                var ilMethod = new ILMethod(methodDefinition, GetIType(domain, typeDefinition) as ILType, domain);
                                bool va;
                                var method = domain.GetMethod(genericInstanceMethod, ilType, ilMethod, out va);
                                var match = false;
                                foreach (var clrMethod in genericDict[fullName].Methods)
                                {
                                    if (clrMethod.ToString() == method.ToString())
                                    {
                                        match = true;
                                        break;
                                    }
                                }
                                if (!match)
                                {
                                    genericDict[fullName].Methods.AddIfWithout(method as CLRMethod);
                                }
                                hasCheckGenericList.AddIfWithout(fullName);
                            }
                        }
                    }
                }
            }
        }

        if (hasCheckGenericList.Count != genericDict.Count)
        {
            foreach (var pair in genericDict)
            {
                if (!hasCheckGenericList.Contains(pair.Key))
                {
                    Debug.LogError("没有找到引用：" + pair.Key);
                }
            }
        }

        return list;
    }

    private static IType GetIType(AppDomain domain, TypeReference typeRef, IType contextType = null, IMethod contextMethod = null)
    {
        return domain.GetType(typeRef, contextType, contextMethod);
        //        return domain.GetType(string.Format("{0}, {1}", typeRef.FullName, typeRef.Scope.Name)) as CLRType;
    }

    private static bool IsConstructor(string methodName)
    {
        return methodName == ".ctor";
    }

    private static bool CheckCrossingAdaptor(TypeDefinition typeDef, AppDomain domain)
    {
        var ilType = GetIType(domain, typeDef) as ILType;

        try
        {
            return !(ilType.BaseType is CLRType);
        }
        catch (Exception e)
        {
            if (ilType.Name == "TestInheritance")
            {
                Debug.Log(e.Message);

                return false;
            }
            else
            {
                throw e;
            }
        }
    }
    #endregion

    #region 生成代码
    private static readonly StringBuilder _stringBuilder = new StringBuilder();

    private static void ClearStringBuilder()
    {
        _stringBuilder.Length = 0;
    }

    private static void StringBuilderAppend(string str)
    {
        _stringBuilder.Append(str);
    }

    private static string StringBuilderValue()
    {
        return _stringBuilder.ToString();
    }

    public static string GetParamsStr(ParameterInfo[] parameters)
    {
        ClearStringBuilder();

        var first = true;
        foreach (var param in parameters)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                StringBuilderAppend(", ");
            }

            if (param.IsOut)
            {
                StringBuilderAppend("out ");
            }
            else if (param.ParameterType.IsByRef)
            {
                StringBuilderAppend("ref ");
            }
            StringBuilderAppend(param.Name);
        }

        return StringBuilderValue();
    }


    public static string GetParamTypesStr(ParameterInfo[] parameters, Type returnType = null)
    {
        ClearStringBuilder();

        var first = true;
        foreach (var param in parameters)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                StringBuilderAppend(", ");
            }

            StringBuilderAppend(param.ParameterType.FullName);
        }

        if (returnType != null && returnType != typeof(void))
        {
            if (!first)
            {
                StringBuilderAppend(", ");
            }
            StringBuilderAppend(returnType.FullName);
        }

        return StringBuilderValue();
    }

    public static string GetGenericParamTypesStr(Type[] parameters)
    {
        ClearStringBuilder();

        var first = true;
        foreach (var param in parameters)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                StringBuilderAppend(", ");
            }

            StringBuilderAppend(param.FullName);
        }

        return StringBuilderValue();
    }
    #endregion
}
