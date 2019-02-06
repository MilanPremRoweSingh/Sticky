using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    // Horizontal decelleration on input horiz input opposite current velocity
    public float turnDecc;

    // Horizontal decelleration on no input
    public float stopDecc;

    // Horizontal Accelleration on input
    public float startAcc;

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
        ImpulseMoveOnInput(xIn);
    }

    void ImpulseMoveOnInput(float xIn)
    {
        float moveThreshold = rpp.moveThreshold;

        Vector2 v = rpp.CurrentVel();
        Vector2 impulse = new Vector2();

        xIn = Mathf.Abs(xIn) > 0.25f ? Mathf.Sign(xIn) : 0;
        if (Mathf.Abs(xIn) > moveThreshold)
        {
            Debug.Log("Reading Input: " + xIn);
            if (Mathf.Sign(xIn) == Mathf.Sign(v.x) || Mathf.Abs(v.x) < moveThreshold)
            {
                impulse.x += xIn * startAcc * Time.deltaTime;
            }
            else
            {
                impulse.x += xIn * turnDecc * Time.deltaTime;
            }
        }
        else
        {
            impulse.x =  -StickyMath.MinAbs(v.x * stopDecc * Time.deltaTime, v.x);
        }

        rpp.ApplyImpulse(impulse);
    }

    void ForceMoveOnInput( float xIn )
    {

    }
}
