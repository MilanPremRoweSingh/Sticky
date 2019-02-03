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
    private Vector3 v = new Vector3();

    // Force Accumulator
    private Vector3 f = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        // Assume the player is a circle 
        radius = Mathf.Max(transform.localScale.x, transform.localScale.y) * 0.5f;
    }

    private void FixedUpdate()
    {
        ApplyGravity();
        MovementUpdate();
    }

    private void ApplyGravity()
    {
        AddForce(Vector3.down * baseGravity * gravityScale);
    }

    private void MovementUpdate()
    {
        v += f * Time.fixedDeltaTime / mass;
        transform.position += v * Time.fixedDeltaTime;

        f = new Vector3();
    }

    public void AddForce( Vector3 addedF )
    {
        addedF.z = 0;
        f += addedF;
    }

    private void DetectCollision()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, radius, v.normalized, 1e-3f, LayerMask.GetMask("Player"));
        if (hit.collider)
        {
            ResolveCollision(hit);
        }

    }

    private void ResolveCollision(RaycastHit2D hit)
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DetectCollision();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
    }
}
