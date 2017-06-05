
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime;
using Mono.Cecil;
using UnityEditor.Graphs;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public class ILRuntimeBindingGenerator
{
    public static void Generate()
    {
        var domain = ILRuntimeManager.Create().Domain;

        var moduleDef = ILRuntimeBindingHelper.ReadModule(ILRuntimePaths.AssemblyCSharpPath);
        GenerateFramworkMessage(moduleDef);

        var refDict = ILRuntimeBindingHelper.GetAllMonoCecilReference(domain, moduleDef);
        var typeList = ILRuntimeBindingHelper.GetAllReference(domain, moduleDef, refDict);
        CLRBindingCode(domain, typeList);

        ILRuntimeManager.DestroyInstance();
    }

    private static void GenerateFramworkMessage(ModuleDefinition moduleDef)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        foreach (var reference in moduleDef.AssemblyReferences)
        {
            sb.AppendLine(reference.ToString());
        }

        var domain = System.AppDomain.CreateDomain(typeof(ILRuntimeBindingGenerator).FullName);
        var frameworkAssembly = domain.Load(FileHelper.ReadAllBytes(ILRuntimePaths.FrameworkyCSharpPath));
        var types = frameworkAssembly.GetExportedTypes();
        var typeList = types.ToList();
        typeList.Sort((type, type1) =>
        {
            return type.FullName.CompareTo(type1.FullName);
        });
        foreach (var type in typeList)
        {
            sb.AppendLine();
            sb.AppendLine(type.ToString());
            foreach (var fieldInfo in type.GetFields())
            {
                sb.AppendLine(fieldInfo.ToString());
            }
            foreach (var constructorInfo in type.GetConstructors())
            {
                sb.AppendLine(constructorInfo.ToString());
            }
            foreach (var methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                sb.AppendLine(methodInfo.ToString());
            }
        }

        FileHelper.WriteAllText(ILRuntimePaths.FrameworkMessagePath, sb.ToString());
    }


    private static void CLRBindingCode(AppDomain domain, List<ILRuntimeBindingTypeInfo> typeInfoList)
    {
        ClearOutputPath(ILRuntimePaths.BindingCodePath);

        SortBindingTypeInfoList(typeInfoList);

        typeInfoList = CheckTypeInfoList(typeInfoList);

        GenerateCLRBindingCode(domain, typeInfoList);

        GenerateCLRBindingMessage(typeInfoList);
    }


    private static void ClearOutputPath(string outputPath)
    {
        FileHelper.CreateDirectory(outputPath);
        foreach (var file in Directory.GetFiles(outputPath, "*.cs"))
        {
            File.Delete(file);
        }
    }

    private static void SortBindingTypeInfoList(List<ILRuntimeBindingTypeInfo> typeInfoList)
    {
        typeInfoList.Sort((info, typeInfo) =>
        {
            return info.DeclaringType.FullName.CompareTo(typeInfo.DeclaringType.FullName);
        });

        foreach (var typeInfo in typeInfoList)
        {
            typeInfo.Fields.Sort((info, fieldInfo) =>
            {
                return info.Name.CompareTo(fieldInfo.Name);
            });

            typeInfo.Constructors.Sort((method, clrMethod) =>
            {
                return method.ToString().CompareTo(clrMethod.ToString());
            });

            typeInfo.Methods.Sort((method, clrMethod) =>
            {
                return method.ToString().CompareTo(clrMethod.ToString());
            });
        }
    }

    private static List<ILRuntimeBindingTypeInfo> CheckTypeInfoList(List<ILRuntimeBindingTypeInfo> typeInfoList)
    {
        var newList = new List<ILRuntimeBindingTypeInfo>();
        foreach (var typeInfo in typeInfoList)
        {
            if (!ShouldRemoveTypeInfo(typeInfo))
            {
                newList.Add(typeInfo);
            }
            else
            {
                Debug.Log(string.Format("忽略 Attribute：{0}", typeInfo));
            }
        }
        return newList;
    }

    private static bool ShouldRemoveTypeInfo(ILRuntimeBindingTypeInfo typeInfo)
    {
        var realType = typeInfo.DeclaringType.TypeForCLR;
        if (realType.IsSubclassOf(typeof (Attribute)))
        {
            return true;
        }

        return false;
    }

    private static void GenerateCLRBindingMessage(List<ILRuntimeBindingTypeInfo> typeInfoList)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        foreach (var info in typeInfoList)
        {
            sb.AppendLine(info.ToString());
        }

        FileHelper.WriteAllText(ILRuntimePaths.BindingCodeMessagePath, sb.ToString());
    }

    private static void GenerateCLRBindingCode(AppDomain domain, List<ILRuntimeBindingTypeInfo> typeInfoList)
    {
        var sb = new StringBuilder();

        sb.Append(@"
using ILRuntime.Runtime.Enviorment;

namespace ILRuntime.Binding.Generated
{
    internal class CLRBindings
    {
        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(AppDomain domain)
        {
            RegisterBindingClass(domain);
            RegisterDelegateConvertor(domain.DelegateManager);
            RegisterDelegate(domain.DelegateManager);
        }

        private static void RegisterBindingClass(AppDomain domain)
        {
");
        foreach (var info in typeInfoList)
        {
            var className = GenerateRegisterBindingClass(domain, info);
            if (!string.IsNullOrEmpty(className))
            {
                sb.Append(string.Format(@"
            {0}.Register(domain);", className));
            }
        }
        sb.Append(@"
        }

        private static void RegisterDelegateConvertor(DelegateManager dm)
        {
");
        var delegateList = GetAllCrossDelegateCLRType(domain, typeInfoList);
        GenerateRegisterDelegateConvertor(domain, sb, delegateList);
        sb.Append(@"
        }

        private static void RegisterDelegate(DelegateManager dm)
        {
");
        GenerateRegisterDelegate(domain, sb, delegateList);
        sb.Append(@"
        }
    }
}
");

        File.WriteAllText(GetBindingCodePath("CLRBindings"), sb.ToString());
    }

    #region 绑定类
    private static string GenerateRegisterBindingClass(AppDomain domain, ILRuntimeBindingTypeInfo typeInfo)
    {
        if (ShouldSkipTypeInfo(typeInfo))
        {
            return null;
        }

        string clsName, realClsName;
        bool isByRef;
        typeInfo.DeclaringType.TypeForCLR.GetClassName(out clsName, out realClsName, out isByRef);

        var sb = new StringBuilder();
        var methodSb = new StringBuilder();

        sb.Append(string.Format(@"
using System;
using System.Collections.Generic;
using System.Reflection;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Binding.Generated
{{
    unsafe class {0}
    {{
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain domain)
        {{
            var flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof({1});
", clsName, realClsName));

        GenerateCommonCode(typeInfo.DeclaringType.TypeForCLR, methodSb);

        foreach (var constructor in typeInfo.Constructors)
        {
            sb.Append(GenerateConstructor(constructor, methodSb));
        }

        foreach (var clrMethod in typeInfo.Methods)
        {
            sb.Append(GenerateMethod(clrMethod, methodSb));
        }

        sb.Append(string.Format(@"
        }}
{0}
    }}
}}
", methodSb));

        FileHelper.WriteAllText(GetBindingCodePath(clsName), sb.ToString());

        return clsName;
    }

    private static bool ShouldSkipTypeInfo(ILRuntimeBindingTypeInfo typeInfo)
    {
        if (typeInfo.DeclaringType.IsDelegate)
        {
            return true;
        }

        return false;
    }

    private static void GenerateCommonCode(Type type, StringBuilder sb)
    {
        if (!type.IsValueType)
        {
            return;
        }

        ILRuntimeBindingHelper.GetInstanceCode(type, sb);
        ILRuntimeBindingHelper.GetWriteBackInstanceCode(type, sb);
    }

    private static string GenerateConstructor(CLRMethod method, StringBuilder sb)
    {
        if (ShouldSkipMethod(method))
        {
            return null;
        }

        var methodName = ILRuntimeBindingHelper.GetMethodName(method);

        var parameters = method.ConstructorInfo.GetParameters();

        ILRuntimeBindingHelper.GetMethodHeader(methodName, parameters.Length, sb);

        var tabs = "\t\t\t";
        ILRuntimeBindingHelper.GetMethodParameters(parameters, tabs, sb);

        sb.Append(string.Format(@"
            var result_of_this_method = new {0}({1});
", method.DeclearingType.TypeForCLR.GetRealClassName(), ILRuntimeBindingHelper.GetParamsCode(method.ConstructorInfo.GetParameters())));

        if (method.DeclearingType.TypeForCLR.IsValueType)
        {
            sb.Append(@"
            if(!isNewObj)
            {
                __ret--;
                WriteBackInstance(__domain, __ret, __mStack, ref result_of_this_method);
                return __ret;
            }
");
        }

        ILRuntimeBindingHelper.GetRefOutCode(parameters, sb);

        sb.Append(string.Format(@"{0}
        }}
", ILRuntimeBindingHelper.GetReturnValueCode(method)));

        return ILRuntimeBindingHelper.GetConstructorGenerateCode(method);
    }

    private static string GenerateMethod(CLRMethod method, StringBuilder sb)
    {
        if (ShouldSkipMethod(method))
        {
            return null;
        }

        var type = method.DeclearingType.TypeForCLR;
        var typeClsName = type.GetRealClassName();
        var methodName = ILRuntimeBindingHelper.GetMethodName(method);

        var methodInfo = method.MethodInfo;
        var isProperty = methodInfo.IsProperty();
        var parameters = methodInfo.GetParameters();

        ILRuntimeBindingHelper.GetMethodHeader(methodName, methodInfo.IsStatic ? parameters.Length : parameters.Length + 1, sb);

        var tabs = "\t\t\t";
        ILRuntimeBindingHelper.GetMethodParameters(parameters, tabs, sb);

        if (!methodInfo.IsStatic)
        {
            sb.Append(string.Format(tabs + @"ptr_of_this_method = ILIntepreter.Minus(__esp, {0});", methodInfo.IsStatic ? parameters.Length : parameters.Length + 1));
            if (type.IsPrimitive)
            {
                sb.AppendLine(string.Format(tabs + "{0} instance_of_this_method = GetInstance(__domain, ptr_of_this_method, __mStack);", typeClsName));
            }
            else
            {
                if (type.IsValueType)
                {
                    sb.AppendLine(tabs + "ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);");
                }
                sb.AppendLine(string.Format(tabs + "{0} instance_of_this_method;", typeClsName));
                sb.AppendLine(string.Format(tabs + "instance_of_this_method = {0};", ILRuntimeBindingHelper.GetRetrieveValueCode(type)));
                if (!type.IsValueType)
                {
                    sb.AppendLine(tabs + "__intp.Free(ptr_of_this_method);");
                }
            }
        }

        sb.AppendLine();
        if (methodInfo.ReturnType != typeof(void))
        {
            sb.Append(tabs + "var result_of_this_method = ");
        }
        else
        {
            sb.Append(tabs);
        }

        if (methodInfo.IsStatic)
        {
            if (isProperty)
            {
                var t = method.Name.Split('_');
                string propType = t[0];

                if (propType == "get")
                {
                    bool isIndexer = parameters.Length > 0;
                    if (isIndexer)
                    {
                        sb.AppendLine(string.Format("{1}[{0}];", parameters[0].Name, typeClsName));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("{1}.{0};", t[1], typeClsName));
                    }
                }
                else if (propType == "set")
                {
                    bool isIndexer = parameters.Length > 1;
                    if (isIndexer)
                    {
                        sb.AppendLine(string.Format("{2}[{0}] = {1};", parameters[0].Name, parameters[1].Name, typeClsName));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("{2}.{0} = {1};", t[1], parameters[0].Name, typeClsName));
                    }
                }
                else if (propType == "op")
                {
                    switch (t[1])
                    {
                        case "Equality":
                        {
                            sb.AppendLine(string.Format("{0} == {1};", parameters[0].Name, parameters[1].Name));
                            break;
                        }
                        case "Inequality":
                        {
                            sb.AppendLine(string.Format("{0} != {1};", parameters[0].Name, parameters[1].Name));
                            break;
                        }
                        case "Addition":
                        {
                            sb.AppendLine(string.Format("{0} + {1};", parameters[0].Name, parameters[1].Name));
                            break;
                        }
                        case "Subtraction":
                        {
                            sb.AppendLine(string.Format("{0} - {1};", parameters[0].Name, parameters[1].Name));
                            break;
                        }
                        case "Multiply":
                        {
                            sb.AppendLine(string.Format("{0} * {1};", parameters[0].Name, parameters[1].Name));
                            break;
                        }
                        case "Division":
                        {
                            sb.AppendLine(string.Format("{0} / {1};", parameters[0].Name, parameters[1].Name));
                            break;
                        }
                        case "GreaterThan":
                        {
                            sb.AppendLine(string.Format("{0} > {1};", parameters[0].Name, parameters[1].Name));
                            break;
                        }
                        case "GreaterThanOrEqual":
                        {
                            sb.AppendLine(string.Format("{0} >= {1};", parameters[0].Name, parameters[1].Name));
                            break;
                        }
                        case "LessThan":
                        {
                            sb.AppendLine(string.Format("{0} < {1};", parameters[0].Name, parameters[1].Name));
                            break;
                        }
                        case "LessThanOrEqual":
                        {
                            sb.AppendLine(string.Format("{0} <= {1};", parameters[0].Name, parameters[1].Name));
                            break;
                        }
                        case "UnaryNegation":
                        {
                            sb.AppendLine(string.Format("-{0};", parameters[0].Name));
                            break;
                        }
                        case "Implicit":
                        case "Explicit":
                            {
                                sb.AppendLine(string.Format("({1}){0};", parameters[0].Name, methodInfo.ReturnType.GetRealClassName()));
                                break;
                            }
                        default:
                            throw new NotImplementedException(methodInfo.Name);
                    }
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                sb.Append(string.Format("{0}.{1}(", typeClsName, methodInfo.Name));
                sb.Append(ILRuntimeBindingHelper.GetParamsCode(parameters));
                sb.AppendLine(");");
            }
        }
        else
        {
            if (isProperty)
            {
                string[] t = methodInfo.Name.Split('_');
                string propType = t[0];

                if (propType == "get")
                {
                    bool isIndexer = parameters.Length > 0;
                    if (isIndexer)
                    {
                        sb.AppendLine(string.Format("instance_of_this_method[{0}];", parameters[0].Name));
                    }
                    else
                        sb.AppendLine(string.Format("instance_of_this_method.{0};", t[1]));
                }
                else if (propType == "set")
                {
                    bool isIndexer = parameters.Length > 1;
                    if (isIndexer)
                    {
                        sb.AppendLine(string.Format("instance_of_this_method[{0}] = {1};", parameters[0].Name, parameters[1].Name));
                    }
                    else
                        sb.AppendLine(string.Format("instance_of_this_method.{0} = {1};", t[1], parameters[0].Name));
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                sb.Append(string.Format("instance_of_this_method.{0}(", methodInfo.Name));
                sb.Append(ILRuntimeBindingHelper.GetParamsCode(parameters));
                sb.AppendLine(");");
            }
        }
        sb.AppendLine();


        if (!method.IsStatic && type.IsValueType && !type.IsPrimitive)//need to write back value type instance
        {
            sb.AppendLine(tabs + "WriteBackInstance(__domain, ptr_of_this_method, __mStack, ref instance_of_this_method);");
            sb.AppendLine();
        }
        
        ILRuntimeBindingHelper.GetRefOutCode(parameters, sb);

        sb.Append(string.Format(@"{0}
        }}
", ILRuntimeBindingHelper.GetReturnValueCode(method)));

        return ILRuntimeBindingHelper.GetMethodGenerateCode(method);
    }

    private static bool ShouldSkipMethod(CLRMethod method)
    {
        if (method.Redirection != null)
        {
            return true;
        }

        MethodBase methodInfo = null;
        if (method.IsConstructor)
        {
            methodInfo = method.ConstructorInfo;
        }
        else
        {
            methodInfo = method.MethodInfo;
        }

        if (methodInfo.IsGenericMethod)
        {
            return true;
        }

        if (methodInfo.IsSpecialName)
        {
            var t = methodInfo.Name.Split('_');
            if (t[0] == "add" || t[0] == "remvoe")
            {
                return true;
            }
        }

        foreach (var parameterInfo in methodInfo.GetParameters())
        {
            if (parameterInfo.ParameterType.IsPointer)
            {
                return true;
            }
        }

        return false;
    }
#endregion

    private static List<CLRType> GetAllCrossDelegateCLRType(AppDomain domain, List<ILRuntimeBindingTypeInfo> typeInfoList)
    {
        var list = new List<CLRType>();
        foreach (var typeInfo in typeInfoList)
        {
            if (typeInfo.DeclaringType.IsDelegate)
            {
                continue;
            }

            foreach (var fieldInfo in typeInfo.Fields)
            {
                var type = domain.GetType(fieldInfo.FieldType) as CLRType;
                if (type.IsDelegate)
                {
                    list.AddIfWithout(type);
                }
            }

            foreach (var constructor in typeInfo.Constructors)
            {
                foreach (var parameter in constructor.Parameters)
                {
                    var type = parameter as CLRType;
                    if (type.IsDelegate)
                    {
                        list.AddIfWithout(type);
                    }
                }
            }

            foreach (var clrMethod in typeInfo.Methods)
            {
                foreach (var parameter in clrMethod.Parameters)
                {
                    var type = parameter as CLRType;
                    if (type.IsDelegate)
                    {
                        list.AddIfWithout(type);
                    }
                }
            }
        }
        return list;
    }

    private static void GenerateRegisterDelegateConvertor(AppDomain domain, StringBuilder sb, List<CLRType> delegateList)
    {
        var sortList = delegateList.ToList();
        sortList.Sort((type, clrType) =>
        {
            return type.FullName.CompareTo(clrType.FullName);
        });

        var paramSB = new StringBuilder();
        var genericParamSB = new StringBuilder();
        foreach (var clrType in sortList)
        {
            if (!Regex.IsMatch(clrType.Name, @"^((Action)|(Func))`\d$"))
            {
                var invokeMethod = clrType.GetMethods().FirstOrDefault(method => (method as CLRMethod).IsDelegateInvoke) as CLRMethod;
                var returnVoid = invokeMethod.ReturnType == domain.VoidType;
                var name = clrType.FullName.Split('`')[0];
                paramSB.Length = 0;
                genericParamSB.Length = 0;
                paramSB.Append(ILRuntimeBindingHelper.GetParamTypesCode(invokeMethod.MethodInfo.GetParameters(),
                    invokeMethod.ReturnType.TypeForCLR));
                if (clrType.IsGenericInstance)
                {
                    var genericFirst = true;
                    genericParamSB.Append("<");
                    genericParamSB.Append(
                        ILRuntimeBindingHelper.GetGenericParamTypesCode(clrType.TypeForCLR.GetGenericArguments()));
                    genericParamSB.Append(">");
                }

                sb.Append(string.Format(@"
            dm.RegisterDelegateConvertor<{0}{5}>((action) =>
            {{
                return new {0}{5}(({4}) =>
                {{
                    {1}((System.{2}<{3}>)action)({4});
                }});
            }});
", name, returnVoid ? "" : "return ", returnVoid ? "Action" : "Func", paramSB, ILRuntimeBindingHelper.GetParamsCode(invokeMethod.MethodInfo.GetParameters()), genericParamSB));
            }
        }
    }

    private static void GenerateRegisterDelegate(AppDomain domain, StringBuilder sb, List<CLRType> delegateList)
    {
        var dict = new Dictionary<string, CLRMethod>();
        foreach (var clrType in delegateList)
        {
            var invokeMethod = clrType.GetMethods().FirstOrDefault(method => (method as CLRMethod).IsDelegateInvoke) as CLRMethod;
            dict[invokeMethod.ToString()] = invokeMethod;
        }
        var list = dict.Values.ToList();
        list.Sort((method, clrMethod) =>
        {
            return method.ToString().CompareTo(clrMethod.ToString());
        });
        foreach (var clrMethod in list)
        {
            var returnVoid = clrMethod.ReturnType == domain.VoidType;
            sb.Append(returnVoid
                ? @"
            dm.RegisterMethodDelegate<"
                : @"
            dm.RegisterFunctionDelegate<");
            sb.Append(ILRuntimeBindingHelper.GetParamTypesCode(clrMethod.MethodInfo.GetParameters(), clrMethod.ReturnType.TypeForCLR));
            sb.Append(@">();");
        }
    }

    private static string GetBindingCodePath(string fileName)
    {
        return string.Format("{0}/{1}.cs", ILRuntimePaths.BindingCodePath, fileName);
    }
}
