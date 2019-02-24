using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderBuilder : MonoBehaviour
{
    public string tagForGeneration;
    public static float vectorEqThreshold;

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
        AutoWeld(mesh, 1e-2f, 1);
        
        List<Vector2> verts = new List<Vector2>(mesh.vertexCount);
        Vector3[] meshVerts = mesh.vertices;
        Dictionary<UnorderedIndexPair, int> edges = new Dictionary<UnorderedIndexPair, int>();
        int[] tris = mesh.GetIndices(0);
        for (int i = 0; i < tris.Length/3; i++) 
        {
            UnorderedIndexPair e0 = new UnorderedIndexPair(tris[3*i+0],tris[3*i+1]);
            if (edges.ContainsKey(e0))
            {
                edges[e0] += 1;
            }
            else
            {
                edges[e0] = 1;
            }
            
            UnorderedIndexPair e1 = new UnorderedIndexPair(tris[3*i+1],tris[3*i+2]);
            if (edges.ContainsKey(e1))
            {
                edges[e1] += 1;
            }
            else
            {
                edges[e1] = 1;
            }
        }

        foreach (UnorderedIndexPair edge in edges.Keys)
        {
            if (edges[edge] == 1)
            {
                verts.Add(new Vector2(meshVerts[edge.first].x, meshVerts[edge.first].y));
                verts.Add(new Vector2(meshVerts[edge.second].x, meshVerts[edge.second].y));
            }
        }
        DestroyImmediate(prevColl);

        EdgeCollider2D coll = obj.AddComponent<EdgeCollider2D>();
        Vector2[] vertList = new Vector2[verts.Count+1];
        //vertList[verts.Count] = vertList[0];
        verts.CopyTo(vertList);
        coll.points = verts.ToArray();

        return coll;
    }

    public static void AutoWeld(Mesh mesh, float threshold, float bucketStep)
    {
        Vector3[] oldVertices = mesh.vertices;
        Vector3[] newVertices = new Vector3[oldVertices.Length];
        int[] old2new = new int[oldVertices.Length];
        int newSize = 0;

        // Find AABB
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < oldVertices.Length; i++)
        {
            if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
            if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
            if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
            if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
            if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
            if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
        }

        // Make cubic buckets, each with dimensions "bucketStep"
        int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
        int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
        int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
        List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

        // Make new vertices
        for (int i = 0; i < oldVertices.Length; i++)
        {
            // Determine which bucket it belongs to
            int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
            int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
            int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

            // Check to see if it's already been added
            if (buckets[x, y, z] == null)
                buckets[x, y, z] = new List<int>(); // Make buckets lazily

            for (int j = 0; j < buckets[x, y, z].Count; j++)
            {
                Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                if (Vector3.SqrMagnitude(to) < threshold)
                {
                    old2new[i] = buckets[x, y, z][j];
                    goto skip; // Skip to next old vertex if this one is already there
                }
            }

            // Add new vertex
            newVertices[newSize] = oldVertices[i];
            buckets[x, y, z].Add(newSize);
            old2new[i] = newSize;
            newSize++;

        skip:;
        }

        // Make new triangles
        int[] oldTris = mesh.triangles;
        int[] newTris = new int[oldTris.Length];
        for (int i = 0; i < oldTris.Length; i++)
        {
            newTris[i] = old2new[oldTris[i]];
        }

        Vector3[] finalVertices = new Vector3[newSize];
        for (int i = 0; i < newSize; i++)
            finalVertices[i] = newVertices[i];

        mesh.Clear();
        mesh.vertices = finalVertices;
        mesh.triangles = newTris;
        mesh.RecalculateNormals();
    }


}

public class UnorderedIndexPair
{
    public int first;
    public int second;

    public override bool Equals(object obj)
    {
        var a = obj as UnorderedIndexPair;

        if (obj == null) return false;

        return (a.first == first && a.second == second) || (a.first == second && a.second == first);
    }

    public override int GetHashCode()
    {
        return (first << 16) + second;
    }

    public UnorderedIndexPair(int _first, int _second)
    {
        first = _first;
        second = _second;
    }
}

//public class UnorderedIndexPairComparer : IEqualityComparer<UnorderedIndexPair>
//{
//    public bool Equals(UnorderedIndexPair a)
//    {
//        return a.Equals(a,b);// (StickyMath.InRange(dist.magnitude, -ColliderBuilder.vectorEqThreshold, ColliderBuilder.vectorEqThreshold));
//    }

//    public int GetHashCode(UnorderedIndexPair obj)
//    {
//        return obj.GetHashCode();
//    }
//}

public class Vector2Comparer : IEqualityComparer<Vector2>
{
    public bool Equals(Vector2 a, Vector2 b)
    {
        Vector2 dist = a - b;
        return false;// (StickyMath.InRange(dist.magnitude, -ColliderBuilder.vectorEqThreshold, ColliderBuilder.vectorEqThreshold));
    }

    public int GetHashCode(Vector2 obj)
    {
        return obj.GetHashCode();
    }
}
