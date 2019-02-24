using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderBuilder : MonoBehaviour
{
    public string tagForGeneration;

    public void GenerateLevelColliders()
    {
        foreach (Transform child in transform)
        {
            GameObject childObj = child.gameObject;
            if (childObj.tag == tagForGeneration)
            {
                Collider2D coll2D = GenerateCollider2DForObject(childObj);
            }
        }
    }

    // Creates a PolygonCollider for the object \
    public static Collider2D GenerateCollider2DForObject(GameObject obj)
    {
        // Ensure the obj has a mesh filter with a non-null mesh
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null) return null;

        Mesh mesh = meshFilter.sharedMesh;

        return null;
    }
    
}
