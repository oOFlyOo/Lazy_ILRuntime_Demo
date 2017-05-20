
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FileHelper
{
    #region File
    public static byte[] ReadAllBytes(string path)
    {
        try
        {
            return File.ReadAllBytes(path);
        }
        catch (Exception e)
        {
            Debug.LogException(e);

            return null;
        }
    }

    public static void WriteAllText(string path, string contents)
    {
        CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, contents);
    }

    public static void ReadFileStream(string path, FileMode mode, FileAccess access, Action<FileStream> callback, bool dispose = true)
    {
        if (callback != null)
        {
            try
            {
                var fs = new FileStream(path, mode, access);
                callback(fs);
                if (dispose)
                {
                    fs.Dispose();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                callback(null);
            }
        }
    }
    #endregion

    #region Directory

    public static bool CreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
}
