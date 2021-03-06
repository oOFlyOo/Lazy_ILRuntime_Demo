﻿
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

namespace ILRuntime.Binding.Redirect
{
    unsafe internal static class UnityEngine_GameObject_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            var flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(UnityEngine.GameObject);

            args = new Type[] { typeof(System.Type) };
            method = type.GetMethod("AddComponent", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, AddComponent_Type);

            args = new Type[] { typeof(System.Type) };
            method = type.GetMethod("GetComponent", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, GetComponent_Type);

            args = new Type[] { typeof(System.Type) };
            method = type.GetMethod("GetComponentInChildren", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, GetComponentInChildren_Type);

            args = new Type[] { typeof(System.Type), typeof(System.Boolean) };
            method = type.GetMethod("GetComponentInChildren", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, GetComponentInChildren_Type_Boolean);

            args = new Type[] { typeof(System.Type) };
            method = type.GetMethod("GetComponentInParent", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, GetComponentInParent_Type);

            args = new Type[] { typeof(System.Type) };
            method = type.GetMethod("GetComponents", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, GetComponents_Type);

            args = new Type[] { typeof(System.Type) };
            method = type.GetMethod("GetComponentsInChildren", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, GetComponentsInChildren_Type);

            args = new Type[] { typeof(System.Type), typeof(System.Boolean) };
            method = type.GetMethod("GetComponentsInChildren", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, GetComponentsInChildren_Type_Boolean);

            args = new Type[] { typeof(System.Type) };
            method = type.GetMethod("GetComponentsInParent", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, GetComponentsInParent_Type);

            foreach (var methodInfo in type.GetMethods(flag))
            {
                if (methodInfo.IsGenericMethodDefinition && methodInfo.Name == "AddComponent" &&
                    methodInfo.GetParameters().Length == 0 && methodInfo.GetGenericArguments().Length == 1)
                {
                    domain.RegisterCLRMethodRedirection(methodInfo, AddComponent_T);
                    break;
                }
            }

            foreach (var methodInfo in type.GetMethods(flag))
            {
                if (methodInfo.IsGenericMethodDefinition && methodInfo.Name == "GetComponent" &&
                    methodInfo.GetParameters().Length == 0 && methodInfo.GetGenericArguments().Length == 1)
                {
                    domain.RegisterCLRMethodRedirection(methodInfo, GetComponent_T);
                    break;
                }
            }

            foreach (var methodInfo in type.GetMethods(flag))
            {
                if (methodInfo.IsGenericMethodDefinition && methodInfo.Name == "GetComponentInChildren" &&
                    methodInfo.GetParameters().Length == 0 && methodInfo.GetGenericArguments().Length == 1)
                {
                    domain.RegisterCLRMethodRedirection(methodInfo, GetComponentInChildren_T);
                    break;
                }
            }

            foreach (var methodInfo in type.GetMethods(flag))
            {
                if (methodInfo.IsGenericMethodDefinition && methodInfo.Name == "GetComponentInChildren" &&
                    methodInfo.GetParameters().Length == 1 && methodInfo.GetGenericArguments().Length == 1)
                {
                    domain.RegisterCLRMethodRedirection(methodInfo, GetComponentInChildren_T_Boolean);
                    break;
                }
            }

            foreach (var methodInfo in type.GetMethods(flag))
            {
                if (methodInfo.IsGenericMethodDefinition && methodInfo.Name == "GetComponentInParent" &&
                    methodInfo.GetParameters().Length == 0 && methodInfo.GetGenericArguments().Length == 1)
                {
                    domain.RegisterCLRMethodRedirection(methodInfo, GetComponentInParent_T);
                    break;
                }
            }

            foreach (var methodInfo in type.GetMethods(flag))
            {
                if (methodInfo.IsGenericMethodDefinition && methodInfo.Name == "GetComponents" &&
                    methodInfo.GetParameters().Length == 0 && methodInfo.GetGenericArguments().Length == 1)
                {
                    domain.RegisterCLRMethodRedirection(methodInfo, GetComponents_T);
                    break;
                }
            }

            foreach (var methodInfo in type.GetMethods(flag))
            {
                if (methodInfo.IsGenericMethodDefinition && methodInfo.Name == "GetComponentsInChildren" &&
                    methodInfo.GetParameters().Length == 0 && methodInfo.GetGenericArguments().Length == 1)
                {
                    domain.RegisterCLRMethodRedirection(methodInfo, GetComponentsInChildren_T);
                    break;
                }
            }

            foreach (var methodInfo in type.GetMethods(flag))
            {
                if (methodInfo.IsGenericMethodDefinition && methodInfo.Name == "GetComponentsInChildren" &&
                    methodInfo.GetParameters().Length == 1 && methodInfo.GetGenericArguments().Length == 1)
                {
                    domain.RegisterCLRMethodRedirection(methodInfo, GetComponentsInChildren_T_Boolean);
                    break;
                }
            }

            foreach (var methodInfo in type.GetMethods(flag))
            {
                if (methodInfo.IsGenericMethodDefinition && methodInfo.Name == "GetComponentsInParent" &&
                    methodInfo.GetParameters().Length == 0 && methodInfo.GetGenericArguments().Length == 1)
                {
                    domain.RegisterCLRMethodRedirection(methodInfo, GetComponentsInParent_T);
                    break;
                }
            }

            foreach (var methodInfo in type.GetMethods(flag))
            {
                if (methodInfo.IsGenericMethodDefinition && methodInfo.Name == "GetComponentsInParent" &&
                    methodInfo.GetParameters().Length == 1 && methodInfo.GetGenericArguments().Length == 1)
                {
                    domain.RegisterCLRMethodRedirection(methodInfo, GetComponentsInParent_T_Boolean);
                    break;
                }
            }
        }

        private static StackObject* AddComponent_Type(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var componentType = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.AddComponent(instance_of_this_method, componentType, __domain);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponent_Type(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var type = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponent(instance_of_this_method, type);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponentInChildren_Type(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var type = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponentInChildren(instance_of_this_method, type);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponentInChildren_Type_Boolean(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var includeInactive = ptr_of_this_method->Value == 1;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            var type = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponentInChildren(instance_of_this_method, type, includeInactive);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponentInParent_Type(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var type = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponentInParent(instance_of_this_method, type);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponents_Type(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var type = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponents(instance_of_this_method, type);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponentsInChildren_Type(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var type = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponentsInChildren(instance_of_this_method, type);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponentsInChildren_Type_Boolean(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var includeInactive = ptr_of_this_method->Value == 1;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            var type = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponentsInChildren(instance_of_this_method, type, includeInactive);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponentsInParent_Type(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var type = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponentsInParent(instance_of_this_method, type);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* AddComponent_T(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.AddComponent(instance_of_this_method, __method.GenericArguments[0], __domain);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponent_T(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponent(instance_of_this_method, __method.GenericArguments[0]);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponentInChildren_T(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponentInChildren(instance_of_this_method, __method.GenericArguments[0]);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponentInChildren_T_Boolean(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var includeInactive = ptr_of_this_method->Value == 1;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponentInChildren(instance_of_this_method, __method.GenericArguments[0], includeInactive);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponentInParent_T(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponentInParent(instance_of_this_method, __method.GenericArguments[0]);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponents_T(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponents(instance_of_this_method, __method.GenericArguments[0]);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponentsInChildren_T(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponentsInChildren(instance_of_this_method, __method.GenericArguments[0]);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponentsInChildren_T_Boolean(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var includeInactive = ptr_of_this_method->Value == 1;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponentsInChildren(instance_of_this_method, __method.GenericArguments[0], includeInactive);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponentsInParent_T(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponentsInParent(instance_of_this_method, __method.GenericArguments[0]);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        private static StackObject* GetComponentsInParent_T_Boolean(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var includeInactive = ptr_of_this_method->Value == 1;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.GameObject instance_of_this_method;
            instance_of_this_method = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = ILRMonoAdaptorHelper.GetComponentsInParent(instance_of_this_method.gameObject, __method.GenericArguments[0], includeInactive);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }
    }
}
