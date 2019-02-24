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
            Collider2D coll2D = GenerateCollider2DForObject(childObj);
        }
    }

    // Creates a shitty EdgeCollider for the object with a mesh collider
    public static Collider2D GenerateCollider2DForObject(GameObject obj)
    {
        MeshCollider prevColl = obj.GetComponent<MeshCollider>();
        if (prevColl == null || prevColl.sharedMesh == null) return null;


        Collider2D prevColl2D = obj.GetComponent<Collider2D>();
        if (prevColl2D != null) DestroyImmediate(prevColl2D);

        Mesh mesh = prevColl.sharedMesh;
        
        List<Vector2> verts = new List<Vector2>(mesh.vertexCount);
        foreach (Vector3 vert in mesh.vertices)
        {
            verts.Add(new Vector2(vert.x, vert.y));
        }
        DestroyImmediate(prevColl);

        EdgeCollider2D coll = obj.AddComponent<EdgeCollider2D>();
        coll.points = verts.ToArray();

        return coll;
    }
    
}
