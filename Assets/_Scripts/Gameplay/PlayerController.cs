using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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

    [HideInInspector]
    public bool attemptingPipeEnter;

    // Player physics script
    public RigidPlayerPhysics rpp;

    // Start is called before the first frame update
    void Start()
    {
        attemptingPipeEnter = false;
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
        float xInDir = Mathf.Sign(xIn) * Mathf.CeilToInt(Mathf.Abs(xIn));
        Vector2 acc = xInDir * startAcc * Vector2.right;
        rpp.SetAcceleration(acc);
        rpp.SetMaxHorizSpeedForFrame(Mathf.Abs(rpp.maxHorizSpeed * xIn));
    }

    public void EnterPipe(float yIn)
    {
        attemptingPipeEnter = yIn < -0.5f;
    }

    public void Jump()
    { 
        rpp.Jump(Vector2.up* jumpForce);
    }

    public void MakePlayerBouncy()
    {
        rpp.restitution = 1.0f;
    }

    public void MakePlayerUnbouncy()
    {
        rpp.restitution = 0.0f;
    }

    public void MakePlayerBouncyTrigger(float axisIn)
    {
        //if (axisIn < 0.1f) return;

        if (axisIn > 0.5f)
        {
            rpp.restitution = 1.0f;
        }
        else if (rpp.restitution > 1e-3f)
        {
            rpp.restitution = 0.0f;
        }
    }



    public void MakePlayerSticky()
    {
        rpp.SetSticky(true);
    }

    public void MakePlayerUnsticky()
    {
        rpp.SetSticky(false);
    }

    public void MakePlayerStickyTrigger(float axisIn)
    {
        //if (axisIn < 0.1f) return;

        if (axisIn > 0.5f)
        {
            rpp.SetSticky(true);
        }
        else if (rpp.IsSticky())
        {
            rpp.SetSticky(false);
        }
    }
}
