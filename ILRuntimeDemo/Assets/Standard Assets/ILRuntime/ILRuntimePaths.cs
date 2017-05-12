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

    public const string AssemblyCSharpMDBName = "Assembly-CSharp.dll.mdb";
    public static string AssemblyCSharpMDBPath
    {
        get { return string.Format("{0}/Library/ScriptAssemblies/{1}", DataPath, AssemblyCSharpMDBName); }
    }

}
