
using System.Collections.Generic;
using ILRuntime.Reflection;
using UnityEngine;
using UnityEditor;

public class ILRuntimeEditorWindow: EditorWindow
{
    private Vector2 _scrollPos;


    [MenuItem("Custom/ILRuntime/Window")]
    private static ILRuntimeEditorWindow GetWindow()
    {
        return GetWindow<ILRuntimeEditorWindow>(typeof(ILRuntimeEditorWindow).Name.Replace("EditorWindow", ""));
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        {
            if (GUILayout.Button("生成绑定"))
            {
                ILRuntimeBindingGenerator.CLRBinding();
                AssetDatabase.Refresh();
            }
        }
        EditorGUILayout.EndScrollView();
    }
}
