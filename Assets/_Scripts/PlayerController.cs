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

    // Threshold for horiz input to be considered
    [Range(1e-6f, 0.1f)]
    public float moveThreshold;

    // Bounce Resitition
    [Range(0, 1)]
    public float restitution;

    // Velocity
    private Vector2 v;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float xIn = Input.GetAxis("Horizontal");
        MoveOnInput(xIn);
        transform.Translate(v * Time.deltaTime);
    }

    void MoveOnInput( float xIn )
    {
        xIn = xIn > 0.25f ? Mathf.Sign(xIn) : 0;
        if (Mathf.Abs(xIn) > moveThreshold)
        {
            if (Mathf.Sign(xIn) == Mathf.Sign(v.x) || Mathf.Abs(v.x) < moveThreshold)
            {
                float vX = v.x;
                vX += xIn * startAcc * Time.time;
                Debug.Log( v.x + " += " + xIn + " * " + startAcc + " * " + Time.deltaTime);
                Debug.Log( "vX: " + vX );
                v.x = ClampAbs(vX, maxHorizSpeed);
            }
            else
            {
                v.x += xIn * turnDecc * Time.deltaTime;
            }
        }
        else
        {
            //v.x += -1 * Mathf.Sign(v.x) * stopDecc * Time.deltaTime;
           // v.x = (Mathf.Abs(v.x) < moveThreshold) ? 0 : v.x;
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
}
