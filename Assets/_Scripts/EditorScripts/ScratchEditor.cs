using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Scratch))]
public class ScratchEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Scratch myScript = (Scratch)target;
        if (GUILayout.Button("Vector2.SignedAngle(from, to)"))
        {
            Debug.Log(Vector2.SignedAngle(myScript.from, myScript.to));
        }
    }
}
