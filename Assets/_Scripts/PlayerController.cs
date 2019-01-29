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
    
    public float jumpForce;

    // Threshold for horiz input to be considered
    [Range(0.05f, 0.5f)]
    public float moveThreshold;

    // Bounce Resitition
    [Range(0, 1)]
    public float restitution;

    // Velocity
    private Vector2 v;
    private Rigidbody2D rb;

    private bool bouncy;

    public float radius;




    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();


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
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            GetComponent<CircleCollider2D>().isTrigger = false;
            bouncy = false;
        }
    }

    void MoveOnInput( float xIn )
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
        rb.AddForce(Vector2.up * jumpForce);
    }

    float MaxAbs( float f0, float f1 )
    {
        return (Mathf.Abs(f0) >= Mathf.Abs(f1)) ? f0 : f1;
    }

    float ClampAbs( float val, float maxVal)
    {
        return Mathf.Abs(val) > Mathf.Abs(maxVal) ? Mathf.Sign(val) * Mathf.Abs(maxVal) : val;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (bouncy)
        {
            if (other.tag == "Level")
            {
                // TODO: Generalise CircleCast radius
                RaycastHit2D contact = Physics2D.CircleCast(transform.position, transform.lossyScale.x * 0.5f, rb.velocity, 10.0f, ~LayerMask.GetMask("Player"));
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
            RaycastHit2D contact = Physics2D.CircleCast(transform.position, transform.lossyScale.x * 0.5f, rb.velocity, 10.0f, ~LayerMask.GetMask("Player"));
            if (contact.collider != null)
            {
                Vector2 toContact = new Vector2(transform.position.x, transform.position.y) - contact.point;
                if (toContact.sqrMagnitude < radius * radius) {
                    transform.position = contact.point + toContact.normalized * radius;
                    Vector2 normalPerp = new Vector2(-contact.normal.y, contact.normal.x);
                    float vAlongNormPerp = Vector2.Dot(normalPerp, v);
                    v = normalPerp * vAlongNormPerp;
                    rb.velocity = v;
                }
            }
        }
    }

    private void UpdateScaleFromRadius()
    {
        float diameter = 2 * radius;
        transform.localScale = new Vector3(diameter, diameter, diameter);
    }
}
