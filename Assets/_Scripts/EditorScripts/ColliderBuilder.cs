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
    public void Generate3DColliders()
    {
        foreach (Transform child in transform)
        {
            GameObject childObj = child.gameObject;
            RecreateMeshCollider(childObj);
        }
    }

    // Creates a shitty EdgeCollider for the object with a mesh collider
    /*
    public static Collider2D GenerateCollider2DForObject(GameObject obj)
    {
        MeshCollider prevColl = obj.GetComponent<MeshCollider>();
        if (prevColl == null || prevColl.sharedMesh == null) return null;


        Collider2D prevColl2D = obj.GetComponent<Collider2D>();
        if (prevColl2D != null) DestroyImmediate(prevColl2D);

        Mesh mesh = prevColl.sharedMesh;
        Mesh weldedMesh = (Mesh)Instantiate(mesh);
        //AutoWeld(weldedMesh, 1e-2f, 1);
        
        List<Vector2> verts = new List<Vector2>(weldedMesh.vertexCount);
        Vector3[] meshVerts = weldedMesh.vertices;
        Dictionary<UnorderedIndexPair, int> edges = new Dictionary<UnorderedIndexPair, int>();
        int[] tris = weldedMesh.GetIndices(0);
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
    */

    public static Collider2D GenerateCollider2DForObject(GameObject obj)
    {
        MeshCollider prevColl = obj.GetComponent<MeshCollider>();
        if (prevColl == null || prevColl.sharedMesh == null) return null;


        Collider2D prevColl2D = obj.GetComponent<Collider2D>();
        if (prevColl2D != null) DestroyImmediate(prevColl2D);

        Mesh mesh = prevColl.sharedMesh;
        Vector2[] verts = ProjectAndWeld(mesh, 1e-3f);

        DestroyImmediate(prevColl);
        PolygonCollider2D coll = obj.AddComponent<PolygonCollider2D>();
        coll.points = verts;

        return null;
    }

    // Only works for meshes with a single submesh
    public static Vector2[] ProjectAndWeld(Mesh mesh, float threshold)
    {
        Mesh locMesh = Instantiate<Mesh>(mesh);
        Vector3[] oldVerts = locMesh.vertices;
        int[] oldTris = locMesh.GetIndices(0);

        // Project onto xy-plane
        for (int i = 0; i < locMesh.vertexCount; i++)
        {
            oldVerts[i].z = 0;
        }

        // Map oldVertices which overlap to a single vertex (edges updated after)
        float sqrThreshold = threshold * threshold;
        int[] newVertIndices = new int[locMesh.vertexCount];
        Vector3[] newVerts = new Vector3[locMesh.vertexCount];
        int numNewVerts = 0;
        for (int i = 0; i < locMesh.vertexCount; i++)
        {
            bool vertOverlaps = false;
            for (int j = 0; j < numNewVerts; j++)
            {
                if ((oldVerts[i] - newVerts[j]).sqrMagnitude <= sqrThreshold)
                {
                    newVertIndices[i] = j;
                    vertOverlaps = true;
                    break; // Only ever in the same position as one vert in new verts
                }
            }

            if (!vertOverlaps)
            {
                newVertIndices[numNewVerts] = i;
                newVerts[numNewVerts] = oldVerts[i];
                numNewVerts++;
            }
        }

        // Generate edges for new set of verts, projected on xy-plane ////

        // Stores existing edges, so we don't create duplicate edges
        HashSet<UnorderedIndexPair> edges = new HashSet<UnorderedIndexPair>();
        for (int i = 0; i < oldTris.Length/3; i++)
        {
            int u, v;
            UnorderedIndexPair uv;
            // Handle edge 0
            u = newVertIndices[oldTris[3*i+0]];
            v = newVertIndices[oldTris[3*i+1]];
            uv = new UnorderedIndexPair(u, v);
            if (!edges.Contains(uv) && u != v)
            {
                edges.Add(uv);
            }

            // Handle edge 1
            u = newVertIndices[oldTris[3*i+1]];
            v = newVertIndices[oldTris[3*i+2]];
            uv = new UnorderedIndexPair(u, v);
            if (!edges.Contains(uv) && u != v)
            {
                edges.Add(uv);
            }

            // Handle edge 2
            u = newVertIndices[oldTris[3*i+2]];
            v = newVertIndices[oldTris[3*i+0]];
            uv = new UnorderedIndexPair(u, v);
            if (!edges.Contains(uv) && u != v)
            {
                edges.Add(uv);
            }
        }
        ////


        int maxXIdx = -1;
        float maxX = Mathf.NegativeInfinity;
        for (int i = 0; i < numNewVerts; i++)
        {
            if (newVerts[i].x > maxX)
            {
                maxXIdx = i;
                maxX = newVerts[i].x;
            }
        }

        float minCCWAngle = Mathf.Infinity;
        int prevVert = -1;
        for (int u = 0; u < numNewVerts; u++)
        {
            if (u == maxXIdx) continue;
            if (edges.Contains(new UnorderedIndexPair(maxXIdx, u)))
            {
                Vector2 vu = newVerts[u].normalized;
                Vector2 vv = newVerts[maxXIdx].normalized;

                float angle = Vector2.SignedAngle(Vector2.up, vu-vv);
                angle = (angle < 0) ? angle + 360 : angle;

                if (angle < minCCWAngle)
                {
                    prevVert = u;
                    minCCWAngle = angle;
                }
            }
        }

        Vector2 prevEdge = newVerts[prevVert] - newVerts[maxXIdx];
        int prevV = prevVert;
        List<Vector2> retVerts = new List<Vector2>();
        retVerts.Add(newVerts[maxXIdx]);
        int currIdx = maxXIdx;
        int its = 0;
        do
        {
            List<UnorderedIndexPair> adjEdges = new List<UnorderedIndexPair>();

            int nextIdx = -1;
            float minClockwiseAngle = Mathf.Infinity;
            Vector2 nextPrevEdge = new Vector2();
            Debug.Log("Start Vertex: " + newVerts[currIdx]);
            Debug.Log("PrevEdge : " + prevEdge.normalized);
            for (int u = 0; u < numNewVerts; u++)
            {
                if (u == currIdx || u == prevV) continue;
                if (edges.Contains(new UnorderedIndexPair(currIdx, u)))
                {
                    Vector2 vu = newVerts[u];
                    Vector2 vv = newVerts[currIdx];

                    Vector2 currEdge = vu - vv;

                    currEdge.Normalize();
                    prevEdge.Normalize();

                    //float angle = Mathf.Rad2Deg*Mathf.Atan2(currEdge.x*prevEdge.y-currEdge.y*prevEdge.x,Vector2.Dot(currEdge, prevEdge));
                    float angle = -Vector2.SignedAngle(prevEdge, currEdge);
                    Debug.Log("EdgeV : " + vu);
                    Debug.Log("currEdge : " + currEdge);
                    Debug.Log("angle : " + angle);
                    Debug.Log("");
                    angle = (angle < 0) ? angle + 360 : angle;

                    if (angle < minClockwiseAngle && Mathf.Abs(angle) > 1e-3f )
                    {
                        nextPrevEdge = -currEdge;
                        minClockwiseAngle = angle;
                        nextIdx = u;
                    }
                }
            }
            prevV = currIdx;
            currIdx = nextIdx;
            prevEdge = nextPrevEdge;
            retVerts.Add(newVerts[currIdx]);
            its++;

        } while (currIdx != maxXIdx && its < 1000);

        return retVerts.ToArray();
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

    public static void RecreateMeshCollider(GameObject obj)
    {
        PolygonCollider2D polColl = obj.GetComponent<PolygonCollider2D>();
        EdgeCollider2D edgeColl = obj.GetComponent<EdgeCollider2D>();

        if (edgeColl == null && polColl == null) return;

        if (edgeColl == null)
        {
            DestroyImmediate(polColl);
        }
        else
        {
            DestroyImmediate(edgeColl);
        }

        obj.AddComponent<MeshCollider>();
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
        int min = first < second ? first : second;
        int max = first > second ? first : second;
        return (min << 16) + max;
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
