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
        if (GUILayout.Button("Generate 2D Colliders"))
        {
            myScript.GenerateLevelColliders();
        }
        if (GUILayout.Button("Generate 3D Colliders"))
        {
            myScript.Generate3DColliders();
        }
    }
}
