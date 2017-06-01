
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
                paramSB.Append(ILRuntimeBindingHelper.GetParamTypesStr(invokeMethod.MethodInfo.GetParameters(),
                    invokeMethod.ReturnType.TypeForCLR));
                if (clrType.IsGenericInstance)
                {
                    var genericFirst = true;
                    genericParamSB.Append("<");
                    genericParamSB.Append(
                        ILRuntimeBindingHelper.GetGenericParamTypesStr(clrType.TypeForCLR.GetGenericArguments()));
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
", name, returnVoid ? "" : "return ", returnVoid ? "Action" : "Func", paramSB, ILRuntimeBindingHelper.GetParamsStr(invokeMethod.MethodInfo.GetParameters()), genericParamSB));
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
            sb.Append(ILRuntimeBindingHelper.GetParamTypesStr(clrMethod.MethodInfo.GetParameters(), clrMethod.ReturnType.TypeForCLR));
            sb.Append(@">();");
        }
    }

    private static string GetBindingCodePath(string fileName)
    {
        return string.Format("{0}/{1}.cs", ILRuntimePaths.BindingCodePath, fileName);
    }
}
