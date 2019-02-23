using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// Must be order so it runs before anything which changes forces/velocity/acceleration
public class RigidPlayerPhysics : MonoBehaviour
{
    // Threshold for horiz input to be considered
    [Range(1e-6f, 0.5f)]
    public float moveThreshold;

    // Velocity dropoff in air when acceleration is 0. Effect: (v.x = airSlowdown*v.x) every second, achieved with a force
    [Range(0.0f, 5.0f)]
    public float airSlowdown;

    public float maxOverallSpeed;

    public float maxHorizSpeed;

    public float gravityScale;

    // Margin allowed for collisions
    public float collisionMargin;

    // Friction applied horizontally when grounded
    public float kinematicFriction;

    // Damping Coefficient applied when friction would change velocity direction
    public float frictionDamping;

    // Minimum Time required between 'sticks'
    public float timeBetweenSticks; 

    [Range(1, 10)]
    public float mass;

    private const float baseGravity = 9.81f;
    // Bounce Resitition
    [Range(0, 1)]
    public float restitution;

    private float radius;

    // Velocity
    private Vector2 v = new Vector2();

    // Acceleration - Constant acceleration, reset every frame, used for input-based movement
    private Vector2 a = new Vector2();

    // Force Accumulator
    private Vector2 f = new Vector2();

    // Ground Tangent = move direction on input when grounded
    private Vector2 groundTangent = new Vector2();

    // Is The Player Grounded
    private bool isGrounded = false;

    // Is the player sticky
    private bool isSticky = false;

    // Is the player stuc to something
    private bool isStuck = false;

    private float lastUnstickTime;

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
        a = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (!hasCollided)
        {
            isGrounded = false;
        }

        //AirControlForceUpdate();
        AccelerationForceUpdate();
        MovementUpdate();
        ApplyGravity();

        hasCollided = false;
    }

    private void AirControlForceUpdate()
    {
        if (a.magnitude < 1e-3f && !isGrounded)
        {
            float deltaVx = - v.x * airSlowdown*Time.fixedDeltaTime;
            ApplyImpulse(Vector2.right * deltaVx);
        }
    }

    private void AccelerationForceUpdate()
    {
        Vector2 tangent = (isGrounded) ? groundTangent : Vector2.right;
        Vector2 velT = (Vector2.Dot(tangent, v)) * tangent;
        Vector2 accT = (Vector2.Dot(tangent, a) * Time.fixedDeltaTime) * tangent;

        if (velT.magnitude > maxHorizSpeed)
        {
            a = Vector2.zero;
        }
        else if((velT + accT).magnitude >= maxHorizSpeed)
        {
            a = tangent * Mathf.Sign((Vector2.Dot(tangent, v))) * (maxHorizSpeed - velT.magnitude) / Time.fixedDeltaTime;
        }
        f += mass * a;

    }

    private void ApplyGravity()
    {
        AddForce(Vector2.down * baseGravity * gravityScale);
    }

    private void MovementUpdate()
    {
        v += f * Time.fixedDeltaTime / mass;

        //v.x = StickyMath.MinAbs(v.x, Mathf.Sign(v.x)*maxHorizSpeed);
        v.x = (Mathf.Abs(v.x) < moveThreshold) ? 0 : v.x;

        v = (isStuck) ? Vector2.zero : v;

        transform.position += new Vector3(v.x, v.y) * Time.fixedDeltaTime;
        f = Vector2.zero;
    }

    public void AddForce( Vector2 addedF )
    {
        f += addedF;
    }

    public void Jump(Vector2 jumpForce)
    {
        if (isStuck)
        {
            setStuck(false);
            AddForce(jumpForce);
        }
        else if (isGrounded)
        {
            AddForce(jumpForce);
        }
    }

    public void SetAcceleration( Vector2 newAcc )
    {
        if (isGrounded)
        {
            if (Vector2.Dot(newAcc, groundTangent) > 0)
            {
                a = newAcc.magnitude * groundTangent;
            }
            else
            {
                a = -1 * newAcc.magnitude * groundTangent;
            }

        }
        else
        {
            a = newAcc;
        }
    }

    public void ApplyImpulse(Vector2 velToAdd)
    {
        f += velToAdd * mass / Time.fixedDeltaTime;
    }

    private void DetectCollision( Collider2D other )
    {
        hasCollided = true;
        groundedByCollisions = false;
        
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, radius*.98f, v.normalized, 1e-2f, ~LayerMask.GetMask("Player"));

        // Loop through the hits to resolve collisions, and check if any grounds the player
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider)
            {
                hasCollided = true;
                if (isSticky && Time.time - lastUnstickTime > timeBetweenSticks)
                {
                    groundedByCollisions |= ResolveCollisionSticky(hit);
                }
                else
                {
                    groundedByCollisions |= ResolveCollision(hit);
                }
            }
        }

        // The above ensures grounding collision update isGrounded, but for collisions to still be accurate, the size of the circlecast must remain small
        // We want isGrounded to be more generous, while not affecting collisions, so we circleCast again with a larger sweep to make it more responsive.
        // Also checks if object that triggers hit is intersecting the player, otherwise it will not ground
        hits = Physics2D.CircleCastAll(Pos2D(), radius, Vector2.down, 1f, ~LayerMask.GetMask("Player"));
        Collider2D[] colObjs = Physics2D.OverlapCircleAll(Pos2D(), radius*1.05f);
        foreach (RaycastHit2D hit in hits)
        {
            bool hitColIntersects = false;
            foreach (Collider2D colObj in colObjs) hitColIntersects |= hit.collider && hit.collider == colObj;
            if (hitColIntersects)
            {
                bool groundedByCollision = CheckCollisionForGrounding(hit);
                if (groundedByCollision)
                {
                    groundedByCollisions = true;
                    groundTangent = new Vector2(-hit.normal.y, hit.normal.x);
                }

            }
        }
        isGrounded = groundedByCollisions;
    }

    /** Returns whether this collision grounds the player*/
    private bool ResolveCollision(RaycastHit2D hit)
    {
        // Fix interpenetration with translation
        Vector2 hitCentroid = hit.centroid;

        Vector2 penDepth = (Pos2D() - hit.point).normalized * radius;
        penDepth = penDepth - (Pos2D() - hit.point);
        if (hit.normal.normalized.y < 0)
        {
            transform.Translate(penDepth);
        }
        else if (penDepth.magnitude > collisionMargin)
        {
            transform.Translate(penDepth.normalized * (penDepth.magnitude - collisionMargin));
        }
        else return true;


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

        Debug.Log(a.magnitude);
        // Calculate Friction Force
        if (a.magnitude < 1e-3f)
        {
            float fAlongNorm = Vector2.Dot(f, nNorm);
            float velTanDir = Vector2.Dot(v.normalized, tNorm);
            float nForceDueToGravity = -Vector2.Dot(nNorm, (gravityScale * baseGravity * mass) * Vector2.down);
            Vector2 frictionForce = -1 * velTanDir * tNorm * nForceDueToGravity * kinematicFriction;
            Vector2 velAfterFriction = velAlongTan + frictionForce * Time.fixedDeltaTime / mass;
            // If friction would accelerate object in direction opposite to current velocity along tangent, clamp force to bring object to rest
            if (!(StickyMath.InRange(Mathf.Sign(velTanDir) - Mathf.Sign(Vector2.Dot(velAfterFriction, tNorm)), -1e-3f, 1e-3f)))
            {
                frictionForce = -frictionDamping * velAlongTan * mass / Time.fixedDeltaTime;
            }
            f += frictionForce;
        }


        // Check if this contact grounds the player
        bool isGroundedByCollision = CheckCollisionForGrounding(hit);
        if (isGroundedByCollision)
        {
            groundTangent = new Vector2(-hit.normal.y, hit.normal.x);
        }
        return isGroundedByCollision;
            
    }

    private bool ResolveCollisionSticky(RaycastHit2D hit)
    {
        Vector2 penDepth = (Pos2D() - hit.point).normalized * radius;
        penDepth = penDepth - (Pos2D() - hit.point)*0.95f;
        if (penDepth.magnitude > 1e-2f)
            transform.Translate(penDepth);
        else if (hit.normal.normalized.y < 0)
            transform.Translate(penDepth);

        setStuck(true);
        lastUnstickTime = Time.time;
        
        return isStuck;

    }

    private bool CheckCollisionForGrounding(RaycastHit2D hit)
    {

        float normAngle = Mathf.Acos(Vector2.Dot(Vector2.right, hit.normal.normalized)) * Mathf.Rad2Deg;
        return StickyMath.InRange(normAngle, 45, 135);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DetectCollision(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        DetectCollision(other);
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

    public void setStuck(bool _isStuck)
    {
        bool wasStuck = isStuck;
        isStuck = _isStuck;
        if (!isStuck && wasStuck)
        {
            lastUnstickTime = Time.time;
        }
    }

    public void SetSticky(bool _isSticky)
    {
        isSticky = _isSticky;
        if (isSticky)
        {
            restitution = 0; //Stickiness overrides bounciness 
        }
        else
        {
            setStuck(false);
        }
    }
}
