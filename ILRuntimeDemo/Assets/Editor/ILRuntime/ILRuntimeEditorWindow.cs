
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
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("生成绑定"))
            {
                ILRuntimeBindingGenerator.Generate();
                AssetDatabase.Refresh();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("生成 MonoMessage"))
            {
                ILRuntimeMonoAdaptorGenerator.Generate();
                AssetDatabase.Refresh();
            }
        }
        EditorGUILayout.EndScrollView();
    }
}
