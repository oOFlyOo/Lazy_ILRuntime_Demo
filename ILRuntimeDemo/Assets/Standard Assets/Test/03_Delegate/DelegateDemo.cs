﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

public delegate void TestDelegateMethod(int a);
public delegate string TestDelegateFunction(int a);


public class DelegateDemo : MonoBehaviour
{
    public static TestDelegateMethod TestMethodDelegate;
    public static TestDelegateFunction TestFunctionDelegate;
    public static System.Action<string> TestActionDelegate;

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
        //这里做一些ILRuntime的注册，比如委托的适配器，但是为了演示不些适配器的报错，注册写在了OnHotFixLoaded里

    }

    //这个方法仅仅是为了演示，强制删除缓存的委托适配器，实际项目不要这么调用
    void ClearDelegateCache()
    {
        var type = appdomain.LoadedTypes["HotFix_Project.TestDelegate"];
        ILMethod m = type.GetMethod("Method", 1) as ILMethod;
        m.DelegateAdapter = null;

        m = type.GetMethod("Function", 1) as ILMethod;
        m.DelegateAdapter = null;

        m = type.GetMethod("Action", 1) as ILMethod;
        m.DelegateAdapter = null;
    }

    void OnHotFixLoaded()
    {
        Debug.Log("完全在热更DLL内部使用的委托，直接可用，不需要做任何处理");
        appdomain.Invoke("HotFix_Project.TestDelegate", "Initialize", null, null);
        appdomain.Invoke("HotFix_Project.TestDelegate", "RunTest", null, null);

        Debug.Log("如果需要跨域调用委托（将热更DLL里面的委托实例传到Unity主工程用）, 就需要注册适配器，不然就会像下面这样");
        try
        {
            appdomain.Invoke("HotFix_Project.TestDelegate", "Initialize2", null, null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        //为了演示，清除适配器缓存，实际使用中不要这么做
        ClearDelegateCache();
        Debug.Log("这是因为iOS的IL2CPP模式下，不能动态生成类型，为了避免出现不可预知的问题，我们没有通过反射的方式创建委托实例，因此需要手动进行一些注册");
        Debug.Log("首先需要注册委托适配器,刚刚的报错的错误提示中，有提示需要的注册代码");
        //下面这些注册代码，正式使用的时候，应该写在InitializeILRuntime中
        //TestDelegateMethod, 这个委托类型为有个参数为int的方法，注册仅需要注册不同的参数搭配即可
        appdomain.DelegateManager.RegisterMethodDelegate<int>();
        //带返回值的委托的话需要用RegisterFunctionDelegate，返回类型为最后一个
        appdomain.DelegateManager.RegisterFunctionDelegate<int, string>();
        //Action<string> 的参数为一个string
        appdomain.DelegateManager.RegisterMethodDelegate<string>();

        
        Debug.Log("注册完毕后再次运行会发现这次会报另外的错误");
        try
        {
            appdomain.Invoke("HotFix_Project.TestDelegate", "Initialize2", null, null);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }

        Debug.Log("ILRuntime内部是用Action和Func这两个系统内置的委托类型来创建实例的，所以其他的委托类型都需要写转换器");
        Debug.Log("将Action或者Func转换成目标委托类型");

        appdomain.DelegateManager.RegisterDelegateConvertor<TestDelegateMethod>((action) =>
        {
            //转换器的目的是把Action或者Func转换成正确的类型，这里则是把Action<int>转换成TestDelegateMethod
            return new TestDelegateMethod((a) =>
            {
                //调用委托实例
                ((System.Action<int>)action)(a);
            });
        });
        //对于TestDelegateFunction同理，只是是将Func<int, string>转换成TestDelegateFunction
        appdomain.DelegateManager.RegisterDelegateConvertor<TestDelegateFunction>((action) =>
        {
            return new TestDelegateFunction((a) =>
            {
                return ((System.Func<int, string>)action)(a);
            });
        });

        //下面再举一个这个Demo中没有用到，但是UGUI经常遇到的一个委托，例如UnityAction<float>
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<float>>((action) =>
        {
            return new UnityEngine.Events.UnityAction<float>((a) =>
            {
                ((System.Action<float>)action)(a);
            });
        });


        Debug.Log("现在我们再来运行一次");
        appdomain.Invoke("HotFix_Project.TestDelegate", "Initialize2", null, null);
        appdomain.Invoke("HotFix_Project.TestDelegate", "RunTest2", null, null);
        Debug.Log("运行成功，我们可以看见，用Action或者Func当作委托类型的话，可以避免写转换器，所以项目中在不必要的情况下尽量只用Action和Func");
        Debug.Log("另外应该尽量减少不必要的跨域委托调用，如果委托只在热更DLL中用，是不需要进行任何注册的");
        Debug.Log("---------");
        Debug.Log("我们再来在Unity主工程中调用一下刚刚的委托试试");
        TestMethodDelegate(789);
        var str = TestFunctionDelegate(098);
        Debug.Log("!! OnHotFixLoaded str = " + str);
        TestActionDelegate("Hello From Unity Main Project");

    }

    void Update()
    {

    }
}
