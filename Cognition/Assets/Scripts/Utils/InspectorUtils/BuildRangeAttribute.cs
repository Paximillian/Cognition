using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildRangeAttribute : PropertyAttribute
{
    public int Min { get; set; }
    public int Max { get; set; }

    public BuildRangeAttribute(int min, int max)
    {
        Min = min;
        Max = max;
    }
}
