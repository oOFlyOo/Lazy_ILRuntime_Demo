#if UNITY_EDITOR

using UnityEngine;


internal static class BindingAgency
{
    // 仅仅用于方便生成绑定参考
    private static void Binding()
    {
        Debug.Log(null);
        Debug.LogWarning(null);
        Debug.LogError(null);
    }
}

#endif