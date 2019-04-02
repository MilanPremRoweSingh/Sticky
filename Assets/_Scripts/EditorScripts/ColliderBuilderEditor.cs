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
            foreach (List<Vector3> debugShape in ColliderBuilder.debugEdges)
            {
                for (int i = 0; i < debugShape.Count; i += 2)
                {
                    Handles.DrawLine(debugShape[i], debugShape[i + 1]);
                }
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
            myScript.Generate3DColliders(myScript.gameObject);
        }
        if (GUILayout.Button("Clear Debug Shapes"))
        {
            myScript.ClearDebugShapes();
        }
    }
}
