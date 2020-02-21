using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : ScriptableObject
{
    public float UniformScale = 2.5f;
    public bool UseFlatShading;
    public bool UseFalloff;

    public float MeshHeightMultiplier;
    public AnimationCurve MeshHeightCurve;
}
