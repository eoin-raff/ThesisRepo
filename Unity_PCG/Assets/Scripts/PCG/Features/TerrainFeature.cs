using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TerrainFeature
{
    public bool remove = false;

    public float minHeight = 0.0f;
    public float maxHeight = 1.0f;
    public float minSlope = 0;
    public float maxSlope = 90f;

}
