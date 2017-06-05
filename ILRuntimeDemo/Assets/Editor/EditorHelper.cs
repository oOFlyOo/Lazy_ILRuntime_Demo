
using System;

public static class EditorHelper
{
    public static Type GetType(string name)
    {
        Type type = null;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(name);
            if (type != null)
            {
                return type;
            }
        }

        return type;
    }
}
