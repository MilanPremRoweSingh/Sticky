using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle
{
    // Object Space 
    public Vector2 p;
    // Object Space
    public Vector2 v;
    // Object Space
    public Vector2 f;

    Particle next;
    Particle prev;

    public void AddForce( Vector2 force)
    {
        f += force;
    }

    public void ClearForce()
    {
        f = Vector2.zero;
    }

    public void SetPosition( Vector2 newP )
    {
        p = newP;
    }

}
