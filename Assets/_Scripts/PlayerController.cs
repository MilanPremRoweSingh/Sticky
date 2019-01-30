using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float maxHorizSpeed;

    // Horizontal decelleration on input horiz input opposite current velocity
    public float turnDecc;

    // Horizontal decelleration on no input
    public float stopDecc;

    // Horizontal Accelleration on input
    public float startAcc;

    // Upwards force applied on jump
    public float jumpForce;

    // Threshold for horiz input to be considered
    [Range(0.05f, 0.5f)]
    public float moveThreshold;

    // Bounce Resitition
    [Range(0, 1)]
    public float restitution;

    // Min time between sticky jumps
    public float timeBetweenStickyJumps;

    // Velocity
    private Vector2 v;

    // Maximum distance for a contact to be 'stickable'
    public float stickDistance;


    private Rigidbody2D rb;

    private bool bouncy;
    private bool sticky;
    private bool canJump;

    public float radius;

    const RigidbodyConstraints2D rbStuck = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
    const RigidbodyConstraints2D rbCanMove = RigidbodyConstraints2D.FreezeRotation;

    private float prevTimeStickyJump;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        prevTimeStickyJump = Time.time;

    }

    // Update is called once per frame
    void Update()
    {
        UpdateScaleFromRadius();

        float xIn = Input.GetAxisRaw("Horizontal");
        MoveOnInput(xIn);
        // TODO: Replace to particle-wise application once CD is done
        v.y = rb.velocity.y;
        rb.velocity = v;

        if (Input.GetKeyUp(KeyCode.Space)) Jump();

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            GetComponent<CircleCollider2D>().isTrigger = true;
            bouncy = true;
            sticky = false;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            GetComponent<CircleCollider2D>().isTrigger = false;
            bouncy = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            sticky = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            GetComponent<CircleCollider2D>().isTrigger = false;
            sticky = false;
            rb.constraints = rbCanMove;
        }

    }

    void MoveOnInput(float xIn)
    {
        xIn = Mathf.Abs(xIn) > 0.25f ? Mathf.Sign(xIn) : 0;
        if (Mathf.Abs(xIn) > moveThreshold)
        {
            if (Mathf.Sign(xIn) == Mathf.Sign(v.x) || Mathf.Abs(v.x) < moveThreshold)
            {
                float vX = v.x;
                vX += xIn * startAcc * Time.deltaTime;
                v.x = ClampAbs(vX, maxHorizSpeed);
            }
            else
            {
                v.x += xIn * turnDecc * Time.deltaTime;
            }
        }
        else
        {
            v.x = (Mathf.Abs(v.x) < moveThreshold) ? 0 : v.x;
            if (Mathf.Abs(v.x) < moveThreshold)
            {
                v.x = 0;
            }
            else
            {
                v.x += -1 * Mathf.Sign(v.x) * stopDecc * Time.deltaTime;
            }
        }
    }

    void Jump()
    {
        // TODO: Replace with own Force Accumulator after CD is done
        float timeSinceStickyJump = Time.time - prevTimeStickyJump;

        if (sticky && rb.constraints == rbStuck && timeSinceStickyJump > timeBetweenStickyJumps)
        {
            rb.constraints = rbCanMove;
            rb.AddForce(Vector2.up * jumpForce);
            canJump = false;
            prevTimeStickyJump = Time.time;
        }
        else if (canJump)
        {
            rb.AddForce(Vector2.up* jumpForce);
            canJump = false;
        }
    }

    float MaxAbs( float f0, float f1 )
    {
        return (Mathf.Abs(f0) >= Mathf.Abs(f1)) ? f0 : f1;
    }

    float ClampAbs( float val, float maxVal)
    {
        return Mathf.Abs(val) > Mathf.Abs(maxVal) ? Mathf.Sign(val) * Mathf.Abs(maxVal) : val;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ContactPoint2D[] contacts = new ContactPoint2D[collision.contactCount];
        collision.GetContacts(contacts);
        float minContactDist = Mathf.Infinity;
        foreach (ContactPoint2D contact in contacts)
        {
            float surfAng = Mathf.Acos(Vector2.Dot(contact.normal, Vector2.up))* Mathf.Rad2Deg;
            if (Mathf.Abs(surfAng) <= 75)
            {
                canJump = true;
            }

            float contactDist = (contact.point - Pos2D()).magnitude;
            minContactDist = (minContactDist > contactDist) ? contactDist : minContactDist;
        }

        if (sticky && minContactDist < stickDistance + radius)
        {
            rb.constraints = rbStuck;
            rb.velocity = new Vector2();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (bouncy)
        {
            if (other.tag == "Level")
            {
                RaycastHit2D contact = Physics2D.CircleCast(transform.position, radius, rb.velocity, 1.0f, ~LayerMask.GetMask("Player"));
                if (contact.collider != null)
                {
                    float vMag = rb.velocity.magnitude;
                    if (vMag > moveThreshold)
                    {
                        Vector2 nNorm = contact.normal.normalized;
                        Vector2 vNorm = rb.velocity.normalized;
                        Vector2 r = vNorm - 2 * Vector2.Dot(vNorm, nNorm) * nNorm;
                        r = r * vMag;
                        v = r;
                        rb.velocity = r;
                    }
                }
            }
        }
    }
    private void OnTriggerStay2D(Collider2D other) {
        if (other.tag == "Level")
        {
            StillCollisionResolution();
        }
    }

    private void UpdateScaleFromRadius()
    {
        float diameter = 2 * radius;
        transform.localScale = new Vector3(diameter, diameter, diameter);
    }

    void StillCollisionResolution()
    {
        RaycastHit2D contact = Physics2D.CircleCast(transform.position, transform.lossyScale.x * 0.5f, rb.velocity, 10.0f, ~LayerMask.GetMask("Player"));
        if (contact.collider != null)
        {
            Vector2 toContact = new Vector2(transform.position.x, transform.position.y) - contact.point;
            if (toContact.sqrMagnitude < radius * radius)
            {
                transform.position = contact.point + toContact.normalized * radius;
                Vector2 normalPerp = new Vector2(-contact.normal.y, contact.normal.x);
                float vAlongNormPerp = Vector2.Dot(normalPerp, v);
                v = normalPerp * vAlongNormPerp;
                rb.velocity = v;
            }
        }
    }

    void BounceCollisionResolution()
    {

    }

    Vector2 Pos2D()
    {
        return new Vector2(transform.position.x, transform.position.y);
    }
}
