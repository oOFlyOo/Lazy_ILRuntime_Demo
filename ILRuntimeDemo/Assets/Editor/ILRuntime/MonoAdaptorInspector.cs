
using ILRuntime.Runtime.Adaptor;
using UnityEditor;

[CustomEditor(typeof(MonoBehaviourAdapter.MonoAdaptor), true)]
public class MonoAdaptorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var adaptor = target as MonoBehaviourAdapter.MonoAdaptor;
        var instance = adaptor.ILInstance;

        if (instance != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ILType:", instance.Type.Name);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("MonoMessage:");
            EditorGUI.indentLevel = 1;
            foreach (var pair in adaptor.MonoMethodDict)
            {
                EditorGUILayout.LabelField(pair.Key);
            }
            EditorGUI.indentLevel = 0;
        }
    }
}
