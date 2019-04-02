using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PipeExit), true)]
public class PipeExitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var picker = target as PipeExit;
        var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(picker.scenePath);

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        var newScene = EditorGUILayout.ObjectField("Scene", oldScene, typeof(SceneAsset), false) as SceneAsset;

        if (EditorGUI.EndChangeCheck())
        {
            var newPath = AssetDatabase.GetAssetPath(newScene);
            var scenePathProperty = serializedObject.FindProperty("scenePath");
            scenePathProperty.stringValue = newPath;
        }
        serializedObject.ApplyModifiedProperties();
    }
}