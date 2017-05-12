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

public class CLRBindingTestClass
{
    public static float DoSomeTest(int a, float b)
    {
        return a + b;
    }
}

public class CLRBindingDemo : MonoBehaviour
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
        //这里做一些ILRuntime的注册，这里应该写CLR绑定的注册，为了演示方便，这个例子写在OnHotFixLoaded了
    }

    //这个仅为演示demo用，平时不要这么调用
    void RetryCLRRedirection()
    {
        var type = appdomain.GetType(typeof(CLRBindingTestClass));
        CLRMethod method = type.GetMethod("DoSomeTest", 2) as CLRMethod;
        method.RetryCLRRedirection();
    }

    unsafe void OnHotFixLoaded()
    {
        ilruntimeReady = true;
    }

    bool ilruntimeReady = false;
    bool executed = false;
    void Update()
    {
        if (ilruntimeReady && !executed && Time.realtimeSinceStartup > 3)
        {
            executed = true;
            //这里为了方便看Profiler，代码挪到Update中了
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            Debug.LogWarning("运行这个Demo前请先点击菜单ILRuntime->Generate来生成所需的绑定代码，并按照提示解除下面相关代码的注释");
            Debug.Log("默认情况下，从热更DLL里调用Unity主工程的方法，是通过反射的方式调用的，这个过程中会产生GC Alloc，并且执行效率会偏低");
            Debug.Log("比如下面这个测试方法，请打开Profiler查看该方法耗时和GCAlloc，请确认Deep Profile 没有开启");

            sw.Start();
            Profiler.BeginSample("RunTest");
            appdomain.Invoke("HotFix_Project.TestCLRBinding", "RunTest", null, null);
            Profiler.EndSample();
            RunTest();
            sw.Stop();
            Debug.LogFormat("刚刚的方法执行了:{0} ms", sw.ElapsedMilliseconds);

            Debug.Log("接下来进行CLR绑定注册，在进行注册前，需要先在ILRuntimeCodeGenerator的绑定列表里面，添加上CLRBindingTestClass这个测试类型");
            Debug.Log("CLR绑定会生成较多C#代码，最终会增大包体和Native Code的内存耗用，所以只添加常用类型和频繁调用的接口即可");
            Debug.Log("接下来需要点击Unity菜单里面的ILRuntime->Generate CLR Binding Code来生成绑定代码");
            Debug.Log("这里Demo已经预先执行过了，如果Unity主工程这边接口发生变化或者添加新接口，别忘了重新生成绑定代码");


            //由于CLR重定向只能重定向一次，并且CLR绑定就是利用的CLR重定向，所以请在初始化最后阶段再执行下面的代码，以保证CLR重定向生效
            //请在生成了绑定代码后解除下面这行的注释
            //请在生成了绑定代码后解除下面这行的注释
            //请在生成了绑定代码后解除下面这行的注释
            //请在生成了绑定代码后解除下面这行的注释
            //ILRuntime.Runtime.Generated.CLRBindings.Initialize(appdomain);
            //这个只是为了演示加的，平时不要这么用，直接在InitializeILRuntime方法里面写CLR绑定注册就行了
            RetryCLRRedirection();

            var type = appdomain.LoadedTypes["HotFix_Project.TestCLRBinding"];
            var m = type.GetMethod("RunTest", 0);
            Debug.Log("现在我们再来试试绑定后的效果");
            sw.Reset();
            sw.Start();
            Profiler.BeginSample("RunTest2");
            appdomain.Invoke(m, null, null);
            Profiler.EndSample();
            sw.Stop();
            Debug.LogFormat("刚刚的方法执行了:{0} ms", sw.ElapsedMilliseconds);

            Debug.Log("可以看到运行时间和GC Alloc有大量的差别，RunTest2之所以有20字节的GC Alloc是因为Editor模式ILRuntime会有调试支持，正式发布（关闭Development Build）时这20字节也会随之消失");
        }
    }

    void RunTest()
    {
        appdomain.Invoke("HotFix_Project.TestCLRBinding", "RunTest", null, null);
    }

    void RunTest2(IMethod m)
    {
        appdomain.Invoke(m, null, null);
    }
}
