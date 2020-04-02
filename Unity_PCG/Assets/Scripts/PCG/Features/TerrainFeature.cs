using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TerrainFeature : TableItem
{
    public float minHeight = 0.0f;
    public float maxHeight = 1.0f;
    public float minSlope = 0;
    public float maxSlope = 90f;

}

public abstract class TableItem
{
    public bool remove = false;
}
