using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidPlayerPhysics : MonoBehaviour
{
    public float maxOverallSpeed;

    public float gravityScale;

    [Range(1, 10)]
    public float mass;

    private const float baseGravity = 9.81f;
    // Bounce Resitition
    [Range(0, 1)]
    public float restitution;


    private float radius;

    // Velocity
    private Vector2 v = new Vector3();

    // Force Accumulator
    private Vector2 f = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        // Assume the player is a circle 
        radius = Mathf.Max(transform.localScale.x, transform.localScale.y) * 0.5f;
    }

    private void FixedUpdate()
    {
        MovementUpdate();
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        AddForce(Vector2.down * baseGravity * gravityScale);
    }

    private void MovementUpdate()
    {
        v += f * Time.fixedDeltaTime / mass;
        transform.position += new Vector3(v.x, v.y) * Time.fixedDeltaTime;

        f = new Vector3();
    }

    public void AddForce( Vector2 addedF )
    {
        addedF.z = 0;
        f += addedF;
    }

    private void DetectCollision()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, radius, v.normalized, 1e-3f, ~LayerMask.GetMask("Player"));
        if (hit.collider)
        {
            ResolveCollision(hit);
        }

    }

    private void ResolveCollision(RaycastHit2D hit)
    {
        // Fix interpenetration with translation
        Vector2 penDepth = (Pos2D() - hit.point).normalized * radius;
        penDepth = penDepth - (Pos2D() - hit.point) ;
        transform.Translate(penDepth);

        // Calculate Velocity Due to bounce
        Vector2 nNorm = hit.normal.normalized;
        Vector2 vNorm = v.normalized;
        Vector2 r = vNorm - 2 * Vector2.Dot(vNorm, nNorm) * nNorm;
        r = r * v.magnitude * restitution;
        v = r;

        Vector2 dv = r - v;


        // Calculate Force acting on body from contact resisting current forces on body (N3)
        Vector2 df = f - Vector2.Dot(nNorm, f) * nNorm;
        f.x -= df.x;
        f.y -= df.y;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DetectCollision();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        DetectCollision();
    }

    public Vector2 Pos2D()
    {
        return new Vector2(transform.position.x, transform.position.y);
    }

}
