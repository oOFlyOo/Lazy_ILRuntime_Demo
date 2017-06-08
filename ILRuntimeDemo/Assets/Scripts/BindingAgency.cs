#if UNITY_EDITOR

using System;
using UnityEngine;


internal static class BindingAgency
{
    // 仅仅用于方便生成绑定参考
    private static void Binding()
    {
        Debug.Log(null);
        Debug.LogWarning(null);
        Debug.LogError(null);

        MonoBehaviour mono = null;
        mono.Invoke("", 0);
        mono.InvokeRepeating("", 0, 0);
        mono.CancelInvoke();
        mono.CancelInvoke("");
        mono.IsInvoking();
        mono.IsInvoking("");

        mono.GetComponent<MonoBehaviour>();
        mono.GetComponent((Type) null);
        mono.GetComponentInChildren<MonoBehaviour>();
        mono.GetComponentInChildren<MonoBehaviour>(false);
        mono.GetComponentInChildren((Type)null);
        mono.GetComponentInChildren((Type)null, false);
        mono.GetComponentInParent<MonoBehaviour>();
        mono.GetComponentInParent((Type)null);
        mono.GetComponents<MonoBehaviour>();
        mono.GetComponents<MonoBehaviour>(null);
        mono.GetComponents((Type)null);
        mono.GetComponentsInChildren<MonoBehaviour>();
        mono.GetComponentsInChildren<MonoBehaviour>(false);
        mono.GetComponentsInChildren<MonoBehaviour>(null);
        mono.GetComponentsInChildren<MonoBehaviour>(false, null);
        mono.GetComponentsInChildren((Type)null);
        mono.GetComponentsInChildren((Type)null, false);
        mono.GetComponentsInParent<MonoBehaviour>();
        mono.GetComponentsInParent<MonoBehaviour>(false);
        mono.GetComponentsInParent<MonoBehaviour>(false, null);
        mono.GetComponentsInParent((Type)null);
        mono.GetComponentsInParent((Type)null, false);
    }
}

#endif