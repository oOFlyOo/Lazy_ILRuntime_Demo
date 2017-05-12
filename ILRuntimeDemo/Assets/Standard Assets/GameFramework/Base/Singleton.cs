
using System;

public abstract class Singleton<T> where T: Singleton<T>
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null || !_instance._internalCreateSuccess)
            {
                _instance = null;

                var ins = InternalCreate();
                ins.OnInit();
                if (ins._internalCreateSuccess)
                {
                    _instance = ins;
                }
            }

            return _instance;
        }
    }

    protected static T InternalCreate()
    {
        return Activator.CreateInstance<T>();
    }

    /// <summary>
    /// 自己创建的得在这里赋值
    /// 在创建的最后才赋值比较靠谱
    /// </summary>
    /// <param name="ins"></param>
    protected static void SetInstance(T ins)
    {
        ins.OnInit();
        ins._internalCreateSuccess = true;
        _instance = ins;
    }

    public static void DestroyInstance()
    {
        if (_instance != null)
        {
            if (_instance._internalCreateSuccess)
            {
                _instance.OnDeleted();
            }
            _instance = null;
        }
    }

    protected Singleton()
    {
        
    }

    private bool _internalCreateSuccess = true;
    /// <summary>
    /// 对于那些通过 Instance 来实例化的，确保重载 OnInit
    /// 直接通过静态创建的，可以无视，也可以不用继承或者调用此方法
    /// </summary>
    protected virtual void OnInit()
    {
        _internalCreateSuccess = false;
    }

    protected virtual void OnDeleted()
    {
        
    }
}
