﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Adaptor;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Enviorment;

public class MonoBehaviourDemo : MonoBehaviour
{
    static MonoBehaviourDemo instance;

    public static MonoBehaviourDemo Instance
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

        InitializeILRuntime();
        OnHotFixLoaded();
    }

    unsafe void InitializeILRuntime()
    {
        //这里做一些ILRuntime的注册
//        appdomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
    }

    //这个仅为演示demo用，平时不要这么调用
    void RetryCLRRedirection()
    {
        var type = appdomain.GetType(typeof(GameObject));
        var mt = appdomain.LoadedTypes["HotFix_Project.SomeMonoBehaviour"];
        CLRMethod method = type.GetMethod("AddComponent", new List<IType>(), new IType[] { mt }, mt) as CLRMethod;
        method.RetryCLRRedirection();        
    }

    //这个仅为演示demo用，平时不要这么调用
    void RetryCLRRedirection2()
    {
        var type = appdomain.GetType(typeof(GameObject));
        var mt = appdomain.LoadedTypes["HotFix_Project.SomeMonoBehaviour2"];
        CLRMethod method = type.GetMethod("GetComponent", new List<IType>(), new IType[] { mt }, mt) as CLRMethod;
        method.RetryCLRRedirection();
    }


    unsafe void OnHotFixLoaded()
    {
        Debug.Log("在热更DLL里面使用MonoBehaviour是可以做到的，但是并不推荐这么做");
        Debug.Log("因为即便能做到使用，要完全支持MonoBehaviour的所有特性，会需要很多额外的工作量");
        Debug.Log("而且通过MonoBehaviour做游戏逻辑当项目规模大到一定程度之后会是个噩梦，因此应该尽量避免");

        Debug.Log("下面我们来通过热更DLL往这个GameObject上挂一个热更里面的MonoBehaviour");
        try
        {
//            appdomain.Invoke("HotFix_Project.TestMonoBehaviour", "RunTest", null, gameObject);
        }
        catch(System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        Debug.Log("很不幸，报错了，因为GameObject.AddComponent<T>这个方法是Unity实现的，他并不可能取到热更DLL内部的类型");
        Debug.Log("因此我们需要挟持AddComponent方法，然后自己实现");
        Debug.Log("我们先销毁掉之前创建的不合法的MonoBehaviour");
//        Object.Destroy(GetComponent<MonoBehaviourAdapter.Adaptor>());
//        SetupCLRRedirection();
        appdomain.Invoke("HotFix_Project.TestMonoBehaviour", "RunTest", null, gameObject);

        Debug.Log("可以看到已经成功了");
        Debug.Log("下面做另外一个实验");
        try
        {
//            appdomain.Invoke("HotFix_Project.TestMonoBehaviour", "RunTest2", null, gameObject);
        }
        catch(System.Exception ex)
        {
            Debug.LogError(ex.ToString());
        }
        Debug.Log("我们发现GetComponent出错了，这个跟AddComponent类似，需要我们自己处理");
//        SetupCLRRedirection2();
        Debug.Log("再试一次");
        appdomain.Invoke("HotFix_Project.TestMonoBehaviour", "RunTest2", null, gameObject);
        Debug.Log("再试一次， 成功了");
        Debug.Log("那我们怎么从Unity主工程获取热更DLL的MonoBehaviour呢？");
        Debug.Log("这需要我们自己实现一个GetComponent方法");
        var type = appdomain.LoadedTypes["HotFix_Project.SomeMonoBehaviour2"] as ILType;
        var smb = GetComponent(type);
        var m = type.GetMethod("Test2");
        Debug.Log("现在来试试调用");
        appdomain.Invoke(m, smb, null);

        Debug.Log("调用成功！");
        Debug.Log("我们点一下左边列表里的GameObject，查看一下我们刚刚挂的脚本");
        Debug.Log("默认情况下是无法显示DLL里面定义的public变量的值的");
        Debug.Log("这个Demo我们写了一个自定义Inspector来查看变量，同样只是抛砖引玉");
        Debug.Log("要完整实现MonoBehaviour所有功能得大家自己花功夫了，最好还是避免脚本里使用MonoBehaviour");
        Debug.Log("具体实现请看MonoBehaviourAdapterEditor");
        Debug.Log("特别注意，现在仅仅是运行时可以看到和编辑，由于没有处理序列化的问题，所以并不可能保存到Prefab当中，要想实现就得靠大家自己了");
    }

    unsafe void SetupCLRRedirection()
    {
        //这里面的通常应该写在InitializeILRuntime，这里为了演示写这里
        var arr = typeof(GameObject).GetMethods();
        foreach (var i in arr)
        {
            if (i.Name == "AddComponent" && i.GetGenericArguments().Length == 1)
            {
                appdomain.RegisterCLRMethodRedirection(i, AddComponent);
            }
        }
        //这个仅为演示demo用，平时不要这么调用
        RetryCLRRedirection();
    }

    unsafe void SetupCLRRedirection2()
    {
        //这里面的通常应该写在InitializeILRuntime，这里为了演示写这里
        var arr = typeof(GameObject).GetMethods();
        foreach (var i in arr)
        {
            if (i.Name == "GetComponent" && i.GetGenericArguments().Length == 1)
            {
                appdomain.RegisterCLRMethodRedirection(i, GetComponent);
            }
        }
        //这个仅为演示demo用，平时不要这么调用
        RetryCLRRedirection2();
    }

    MonoBehaviourAdapter.MonoAdaptor GetComponent(ILType type)
    {
        var monoAdaptorType = ILRMonoAdaptorHelper.GetMonoAdaptorType(type);
        var arr = GetComponents(monoAdaptorType);
        for(int i = 0; i < arr.Length; i++)
        {
            var instance = arr[i] as MonoBehaviourAdapter.MonoAdaptor;
            if(instance.ILInstance != null && instance.ILInstance.Type == type)
            {
                return instance;
            } 
        }
        return null;
    }

    unsafe static StackObject* AddComponent(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        //CLR重定向的说明请看相关文档和教程，这里不多做解释
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        //成员方法的第一个参数为this
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new System.NullReferenceException();
        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        //AddComponent应该有且只有1个泛型参数
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res;
            if(type is CLRType)
            {
                //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                res = instance.AddComponent(type.TypeForCLR);
            }
            else
            {
                //热更DLL内的类型比较麻烦。首先我们得自己手动创建实例
                var ilInstance = new ILTypeInstance(type as ILType, false);//手动创建实例是因为默认方式会new MonoBehaviour，这在Unity里不允许
                //接下来创建Adapter实例
                var monoAdaptorType = ILRMonoAdaptorHelper.GetMonoAdaptorType(type as ILType);
                var clrInstance = instance.AddComponent(monoAdaptorType) as MonoBehaviourAdapter.MonoAdaptor;
                //unity创建的实例并没有热更DLL里面的实例，所以需要手动赋值
                clrInstance.Init(ilInstance, __domain);
                //这个实例默认创建的CLRInstance不是通过AddComponent出来的有效实例，所以得手动替换
//                ilInstance.CLRInstance = clrInstance;

                res = clrInstance.ILInstance;//交给ILRuntime的实例应该为ILInstance

//                clrInstance.Awake();//因为Unity调用这个方法时还没准备好所以这里补调一次
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }

    unsafe static StackObject* GetComponent(ILIntepreter __intp, StackObject* __esp, List<object> __mStack, CLRMethod __method, bool isNewObj)
    {
        //CLR重定向的说明请看相关文档和教程，这里不多做解释
        ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;

        var ptr = __esp - 1;
        //成员方法的第一个参数为this
        GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
        if (instance == null)
            throw new System.NullReferenceException();
        __intp.Free(ptr);

        var genericArgument = __method.GenericArguments;
        //AddComponent应该有且只有1个泛型参数
        if (genericArgument != null && genericArgument.Length == 1)
        {
            var type = genericArgument[0];
            object res = null;
            if (type is CLRType)
            {
                //Unity主工程的类不需要任何特殊处理，直接调用Unity接口
                res = instance.GetComponent(type.TypeForCLR);
            }
            else
            {
                //因为所有DLL里面的MonoBehaviour实际都是这个Component，所以我们只能全取出来遍历查找
                var monoAdaptorType = ILRMonoAdaptorHelper.GetMonoAdaptorType(type as ILType);
                var clrInstances = instance.GetComponents(monoAdaptorType);
                for(int i = 0; i < clrInstances.Length; i++)
                {
                    var clrInstance = clrInstances[i] as MonoBehaviourAdapter.MonoAdaptor;
                    if (clrInstance.ILInstance != null)//ILInstance为null, 表示是无效的MonoBehaviour，要略过
                    {
                        if (clrInstance.ILInstance.Type == type)
                        {
                            res = clrInstance.ILInstance;//交给ILRuntime的实例应该为ILInstance
                            break;
                        }
                    }
                }
            }

            return ILIntepreter.PushObject(ptr, __mStack, res);
        }

        return __esp;
    }
}
