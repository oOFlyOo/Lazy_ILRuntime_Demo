
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
using ILRuntime.Runtime.Adaptor;

namespace ILRuntime.Binding.Redirect
{
    unsafe internal static class UnityEngine_MonoBehaviour_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            var flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(UnityEngine.MonoBehaviour);

            args = new Type[] { };
            method = type.GetConstructor(flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, ctor);

            args = new Type[] { };
            method = type.GetMethod("IsInvoking", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, IsInvoking);

            args = new Type[] { typeof(System.String) };
            method = type.GetMethod("IsInvoking", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, IsInvoking_String);

            args = new Type[] { };
            method = type.GetMethod("CancelInvoke", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, CancelInvoke);

            args = new Type[] { typeof(System.String) };
            method = type.GetMethod("CancelInvoke", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, CancelInvoke_String);

            args = new Type[] { typeof(System.String), typeof(System.Single) };
            method = type.GetMethod("Invoke", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, Invoke_String_Single);

            args = new Type[] { typeof(System.String), typeof(System.Single), typeof(System.Single) };
            method = type.GetMethod("InvokeRepeating", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, InvokeRepeating_String_Single_Single);
        }

        private static StackObject* ctor(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
//            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
//            StackObject* ptr_of_this_method;
//            StackObject* __ret = ILIntepreter.Minus(__esp, 0);
//            var result_of_this_method = new UnityEngine.MonoBehaviour();
//            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);

            // 直接无视
            return __esp;
        }

        public static StackObject* IsInvoking(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.MonoBehaviour instance_of_this_method;
            instance_of_this_method = (UnityEngine.MonoBehaviour)typeof(UnityEngine.MonoBehaviour).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            bool result_of_this_method;
            if (instance_of_this_method.IsMonoAdaptor())
            {
                result_of_this_method = MonoTimeManager.Instance.IsInvoking((MonoBehaviourAdapter.MonoAdaptor)instance_of_this_method);
            }
            else
            {
                result_of_this_method = instance_of_this_method.IsInvoking();
            }

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        private static StackObject* IsInvoking_String(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.MonoBehaviour instance_of_this_method;
            instance_of_this_method = (UnityEngine.MonoBehaviour)typeof(UnityEngine.MonoBehaviour).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            bool result_of_this_method;
            if (instance_of_this_method.IsMonoAdaptor())
            {
                result_of_this_method = MonoTimeManager.Instance.IsInvoking((MonoBehaviourAdapter.MonoAdaptor)instance_of_this_method, methodName);
            }
            else
            {
                result_of_this_method = instance_of_this_method.IsInvoking(methodName);
            }

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;

            return __ret + 1;
        }

        private static StackObject* CancelInvoke(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.MonoBehaviour instance_of_this_method;
            instance_of_this_method = (UnityEngine.MonoBehaviour)typeof(UnityEngine.MonoBehaviour).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            if (instance_of_this_method.IsMonoAdaptor())
            {
                MonoTimeManager.Instance.CancelInvoke((MonoBehaviourAdapter.MonoAdaptor)instance_of_this_method);
            }
            else
            {
                instance_of_this_method.CancelInvoke();
            }

            return __ret;
        }

        private static StackObject* CancelInvoke_String(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.MonoBehaviour instance_of_this_method;
            instance_of_this_method = (UnityEngine.MonoBehaviour)typeof(UnityEngine.MonoBehaviour).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            if (instance_of_this_method.IsMonoAdaptor())
            {
                MonoTimeManager.Instance.CancelInvoke((MonoBehaviourAdapter.MonoAdaptor)instance_of_this_method, methodName);
            }
            else
            {
                instance_of_this_method.CancelInvoke(methodName);
            }

            return __ret;
        }

        private static StackObject* Invoke_String_Single(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var time = *(float*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            var methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            UnityEngine.MonoBehaviour instance_of_this_method;
            instance_of_this_method = (UnityEngine.MonoBehaviour)typeof(UnityEngine.MonoBehaviour).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            if (instance_of_this_method.IsMonoAdaptor())
            {
                MonoTimeManager.Instance.Invoke((MonoBehaviourAdapter.MonoAdaptor)instance_of_this_method, methodName, time);
            }
            else
            {
                instance_of_this_method.Invoke(methodName, time);
            }

            return __ret;
        }

        private static StackObject* InvokeRepeating_String_Single_Single(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var repeatRate = *(float*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            var time = *(float*)&ptr_of_this_method->Value;
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            var methodName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            UnityEngine.MonoBehaviour instance_of_this_method;
            instance_of_this_method = (UnityEngine.MonoBehaviour)typeof(UnityEngine.MonoBehaviour).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            if (instance_of_this_method.IsMonoAdaptor())
            {
                MonoTimeManager.Instance.InvokeRepeating((MonoBehaviourAdapter.MonoAdaptor)instance_of_this_method, methodName, time, repeatRate);
            }
            else
            {
                instance_of_this_method.InvokeRepeating(methodName, time, repeatRate);
            }

            return __ret;
        }
    }
}
