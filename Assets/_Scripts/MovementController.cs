using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public float speed;

    // Horizontal decelleration on input horiz input opposite current velocity
    public float turnDecc;

    // Horizontal decelleration on no input
    public float stopDecc;

    // Horizontal move force on input
    public float startAcc;

    // Force Applied on jump
    public float jumpForce;

    RigidPlayerPhysics rpp;

    // Start is called before the first frame update
    void Start()
    {
        rpp = GetComponent<RigidPlayerPhysics>();
    }

    // Update is called once per frame
    void Update()
    {
        float xIn = Input.GetAxisRaw("Horizontal");
        //if (Mathf.Abs(xIn) >= 0.2f)
        {
            AccelerateOnInput(xIn);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Jump();
        }
    }

    void ImpulseMoveOnInput(float xIn)
    {
        Vector2 vCurr = rpp.CurrentVel();
        xIn = Mathf.Sign(xIn)*Mathf.CeilToInt(Mathf.Abs(xIn));
        Vector2 impulse = xIn * speed * Vector2.right - vCurr;
        impulse.y = 0;
        rpp.ApplyImpulse(impulse);
    }

    void AccelerateOnInput(float xIn)
    {
        xIn = Mathf.Sign(xIn) * Mathf.CeilToInt(Mathf.Abs(xIn));
        Vector2 acc = xIn * startAcc * Vector2.right;
        rpp.SetAcceleration(acc);
    }

    void Jump()
    { 
        if( rpp.IsGrounded() )
        {
            rpp.AddForce(Vector2.up* jumpForce);
        }
    }
}
