using UnityEngine;
using System;

[Serializable]
public class Splatmap : TerrainFeature
{
    public Texture2D texture = null;
    public Texture2D normalMap = null;
    public Vector2 tileOffset = Vector2.zero;
    public Vector2 tileSize = Vector2.one;
    public float blendOffset = 0.01f;
    public Vector2 blendNoiseScale = Vector2.one * 0.01f;
    public float blendNoiseScalar = 0.1f;
}

