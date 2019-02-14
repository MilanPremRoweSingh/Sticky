using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyMath
{
    public static Color debugColor = new Color(255.0f / 255.0f, 140.0f / 255.0f, 0.0f);

    public static float MinAbs(float f0, float f1)
    {
        return Mathf.Abs(f0) <= Mathf.Abs(f1) ? f0 : f1;
    }

    public static bool InRange(float value, float min, float max)
    {
        return (value >= min && value <= max);
    }
}
