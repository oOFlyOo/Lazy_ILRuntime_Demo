
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
    unsafe internal class UnityEngine_Debug_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            var flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(UnityEngine.Debug);

            args = new Type[] { typeof(System.Object) };
            method = type.GetMethod("Log", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, Log_Object);

            args = new Type[] { typeof(System.Object) };
            method = type.GetMethod("LogError", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, LogError_Object);

            args = new Type[] { typeof(System.Object) };
            method = type.GetMethod("LogWarning", flag, null, args, null);
            domain.RegisterCLRMethodRedirection(method, LogWarning_Object);
        }

        private static StackObject* Log_Object(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var message = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            //在真实调用Debug.Log前，我们先获取DLL内的堆栈
            var stacktrace = __domain.DebugService.GetStackTrance(__intp);

            UnityEngine.Debug.Log(string.Format("{0}\n{1}", message, stacktrace));

            return __ret;
        }

        private static StackObject* LogError_Object(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var message = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            //在真实调用Debug.Log前，我们先获取DLL内的堆栈
            var stacktrace = __domain.DebugService.GetStackTrance(__intp);

            UnityEngine.Debug.LogError(string.Format("{0}\n{1}", message, stacktrace));

            return __ret;
        }

        private static StackObject* LogWarning_Object(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            var message = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            //在真实调用Debug.Log前，我们先获取DLL内的堆栈
            var stacktrace = __domain.DebugService.GetStackTrance(__intp);

            UnityEngine.Debug.LogWarning(string.Format("{0}\n{1}", message, stacktrace));

            return __ret;
        }
    }
}
