
using System;
using System.IO;
using System.Reflection;
using ILRuntime.Runtime.Enviorment;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public class ILRuntimeManager: Singleton<ILRuntimeManager>
{
    public AppDomain Domain
    {
        get { return _appDomain; }
    }

    private AppDomain _appDomain;
    private FileStream _mdbFileStream;

    public static ILRuntimeManager Create(bool initBinding = true, bool initAdaptor = true)
    {
        if (Instance == null)
        {
            var ins = InternalCreate();
            ins.LoadCreateAppDomain(initBinding, initAdaptor);
            SetInstance(ins);
        }

        return Instance;
    }

    private void LoadCreateAppDomain(bool initBinding, bool initAdaptor)
    {
        _appDomain = new AppDomain();
        FileHelper.ReadFileStream(ILRuntimePaths.AssemblyCSharpPath, FileMode.Open, FileAccess.Read, stream =>
        {
#if DEBUG
            var useMDB = true;
#else
            var useMDB = false;
#endif

            if (useMDB && Application.isPlaying)
            {
                FileHelper.ReadFileStream(ILRuntimePaths.AssemblyCSharpMDBPath, FileMode.Open, FileAccess.ReadWrite,  fileStream =>
                {
                    _appDomain.LoadAssemblyMDB(stream, fileStream);
                    _mdbFileStream = fileStream;
                    MonoMessageManager.Instance.OnApplicationQuitEvent += DestroyInstance;
                }, false);
            }
            else
            {
                _appDomain.LoadAssembly(stream);
            }
        });

        InitializeILRuntime(initBinding, initAdaptor);
    }


    private void InitializeILRuntime(bool initBinding, bool initAdaptor)
    {
        InitializeStaticBinding();

        if (Application.isPlaying && initBinding)
        {
            InitializeBinding();
        }

        if (initAdaptor)
        {
            InitializeAdaptor();
        }
    }

    private void InitializeStaticBinding()
    {
        ILRuntime.Binding.Redirect.CLRBindings.Initialize(_appDomain);
    }

    private void InitializeBinding()
    {
        var type = Type.GetType("ILRuntime.Binding.Generated.CLRBindings");
        if (type != null)
        {
            var method = type.GetMethod("Initialize");
            if (method != null)
            {
                Debug.Log("InitializeILRuntime");

                method.Invoke(null, new[] { _appDomain });
            }
        }
    }

    private void InitializeAdaptor()
    {
        var adaptorNamespace = "ILRuntime.Runtime.Adaptor";
        var adaptorType = typeof(CrossBindingAdaptor);
        var types = GetType().Assembly.GetTypes();
        foreach (var type1 in types)
        {
            if (type1.Namespace == adaptorNamespace && type1.IsSubclassOf(adaptorType))
            {
                _appDomain.RegisterCrossBindingAdaptor(Activator.CreateInstance(type1) as CrossBindingAdaptor);
            }
        }
    }

    protected override void OnDeleted()
    {
        if (_mdbFileStream != null)
        {
            _mdbFileStream.Dispose();
            _mdbFileStream = null;
            MonoMessageManager.Instance.OnApplicationQuitEvent -= DestroyInstance;
        }
    }
}
