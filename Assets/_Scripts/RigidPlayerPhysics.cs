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

    // Friction applied horizontally when grounded
    public float kinematicFriction;




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

    // Is The Player Grounded
    private bool isGrounded = false;

    // Tracks whether the player has collided with anything since the last fixedUpdate step
    private bool hasCollided = false;

    // Used by grounding logic
    private bool groundedByCollisions = false;


    // Start is called before the first frame update
    void Start()
    {
        // Assume the player is a circle 
        radius = Mathf.Max(transform.localScale.x, transform.localScale.y) * 0.5f;
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
        if (!hasCollided)
        {
            isGrounded = false;
        }

        //ApplyFriction();
        MovementUpdate();
        ApplyGravity();

        hasCollided = false;
    }

    private void ApplyGravity()
    {
        AddForce(Vector2.down * baseGravity * gravityScale);
    }

    private void MovementUpdate()
    {
        v += f * Time.fixedDeltaTime / mass;

        v.x = StickyMath.MinAbs(v.x, Mathf.Sign(v.x)*maxHorizSpeed);
        v.x = (Mathf.Abs(v.x) < moveThreshold) ? 0 : v.x;
        
        transform.position += new Vector3(v.x, v.y) * Time.fixedDeltaTime;

        f = Vector2.zero;
    }

    public void AddForce( Vector2 addedF )
    {
        f += addedF;
    }

    public void ApplyImpulse(Vector2 velToAdd)
    {
        f += velToAdd * mass / Time.fixedDeltaTime;
    }

    private void DetectCollision()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, radius*.98f, v.normalized, 1e-2f, ~LayerMask.GetMask("Player"));
        hasCollided = true;

        groundedByCollisions = false;

        // Loop through the hits to resolve collisions, and check if any grounds the player
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider)
            {
                hasCollided = true;
                groundedByCollisions |= ResolveCollision(hit);
            }
        }

        // The above ensures grounding collision update isGrounded, but for collisions to still be accurate, the size of the circlecast must remain small
        // We want isGrounded to be more generous, while not affecting collisions, so we circleCast again with a larger sweep to make it more responsive.
        hits = Physics2D.CircleCastAll(transform.position, radius, Vector2.down, 1.0f, ~LayerMask.GetMask("Player"));
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider)
            {
                groundedByCollisions |= CheckCollisionForGrounding(hit);
            }
        }
        isGrounded = groundedByCollisions;
    }

    /** Returns whether this collision grounds the player*/
    private bool ResolveCollision(RaycastHit2D hit)
    {
        // Fix interpenetration with translation
        Vector2 penDepth = (Pos2D() - hit.point).normalized * radius;
        penDepth = penDepth - (Pos2D() - hit.point);
        if( penDepth.magnitude > 1e-2f)
            transform.Translate(penDepth);
        else if( hit.normal.normalized.y < 0)
            transform.Translate(penDepth);


        // Calculate Force acting on body from contact resisting current forces on body (N3)
        Vector2 nNorm = hit.normal.normalized;
        Vector2 df = Vector2.Dot(nNorm, f) * nNorm;
        f -= df;

        // Calculate Velocity Due to bounce
        Vector2 tNorm = new Vector2(-nNorm.y, nNorm.x);
        Vector2 velAlongNorm = -1 * Vector2.Dot(nNorm, v) * nNorm * restitution;
        Vector2 velAlongTan = Vector2.Dot(tNorm, v) * tNorm;
        Vector2 newVel = velAlongNorm + velAlongTan;
        Vector2 dv = newVel - v;

        // Calculate force required to created desired impulse over one fixedUpdate call
        dv = dv * mass / Time.fixedDeltaTime;
        f += dv;

        // Calculate Friction Force
        float fAlongNorm = Vector2.Dot(f, nNorm);
        float velTanDir = Vector2.Dot(v.normalized, tNorm);
        Vector2 frictionForce = -1 * velTanDir * tNorm * fAlongNorm * kinematicFriction;
        Vector2 velAfterFriction = velAlongTan + frictionForce * mass / Time.fixedDeltaTime;
        // If friction would accelerate object in direction opposite to current velocity along tangent, clamp force to bring object to rest
        if (!(StickyMath.InRange(Mathf.Sign(velTanDir) - Mathf.Sign(Vector2.Dot(velAfterFriction, tNorm)), -1e3f, 1e3f)))
        {
            frictionForce = -1 * velAlongTan * mass / Time.fixedDeltaTime;
        }
        f += frictionForce;
        

        // Check if this contact grounds the player
        return CheckCollisionForGrounding(hit);
            
    }

    private bool CheckCollisionForGrounding(RaycastHit2D hit)
    {

        float normAngle = Mathf.Acos(Vector2.Dot(Vector2.right, hit.normal.normalized)) * Mathf.Rad2Deg;
        return StickyMath.InRange(normAngle, 45, 135);
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

    public bool IsGrounded()
    {
        return isGrounded;
    }


}
