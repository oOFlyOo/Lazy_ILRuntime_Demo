using UnityEngine;
using System.Collections;
using System.IO;
using ILRuntime.Runtime.Enviorment;

public class HelloWorld : MonoBehaviour
{
    public enum TestEnum
    {
        Test = 1,    
    }

    public static int Property
    {
        get { return 1; }
        private set { }
    }

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

        InitializeILRuntime();
        OnHotFixLoaded();

        yield return null;
    }

    void InitializeILRuntime()
    {
        //这里做一些ILRuntime的注册，HelloWorld示例暂时没有需要注册的
    }

    void OnHotFixLoaded()
    {
        //HelloWorld，第一次方法调用
        appdomain.Invoke("HotFix_Project.InstanceClass", "StaticFunTest", null, null);
    }

    public static void GenericMethod<T>()
    {
        Debug.Log(typeof(T));
    }

    void Update()
    {

    }
}
