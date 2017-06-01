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

public class CoroutineDemo : MonoBehaviour
{
    static CoroutineDemo instance;
    public static CoroutineDemo Instance
    {
        get { return instance; }
    }
    //AppDomain是ILRuntime的入口，最好是在一个单例类中保存，整个游戏全局就一个，这里为了示例方便，每个例子里面都单独做了一个
    //大家在正式项目中请全局只创建一个AppDomain
    AppDomain appdomain;

    void Start()
    {
        instance = this;
        StartCoroutine(LoadHotFixAssembly());
    }

    IEnumerator LoadHotFixAssembly()
    {
        ILRuntimeManager.Create();
        appdomain = ILRuntimeManager.Instance.Domain;

        yield return null;

//        InitializeILRuntime();
        OnHotFixLoaded();
    }

    void InitializeILRuntime()
    {
        //这里做一些ILRuntime的注册
        //使用Couroutine时，C#编译器会自动生成一个实现了IEnumerator，IEnumerator<object>，IDisposable接口的类，因为这是跨域继承，所以需要写CrossBindAdapter（详细请看04_Inheritance教程），Demo已经直接写好，直接注册即可
        appdomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
    }

    unsafe void OnHotFixLoaded()
    {
        appdomain.Invoke("HotFix_Project.TestCoroutine", "RunTest", null, null);
    }

    public void DoCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}
