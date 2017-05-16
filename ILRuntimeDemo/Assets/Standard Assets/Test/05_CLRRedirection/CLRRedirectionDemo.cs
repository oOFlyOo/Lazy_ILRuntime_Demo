using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Enviorment;

public class CLRRedirectionDemo : MonoBehaviour
{
    //AppDomain是ILRuntime的入口，最好是在一个单例类中保存，整个游戏全局就一个，这里为了示例方便，每个例子里面都单独做了一个
    //大家在正式项目中请全局只创建一个AppDomain
    AppDomain appdomain;

    void Start()
    {
        StartCoroutine(LoadHotFixAssembly());
    }

    IEnumerator LoadHotFixAssembly()
    {
        ILRuntimeManager.Create();
        appdomain = ILRuntimeManager.Instance.Domain;

        yield return null;

        InitializeILRuntime();
        OnHotFixLoaded();
    }

    void InitializeILRuntime()
    {
        //这里做一些ILRuntime的注册，这里应该写CLR重定向的注册，为了演示方便，这个例子写在OnHotFixLoaded了
    }

    //这个仅为演示demo用，平时不要这么调用
    void RetryCLRRedirection()
    {
        var type = appdomain.GetType(typeof(Debug));
        CLRMethod method = type.GetMethod("Log", 1) as CLRMethod;
        method.RetryCLRRedirection();
    }

    unsafe void OnHotFixLoaded()
    {
        Debug.Log("什么时候需要CLR重定向呢，当我们需要挟持原方法实现，添加一些热更DLL中的特殊处理的时候，就需要CLR重定向了");
        Debug.Log("详细文档请参见Github主页的相关文档");
        Debug.Log("CLR重定向对ILRuntime底层实现密切相关，因此要完全理解这个Demo，需要大家先看关于ILRuntime实现原理的Demo");

        Debug.Log("下面介绍一个CLR重定向的典型用法，比如我们在DLL里调用Debug.Log，默认情况下是无法显示DLL内堆栈的，像下面这样");
        appdomain.Invoke("HotFix_Project.TestCLRRedirection", "RunTest", null, null);

        Debug.Log("接下来进行CLR重定向注册");

        var mi = typeof(Debug).GetMethod("Log", new System.Type[] { typeof(object) });
        appdomain.RegisterCLRMethodRedirection(mi, Log_11);
        //这个只是为了演示加的，平时不要这么用，直接在InitializeILRuntime方法里面写CLR重定向注册就行了
        RetryCLRRedirection();
        Debug.Log("我们再来调用一次刚刚的方法，注意看下一行日志的变化");
        appdomain.Invoke("HotFix_Project.TestCLRRedirection", "RunTest", null, null);
    }

    //编写重定向方法对于刚接触ILRuntime的朋友可能比较困难，比较简单的方式是通过CLR绑定生成绑定代码，然后在这个基础上改，比如下面这个代码是从UnityEngine_Debug_Binding里面复制来改的
    //如何使用CLR绑定请看相关教程和文档
    unsafe static StackObject* Log_11(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        //ILRuntime的调用约定为被调用者清理堆栈，因此执行这个函数后需要将参数从堆栈清理干净，并把返回值放在栈顶，具体请看ILRuntime实现原理文档
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
        StackObject* ptr_of_this_method;
        //这个是最后方法返回后esp栈指针的值，应该返回清理完参数并指向返回值，这里是只需要返回清理完参数的值即可
        StackObject* __ret = ILIntepreter.Minus(__esp, 1);
        //取Log方法的参数，如果有两个参数的话，第一个参数是esp - 2,第二个参数是esp -1, 因为Mono的bug，直接-2值会错误，所以要调用ILIntepreter.Minus
        ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

        //这里是将栈指针上的值转换成object，如果是基础类型可直接通过ptr->Value和ptr->ValueLow访问到值，具体请看ILRuntime实现原理文档
        object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
        //所有非基础类型都得调用Free来释放托管堆栈
        __intp.Free(ptr_of_this_method);

        //在真实调用Debug.Log前，我们先获取DLL内的堆栈
        var stacktrace = __domain.DebugService.GetStackTrance(__intp);

        //我们在输出信息后面加上DLL堆栈
        UnityEngine.Debug.Log(message + "\n" + stacktrace);

        return __ret;
    }

    void Update()
    {

    }
}
