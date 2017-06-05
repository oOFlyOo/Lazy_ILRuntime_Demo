using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;

public abstract class TestClassBase
{
    public virtual int Value
    {
        get
        {
            return 0;
        }
    }

    public virtual void TestVirtual(string str)
    {
        Debug.Log("!! TestClassBase.TestVirtual, str = " + str);
    }

    public abstract void TestAbstract(int gg);
}
public class Inheritance : MonoBehaviour
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
        //这里做一些ILRuntime的注册，这里应该写继承适配器的注册，为了演示方便，这个例子写在OnHotFixLoaded了
    }

    void OnHotFixLoaded()
    {
        Debug.Log("首先我们来创建热更里的类实例");
        TestClassBase obj;
        try
        {
            obj = appdomain.Instantiate<TestClassBase>("HotFix_Project.TestInheritance");
        }
        catch(System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        Debug.Log("Oops, 报错了，因为跨域继承必须要注册适配器。 如果是热更DLL里面继承热更里面的类型，不需要任何注册。");

        Debug.Log("所以现在我们来注册适配器");
        appdomain.RegisterCrossBindingAdaptor(new InheritanceAdapter());
        Debug.Log("现在再来尝试创建一个实例");
        obj = appdomain.Instantiate<TestClassBase>("HotFix_Project.TestInheritance");
        Debug.Log("现在来调用成员方法");
        obj.TestAbstract(123);
        obj.TestVirtual("Hello");

        Debug.Log("现在换个方式创建实例");
        obj = appdomain.Invoke("HotFix_Project.TestInheritance", "NewObject", null, null) as TestClassBase;
        obj.TestAbstract(456);
        obj.TestVirtual("Foobar");

    }

    void Update()
    {

    }
}
