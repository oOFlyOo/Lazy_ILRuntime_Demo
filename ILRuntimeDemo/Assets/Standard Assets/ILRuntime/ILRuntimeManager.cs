
using System.IO;
using ILRuntime.Runtime.Enviorment;

public class ILRuntimeManager: Singleton<ILRuntimeManager>
{
    public AppDomain Domain
    {
        get { return _appDomain; }
    }

    private AppDomain _appDomain;
    private FileStream _mdbFileStream;

    public static void Create()
    {
        var ins = InternalCreate();
        ins.LoadCreateAppDomain();
        SetInstance(ins);
    }

    private void LoadCreateAppDomain()
    {
        _appDomain = new AppDomain();
        FileHelper.ReadFileStream(ILRuntimePaths.AssemblyCSharpPath, FileMode.Open, FileAccess.Read, stream =>
        {
//            _appDomain.LoadAssembly(stream);
            FileHelper.ReadFileStream(ILRuntimePaths.AssemblyCSharpMDBPath, FileMode.Open, FileAccess.ReadWrite,  fileStream =>
            {
                _appDomain.LoadAssemblyMDB(stream, fileStream);
                _mdbFileStream = fileStream;
                MonoMessageManager.Instance.OnApplicationQuitEvent += DestroyInstance;
            }, false);
        });
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
