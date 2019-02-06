using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyMath
{
    public static float MinAbs(float f0, float f1)
    {
        return Mathf.Min(Mathf.Abs(f0), Mathf.Abs(f1));
    }
}
