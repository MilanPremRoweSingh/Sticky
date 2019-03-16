using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ColliderBuilder))]
public class ColliderBuilderEditor : Editor
{
    public void OnSceneGUI()
    {
        ColliderBuilder myScript = (ColliderBuilder)target;
        if (myScript.drawProjectedMesh && ColliderBuilder.debugEdges.Count > 0)
        {
            for (int i = 0; i < ColliderBuilder.debugEdges.Count; i += 2)
            {
                Handles.DrawLine(ColliderBuilder.debugEdges[i], ColliderBuilder.debugEdges[i + 1]);
            }
        }
    }

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
