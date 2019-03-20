using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public RigidPlayerPhysics target;

    public float smoothTime;

    private Vector3 v = new Vector3();

    private void FixedUpdate()
    {
        /*
        Vector3 tPos = target.transform.position;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, tPos, ref v, smoothTime);
        newPos.z = transform.position.z;
        v.z = 0;
        transform.position = newPos;
        */
    }

    private void Update()
    {

    }
}
