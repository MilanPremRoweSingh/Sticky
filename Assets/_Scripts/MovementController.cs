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
        float xIn = Input.GetAxis("Horizontal");
        ImpulseMoveOnInput(xIn);
    }

    void ImpulseMoveOnInput(float xIn)
    {
        Vector2 vCurr = rpp.CurrentVel();
        xIn = Mathf.Sign(xIn)*Mathf.CeilToInt(Mathf.Abs(xIn));
        Vector2 vNew = xIn * speed * Vector2.right;
        rpp.ApplyImpulse(vNew - vCurr);
    }

    void ForceMoveOnInput( float xIn )
    {

    }
}
