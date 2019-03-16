using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderBuilder : MonoBehaviour
{
    public string tagForGeneration;
    public static float vectorEqThreshold;
    public bool drawProjectedMesh;

    public static List<List<Vector3>> debugEdges = new List<List<Vector3>>();

    public void GenerateLevelColliders()
    {
        foreach (Transform child in transform)
        {
            GameObject childObj = child.gameObject;
            Collider2D coll2D = Generate2DCollidersForObject(childObj);


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


    public void ClearDebugShapes()
    {
        debugEdges.Clear();
    }

    public static Collider2D Generate2DCollidersForObject(GameObject obj)
    {
        MeshCollider prevColl = obj.GetComponent<MeshCollider>();
        if (prevColl == null || prevColl.sharedMesh == null) return null;


        Collider2D prevColl2D = obj.GetComponent<Collider2D>();
        if (prevColl2D != null) DestroyImmediate(prevColl2D);


        Mesh mesh = prevColl.sharedMesh;
        List<Vector2> verts = new List<Vector2>(ProjectAndWeld(obj, mesh, 1e-3f));

        List<List<Vector2>> convexParts = BayazitDecomposer.ConvexPartition(verts);

        DestroyImmediate(prevColl);

        foreach (Transform child in obj.transform)
        {
            if (child.gameObject.name == obj.name + "_colParent")
            {
                foreach (Transform colChild in child)
                {
                    DestroyImmediate(colChild.gameObject);
                }
                DestroyImmediate(child.gameObject);
            }
        }
        int partNum = 0;

        GameObject colParent = new GameObject(obj.name + "_colParent");
        colParent.transform.SetParent(obj.transform);
        foreach (List<Vector2> part in convexParts)
        {
            GameObject dummyCol = new GameObject(obj.name + "_colPart_" + partNum);
            
            dummyCol.transform.SetParent(colParent.transform);
            dummyCol.transform.localScale = obj.transform.localScale;
            dummyCol.transform.SetPositionAndRotation(obj.transform.position, obj.transform.rotation);
            PolygonCollider2D coll = dummyCol.AddComponent<PolygonCollider2D>();
            coll.points = part.ToArray();

            partNum++;
        }

        return null;
    }

    // Only works for meshes with a single submesh
    public static Vector2[] ProjectAndWeld(GameObject obj, Mesh mesh, float threshold)
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
                newVertIndices[i] = numNewVerts;
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

        List<Vector3> projectedEdges = new List<Vector3>();
        foreach (UnorderedIndexPair edge in edges)
        {
            Vector3 u = obj.transform.TransformVector(newVerts[edge.first]);
            u += obj.transform.position;
            Vector3 v = obj.transform.TransformVector(newVerts[edge.second]);
            v += obj.transform.position;
            projectedEdges.Add(u);
            projectedEdges.Add(v);
        }
        debugEdges.Add(projectedEdges);
        
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
;
                    float angle = -Vector2.SignedAngle(prevEdge, currEdge);
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

        } while (currIdx != maxXIdx && its < 100000);

        return retVerts.ToArray();
    }

    public static void RecreateMeshCollider(GameObject obj)
    {
        PolygonCollider2D polColl = obj.GetComponent<PolygonCollider2D>();
        EdgeCollider2D edgeColl = obj.GetComponent<EdgeCollider2D>();

        if (polColl != null)
        {
            DestroyImmediate(polColl);
        }
        else if (edgeColl != null)
        {
            DestroyImmediate(edgeColl);
        }

        foreach (Transform child in obj.transform)
        {
            if (child.gameObject.name == obj.name + "_colParent")
            {
                foreach (Transform colChild in child)
                {
                    DestroyImmediate(colChild.gameObject);
                }
                DestroyImmediate(child.gameObject);
            }
        }

        MeshCollider meshColl = obj.GetComponent<MeshCollider>();

        if (meshColl == null)
        {
            obj.AddComponent<MeshCollider>();
        }
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
