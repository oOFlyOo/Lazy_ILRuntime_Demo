﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
[System.Reflection.Obfuscation(Exclude = true)]
public class ILRuntimeCLRBinding
{
    
    [MenuItem("ILRuntime/Generate CLR Binding Code")]
    static void GenerateCLRBinding()
    {
        List<Type> types = new List<Type>();
        types.Add(typeof(int));
        types.Add(typeof(float));
        types.Add(typeof(long));
        types.Add(typeof(object));
        types.Add(typeof(string));
        types.Add(typeof(Array));
        types.Add(typeof(Vector2));
        types.Add(typeof(Vector3));
        types.Add(typeof(Quaternion));
        types.Add(typeof(GameObject));
        types.Add(typeof(UnityEngine.Object));
        types.Add(typeof(Transform));
        types.Add(typeof(RectTransform));
        types.Add(typeof(CLRBindingTestClass));
        types.Add(typeof(Time));
        types.Add(typeof(Debug));
        //所有DLL内的类型的真实C#类型都是ILTypeInstance
        types.Add(typeof(List<ILRuntime.Runtime.Intepreter.ILTypeInstance>));

        ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(types, "Assets/ILRuntime/Generated");
    }
}
#endif
