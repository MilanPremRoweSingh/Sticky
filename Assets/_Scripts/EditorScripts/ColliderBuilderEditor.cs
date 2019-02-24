using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ColliderBuilder))]
public class ColliderBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ColliderBuilder myScript = (ColliderBuilder)target;
        if (GUILayout.Button("Generate Colliders"))
        {
            myScript.GenerateLevelColliders();
        }
    }
}
