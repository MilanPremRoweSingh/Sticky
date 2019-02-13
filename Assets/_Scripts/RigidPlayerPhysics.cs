using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RigidPlayerPhysics : MonoBehaviour
{
    // Threshold for horiz input to be considered
    [Range(1e-6f, 0.5f)]
    public float moveThreshold;

    public float maxOverallSpeed;

    public float maxHorizSpeed;

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

    // Impulse Accumulator
    private Vector2 impulse = new Vector2();

    // Start is called before the first frame update
    void Start()
    {
        // Assume the player is a circle 
        radius = Mathf.Max(transform.localScale.x, transform.localScale.y) * 0.5f;
        lastUpdateTime = Time.time;
    }

    float lastUpdateTime;
    float deltaTime;

    private void Update()
    {
        /*
        deltaTime = Time.time - lastUpdateTime;
        deltaTime = Time.deltaTime;

        //ForceUpdate();
        //VelocityUpdate();
        //PositionUpdate();

        //transform.Translate(new Vector3(2.0f, 0.0f) * deltaTime);
        GetComponent<Rigidbody2D>().velocity = new Vector3(1.0f, 0.0f);

        lastUpdateTime = Time.time;
        */    
    }

    private void FixedUpdate()
    {
        deltaTime = Time.fixedDeltaTime;

        ForceUpdate();
        VelocityUpdate();
        PositionUpdate();
        ApplyGravity();
        //lastUpdateTime = Time.time;
    }

    private void ApplyGravity()
    {
        AddForce(Vector2.down * baseGravity * gravityScale);
    }

    private void ForceUpdate()
    {
        if (deltaTime != 0)
        {
            f += impulse * mass / deltaTime;
            impulse = Vector2.zero;
        }
    }

    private void VelocityUpdate()
    {
        if (deltaTime != 0)
        {
            v += f * (Time.time - lastUpdateTime) / mass;

            v.x = StickyMath.MinAbs(v.x, Mathf.Sign(v.x) * maxHorizSpeed);
            v.x = (Mathf.Abs(v.x) < moveThreshold) ? 0 : v.x;

            f = Vector2.zero;
        }
    }

    private void PositionUpdate()
    {
        transform.position += new Vector3(v.x, v.y) * deltaTime;
    }

    public void AddForce( Vector2 addedF )
    {
        f += addedF;
    }

    public void ApplyImpulse(Vector2 velToAdd)
    {
        impulse += velToAdd;
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
        penDepth = penDepth - (Pos2D() - hit.point);
        transform.Translate(penDepth);

        // Calculate Velocity Due to bounce
        Vector2 nNorm = hit.normal.normalized;
        Vector2 vNorm = v.normalized;
        Vector2 rNorm = vNorm - 2 * Vector2.Dot(vNorm, nNorm) * nNorm;
        rNorm.Normalize();
        Vector2 r = rNorm * v.magnitude * restitution;

        // Calculate Force acting on body from contact resisting current forces on body (N3)
        Vector2 df = f - Vector2.Dot(nNorm, f) * nNorm;
        f -= df;

        // Calculate force required to created desired impulse over one fixedUpdate call
        Vector2 dv = r - v;
        float forceMagForImpulse = dv.magnitude * mass / (Time.time - lastUpdateTime);
        f += forceMagForImpulse * rNorm; 
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

    public Vector2 CurrentVel()
    {
        return v;
    }

}
