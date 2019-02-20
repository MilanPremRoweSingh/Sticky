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

    // Player physics script
    public RigidPlayerPhysics rpp;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void ImpulseMoveOnInput(float xIn)
    {
        Vector2 vCurr = rpp.CurrentVel();
        xIn = Mathf.Sign(xIn)*Mathf.CeilToInt(Mathf.Abs(xIn));
        Vector2 impulse = xIn * speed * Vector2.right - vCurr;
        impulse.y = 0.0f;
        rpp.ApplyImpulse(impulse);
    }

    public void AccelerateOnInput(float xIn)
    {
        xIn = Mathf.Sign(xIn) * Mathf.CeilToInt(Mathf.Abs(xIn));
        Vector2 acc = xIn * startAcc * Vector2.right;
        rpp.SetAcceleration(acc);
    }

    public void Jump()
    { 
        if( rpp.IsGrounded() )
        {
            rpp.AddForce(Vector2.up* jumpForce);
        }
    }

    public void MakePlayerBouncy()
    {
        rpp.restitution = 1.0f;
    }

    public void MakePlayerUnbouncy()
    {
        rpp.restitution = 0.0f;
    }

    public void MakePlayerSticky()
    {
        rpp.SetSticky(true);
    }

    public void MakePlayerUnsticky()
    {
        rpp.SetSticky(false);
    }
}
