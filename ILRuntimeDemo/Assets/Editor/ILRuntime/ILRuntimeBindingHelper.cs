
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime;
using Mono.Cecil;
using UnityEditor;
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

    public static Dictionary<CLRType, List<MemberReference>> GetAllMonoCecilReference(AppDomain domain,
        ModuleDefinition moduleDef)
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
                                var ilMethod = new ILMethod(methodDefinition, GetIType(domain, typeDefinition) as ILType,
                                    domain);
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

    private static IType GetIType(AppDomain domain, TypeReference typeRef, IType contextType = null,
        IMethod contextMethod = null)
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
        var str = _stringBuilder.ToString();
        ClearStringBuilder();
        return str;
    }

    public static string GetParamsCode(ParameterInfo[] parameters)
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


    public static string GetParamTypesCode(ParameterInfo[] parameters, Type returnType = null)
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

            StringBuilderAppend(param.ParameterType.GetRealClassName());
        }

        if (returnType != null && returnType != typeof (void))
        {
            if (!first)
            {
                StringBuilderAppend(", ");
            }
            StringBuilderAppend(returnType.GetRealClassName());
        }

        return StringBuilderValue();
    }

    public static string GetParamTypeTypesCode(ParameterInfo[] parameters, Type returnType = null)
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

            StringBuilderAppend(string.Format("typeof({0}){1}", param.ParameterType.GetRealClassName(), param.ParameterType.IsByRef ? ".MakeByRefType()" : ""));
        }

        if (returnType != null && returnType != typeof (void))
        {
            if (!first)
            {
                StringBuilderAppend(", ");
            }
            StringBuilderAppend(string.Format("typeof({0})", returnType.GetRealClassName()));
        }

        return StringBuilderValue();
    }

    public static string GetGenericParamTypesCode(Type[] parameters)
    {
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

            StringBuilderAppend(param.GetRealClassName());
        }

        return StringBuilderValue();
    }

    public static string GetRealClassName(this Type type)
    {
        string clsName, realClsName;
        bool temp;
        type.GetClassName(out clsName, out realClsName, out temp);

        return realClsName;
    }

    public static bool IsProperty(this MethodInfo method)
    {
        if (method.IsSpecialName)
        {
            var t = method.Name.Split('_');
            if (t[0] == "set" || t[0] == "get")
            {
                return method.DeclaringType.GetProperty(t[1]) != null;
            }
        }

        return false;
    }

    public static string GetMethodName(CLRMethod method)
    {
        StringBuilderAppend(method.Name);
        foreach (var parameter in method.Parameters)
        {
            StringBuilderAppend("_");
            StringBuilderAppend(parameter.TypeForCLR.Name);
        }

        if (method.IsConstructor)
        {
            return StringBuilderValue().TrimStart('.');
        }
        else
        {
            return StringBuilderValue();
        }
    }

    public static string GetConstructorGenerateCode(CLRMethod method)
    {
        var registerCode = string.Format(@"
            args = new Type[]{{{0}}};
            method = type.GetConstructor(flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, {1});
", GetParamTypeTypesCode(method.ConstructorInfo.GetParameters()), GetMethodName(method));

        return registerCode;
    }

    public static string GetMethodGenerateCode(CLRMethod method)
    {
        var registerCode = string.Format(@"
            args = new Type[]{{{0}}};
            method = type.GetMethod(""{2}"", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, {1});
", GetParamTypeTypesCode(method.MethodInfo.GetParameters()), GetMethodName(method), method.Name);

        return registerCode;
    }

    public static void GetInstanceCode(Type type, StringBuilder sb)
    {
        if (!type.IsValueType || !type.IsPrimitive)
        {
            return;
        }

        sb.Append(string.Format(@"
        public static {0} GetInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, List<object> __mStack)
        {{
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            {0} instance_of_this_method;
            switch(ptr_of_this_method->ObjectType)
            {{
                case ObjectTypes.FieldReference:
                    {{
                        var instance_of_fieldReference = __mStack[ptr_of_this_method->Value];
                        if(instance_of_fieldReference is ILTypeInstance)
                        {{
                            instance_of_this_method = ({0})((ILTypeInstance)instance_of_fieldReference)[ptr_of_this_method->ValueLow];
                        }}
                        else
                        {{
                            var t = __domain.GetType(instance_of_fieldReference.GetType()) as CLRType;
                            instance_of_this_method = ({0})t.GetField(ptr_of_this_method->ValueLow).GetValue(instance_of_fieldReference);
                        }}
                        break;
                    }}
                case ObjectTypes.StaticFieldReference:
                    {{
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {{
                            instance_of_this_method = ({0})((ILType)t).StaticInstance[ptr_of_this_method->ValueLow];
                        }}
                        else
                        {{
                            instance_of_this_method = ({0})((CLRType)t).GetField(ptr_of_this_method->ValueLow).GetValue(null);
                        }}
                        break;
                    }}
                case ObjectTypes.ArrayReference:
                    {{
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as {0}[];
                        instance_of_this_method = instance_of_arrayReference[ptr_of_this_method->ValueLow];                        
                        break;
                    }}
                default:
                    {{
                        instance_of_this_method = {1};
                    break;
                    }}
            }}
            return instance_of_this_method;
        }}
", type.GetRealClassName(), GetRetrieveValueCode(type)));
    }

    public static void GetWriteBackInstanceCode(Type type, StringBuilder sb)
    {
        if (!type.IsValueType || type.IsPrimitive || type.IsAbstract)
        {
            return;
        }

        sb.AppendLine(string.Format(@"
        private static void WriteBackInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, List<object> __mStack, ref {0} instance_of_this_method)
        {{
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            switch(ptr_of_this_method->ObjectType)
            {{
                case ObjectTypes.Object:
                    {{
                        __mStack[ptr_of_this_method->Value] = instance_of_this_method;
                        break;
                    }}
                case ObjectTypes.FieldReference:
                    {{
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {{
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }}
                        else
                        {{
                            var t = __domain.GetType(___obj.GetType()) as CLRType;
                            t.GetField(ptr_of_this_method->ValueLow).SetValue(___obj, instance_of_this_method);
                        }}
                        break;
                    }}
                case ObjectTypes.StaticFieldReference:
                    {{
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {{
                            ((ILType)t).StaticInstance[ptr_of_this_method->ValueLow] = instance_of_this_method);
                        }}
                        else
                        {{
                            ((CLRType)t).GetField(ptr_of_this_method->ValueLow).SetValue(null, instance_of_this_method);
                        }}
                        break;
                    }}
                 case ObjectTypes.ArrayReference:
                    {{
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as {0}[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        break;
                    }}
            }}
        }}
", type.GetRealClassName()));
    }

    public static void GetMethodHeader(string methodName, int paramCount, StringBuilder sb)
    {
        sb.Append(string.Format(@"
        private static StackObject* {0}(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {{
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, {1});
", methodName, paramCount));
    }

    public static void GetMethodParameters(ParameterInfo[] parameters, string tabs, StringBuilder sb)
    {
        for (int i = parameters.Length - 1; i >= 0; i--)
        {
            var param = parameters[i];
            sb.AppendLine(string.Format(tabs + @"ptr_of_this_method = ILIntepreter.Minus(__esp, {0});", parameters.Length - i));
            if (param.ParameterType.IsByRef)
            {
                sb.AppendLine(tabs + "ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);");
            }
            sb.AppendLine(string.Format(tabs + "var {0} = {1};", param.Name, ILRuntimeBindingHelper.GetRetrieveValueCode(param.ParameterType)));
            if (!param.ParameterType.IsByRef && !param.ParameterType.IsPrimitive)
            {
                sb.AppendLine(tabs + "__intp.Free(ptr_of_this_method);");
            }
        }
    }

    public static void GetRefOutCode(ParameterInfo[] parameters, StringBuilder sb)
    {
        //Ref/Out
        for (int i = parameters.Length - 1; i >= 0; i--)
        {
            var param = parameters[i];
            if (!param.ParameterType.IsByRef)
            {
                continue;
            }

            sb.Append(string.Format(@"
            ptr_of_this_method = ILIntepreter.Minus(__esp, {0});
            switch(ptr_of_this_method->ObjectType)
            {{
                case ObjectTypes.StackObjectReference:
                    {{
                        var ___dst = *(StackObject**)&ptr_of_this_method->Value;{1};
                        break;
                    }}
                case ObjectTypes.FieldReference:
                    {{
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {{
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = {2};
                        }}
                        else
                        {{
                            var t = __domain.GetType(___obj.GetType()) as CLRType;
                            t.GetField(ptr_of_this_method->ValueLow).SetValue(___obj, {2});
                        }}
                        break;
                    }}
                    case ObjectTypes.StaticFieldReference:
                    {{
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {{
                            ((ILType)t).StaticInstance[ptr_of_this_method->ValueLow] = {2};
                        }}
                        else
                        {{
                            ((CLRType)t).GetField(ptr_of_this_method->ValueLow).SetValue(null, {2});
                        }}
                        break;
                    }}
                    case ObjectTypes.ArrayReference:
                    {{
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as {3}[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = {2};
                        break;
                    }}
            }}
", parameters.Length - i, GetRefWriteBackValueCode(param), param.Name, param.ParameterType.GetElementType().GetRealClassName()));
        }
    }

    public static string GetRefWriteBackValueCode(ParameterInfo param)
    {
        var type = param.ParameterType.GetElementType();
        var paramName = param.Name;
        var sb = new StringBuilder();

        if (type.IsPrimitive)
        {
            if (type == typeof(int))
            {
                sb.Append(string.Format(@"
                        ___dst->ObjectType = ObjectTypes.Integer;
                        ___dst->Value = {0};
", paramName));
            }
            else if (type == typeof(long))
            {
                sb.Append(string.Format(@"
                        ___dst->ObjectType = ObjectTypes.Long;
                        *(long*)&___dst->Value = {0};
", paramName));
            }
            else if (type == typeof(short))
            {
                sb.Append(string.Format(@"
                        ___dst->ObjectType = ObjectTypes.Integer;
                        ___dst->Value = {0};
", paramName));
            }
            else if (type == typeof(bool))
            {
                sb.Append(string.Format(@"
                        ___dst->ObjectType = ObjectTypes.Integer;
                        ___dst->Value = {0} ? 1 : 0;
", paramName));
            }
            else if (type == typeof(ushort))
            {
                sb.Append(string.Format(@"
                        ___dst->ObjectType = ObjectTypes.Integer;
                        ___dst->Value = {0};
", paramName));
                sb.AppendLine("                        ___dst->ObjectType = ObjectTypes.Integer;");
                sb.Append("                        ___dst->Value = " + paramName);
                sb.AppendLine(";");
            }
            else if (type == typeof(float))
            {
                sb.Append(string.Format(@"
                        ___dst->ObjectType = ObjectTypes.Float;
                        *(float*)&___dst->Value = {0};
", paramName));
            }
            else if (type == typeof(double))
            {
                sb.Append(string.Format(@"
                        ___dst->ObjectType = ObjectTypes.Double;
                        *(double*)&___dst->Value =  = {0};
", paramName));
            }
            else if (type == typeof(byte))
            {
                sb.Append(string.Format(@"
                        ___dst->ObjectType = ObjectTypes.Integer;
                        ___dst->Value = {0};
", paramName));
            }
            else if (type == typeof(sbyte))
            {
                sb.Append(string.Format(@"
                        ___dst->ObjectType = ObjectTypes.Integer;
                        ___dst->Value = {0};
", paramName));
            }
            else if (type == typeof(uint))
            {
                sb.Append(string.Format(@"
                        ___dst->ObjectType = ObjectTypes.Integer;
                        ___dst->Value = {0};
", paramName));
            }
            else if (type == typeof(char))
            {
                sb.Append(string.Format(@"
                        ___dst->ObjectType = ObjectTypes.Integer;
                        ___dst->Value = {0};
", paramName));
            }
            else if (type == typeof(ulong))
            {
                sb.Append(string.Format(@"
                        ___dst->ObjectType = ObjectTypes.Long;
                        *(ulong*)&___dst->Value = {0};
", paramName));
            }
            else
                throw new NotImplementedException();
        }
        else
        {
            if (!type.IsValueType)
            {
                sb.Append(string.Format(@"
                        object ___obj = {0};
                        if (___obj is CrossBindingAdaptorType)
                            ___obj = ((CrossBindingAdaptorType)___obj).ILInstance;
                        __mStack[___dst->Value] = ___obj;
", paramName));
            }
            else
            {
                sb.Append(string.Format(@"
                        __mStack[___dst->Value] = {0};
", paramName));
            }
        }

        return sb.ToString();
    }

    public static string GetReturnValueCode(CLRMethod method)
    {
        var type = method.ReturnType.TypeForCLR;
        var sb = new StringBuilder();
        if (type == typeof (void))
        {
            sb.AppendLine("            return __ret;");
        }
        else if (type.IsPrimitive)
        {
            if (type == typeof(int))
            {
                sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                sb.AppendLine("            __ret->Value = result_of_this_method;");
            }
            else if (type == typeof(long))
            {
                sb.AppendLine("            __ret->ObjectType = ObjectTypes.Long;");
                sb.AppendLine("            *(long*)&__ret->Value = result_of_this_method;");
            }
            else if (type == typeof(short))
            {
                sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                sb.AppendLine("            __ret->Value = result_of_this_method;");
            }
            else if (type == typeof(bool))
            {
                sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                sb.AppendLine("            __ret->Value = result_of_this_method ? 1 : 0;");
            }
            else if (type == typeof(ushort))
            {
                sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                sb.AppendLine("            __ret->Value = result_of_this_method;");
            }
            else if (type == typeof(float))
            {
                sb.AppendLine("            __ret->ObjectType = ObjectTypes.Float;");
                sb.AppendLine("            *(float*)&__ret->Value = result_of_this_method;");
            }
            else if (type == typeof(double))
            {
                sb.AppendLine("            __ret->ObjectType = ObjectTypes.Double;");
                sb.AppendLine("            *(double*)&__ret->Value = result_of_this_method;");
            }
            else if (type == typeof(byte))
            {
                sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                sb.AppendLine("            __ret->Value = result_of_this_method;");
            }
            else if (type == typeof(sbyte))
            {
                sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                sb.AppendLine("            __ret->Value = result_of_this_method;");
            }
            else if (type == typeof(uint))
            {
                sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                sb.AppendLine("            __ret->Value = (int)result_of_this_method;");
            }
            else if (type == typeof(char))
            {
                sb.AppendLine("            __ret->ObjectType = ObjectTypes.Integer;");
                sb.AppendLine("            __ret->Value = (int)result_of_this_method;");
            }
            else if (type == typeof(ulong))
            {
                sb.AppendLine("            __ret->ObjectType = ObjectTypes.Long;");
                sb.AppendLine("            *(ulong*)&__ret->Value = result_of_this_method;");
            }
            else
                throw new NotImplementedException();
            sb.Append("            return __ret + 1;");
        }
        else
        {
            if (!method.IsConstructor && !type.IsSealed && type != typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance))
            {
                sb.AppendLine(@"            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }");
            }
            sb.Append("            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);");
        }

        return sb.ToString();
    }

    public static string GetRetrieveValueCode(Type type)
    {
        if (type.IsByRef)
            type = type.GetElementType();
        if (type.IsPrimitive)
        {
            if (type == typeof(int))
            {
                return "ptr_of_this_method->Value";
            }
            else if (type == typeof(long))
            {
                return "*(long*)&ptr_of_this_method->Value";
            }
            else if (type == typeof(short))
            {
                return "(short)ptr_of_this_method->Value";
            }
            else if (type == typeof(bool))
            {
                return "ptr_of_this_method->Value == 1";
            }
            else if (type == typeof(ushort))
            {
                return "(ushort)ptr_of_this_method->Value";
            }
            else if (type == typeof(float))
            {
                return "*(float*)&ptr_of_this_method->Value";
            }
            else if (type == typeof(double))
            {
                return "*(double*)&ptr_of_this_method->Value";
            }
            else if (type == typeof(byte))
            {
                return "(byte)ptr_of_this_method->Value";
            }
            else if (type == typeof(sbyte))
            {
                return "(sbyte)ptr_of_this_method->Value";
            }
            else if (type == typeof(uint))
            {
                return "(uint)ptr_of_this_method->Value";
            }
            else if (type == typeof(char))
            {
                return "(char)ptr_of_this_method->Value";
            }
            else if (type == typeof(ulong))
            {
                return "*(ulong*)&ptr_of_this_method->Value";
            }
            else
                throw new NotImplementedException();
        }
        else
        {
            return string.Format("({0})typeof({0}).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack))", type.GetRealClassName());
        }
    }
    #endregion
}
