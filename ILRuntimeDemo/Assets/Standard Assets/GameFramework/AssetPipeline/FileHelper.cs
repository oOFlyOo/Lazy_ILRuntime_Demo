
using System;
using System.IO;
using UnityEngine;

public static class FileHelper
{
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
//                using (FileStream fs = new FileStream(path, mode, access))
//                {
//                    callback(fs);
//                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                callback(null);
            }
        }
    }
}
