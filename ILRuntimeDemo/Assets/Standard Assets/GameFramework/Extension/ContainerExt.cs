
using System.Collections.Generic;

public static class ContainerExt
{
    #region List<T>
    public static bool AddIfWithout<T>(this List<T> list, T item)
    {
        if (!list.Contains(item))
        {
            list.Add(item);
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
}
