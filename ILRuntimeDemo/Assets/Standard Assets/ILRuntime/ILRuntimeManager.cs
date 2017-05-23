
using System;
using System.IO;
using System.Reflection;
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

    public static ILRuntimeManager Create(bool binding = true)
    {
        if (Instance == null)
        {
            var ins = InternalCreate();
            ins.LoadCreateAppDomain(binding);
            SetInstance(ins);
        }

        return Instance;
    }

    private void LoadCreateAppDomain(bool binding)
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

        InitializeILRuntime(binding);
    }


    private void InitializeILRuntime(bool binding)
    {
        if (Application.isPlaying && binding)
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
