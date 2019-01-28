using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Min(0)]
    public float radius;
    [Range(3,1000)]
    public int numParticles;

    private List<Particle> particles;

    private Vector2 centre;

    private Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        particles = new List<Particle>(numParticles);

        GameObject[] editorMeshObjects = GameObject.FindGameObjectsWithTag("EditorMesh");
        foreach (GameObject obj in editorMeshObjects)
        {
            obj.GetComponent<MeshRenderer>().enabled = false;
        }
        centre = new Vector2( transform.position.x, transform.position.y );
        InitParticles();
    }

    // Update is called once per frame
    void Update()
    {
        UploadParticlesToMesh();
    }

    void FixedUpdate()
    {
        centre.x = transform.position.x;
        centre.y = transform.position.y;

    }

    void ArrangeParticlesInCircle()
    {
        float deltaTheta = Mathf.PI * 2.0f / numParticles;
        float theta = 0;
        for( int i = 0; i < particles.Count; i++)
        {
            particles[i].p.x = centre.x + radius * Mathf.Cos(theta);
            particles[i].p.y = centre.y + radius * Mathf.Sin(theta);
            theta += deltaTheta;
        }
    }

    void UploadParticlesToMesh()
    {
        int n = particles.Count;
        List<Vector3> verts = new List<Vector3>(n + 1);
        List<Vector3> normals = new List<Vector3>(n + 1);
        int[] tris = new int[3*n];

        for (int i = 0; i < n; i++)
        {
            verts.Add(new Vector3(particles[i].p.x, particles[i].p.y));
            normals.Add(new Vector3(0, 0, -1));

            tris[3 * i + 0] = n;
            tris[3 * i + 1] = i % n;
            tris[3 * i + 2] = (i + 1) % n;
        }
        verts.Add( new Vector3(centre.x, centre.y));
        normals.Add( new Vector3(0, 0, -1));
        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetIndices(tris, MeshTopology.Triangles,0);
    }

    void InitParticles()
    {
        float deltaTheta = Mathf.PI * 2.0f / numParticles;
        float theta = 0;
        for (int i = 0; i < numParticles; i++)
        {
            Particle p = new Particle();
            p.p.x = centre.x + radius * Mathf.Cos(theta);
            p.p.y = centre.y + radius * Mathf.Sin(theta);
            p.f = new Vector2();
            p.v = new Vector2();
            particles.Add(p);
            theta += deltaTheta;
        }
    }
}
