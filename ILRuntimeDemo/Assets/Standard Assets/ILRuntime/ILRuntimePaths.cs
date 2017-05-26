using UnityEngine;
using System.IO;

public static class ILRuntimePaths
{
    private static string _dataPath;
    public static string DataPath
    {
        get
        {
            if (_dataPath == null)
            {
                _dataPath = Path.GetDirectoryName(Application.dataPath);
            }
            return _dataPath;
        }
    }

    public const string AssemblyCSharpName = "Assembly-CSharp.dll";
    public static string AssemblyCSharpPath
    {
        get { return string.Format("{0}/Library/ScriptAssemblies/{1}", DataPath, AssemblyCSharpName); }
    }

    public static string FrameworkyCSharpPath
    {
        get { return string.Format("{0}/Library/ScriptAssemblies/Assembly-CSharp-firstpass.dll", DataPath); }
    }

    public const string AssemblyCSharpMDBName = "Assembly-CSharp.dll.mdb";
    public static string AssemblyCSharpMDBPath
    {
        get { return string.Format("{0}/Library/ScriptAssemblies/{1}", DataPath, AssemblyCSharpMDBName); }
    }

    public const string FrameworkMessagePath = "Assets/Editor/ILRuntime/FrameworkMessage.txt";
    public const string BindingCodeMessagePath = "Assets/Editor/ILRuntime/BindingCodeMessage.txt";
    public const string BindingCodePath = "Assets/Standard Assets/ILRuntime/Binding/Generated";
    public const string AdaptorCodePath = "Assets/Standard Assets/ILRuntime/Adaptors/Generated";
}
