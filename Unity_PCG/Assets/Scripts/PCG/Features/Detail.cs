using UnityEngine;


[System.Serializable]
public class Detail : TerrainFeature
{
    public GameObject prototype = null;
    public Texture2D prototypeTexture = null;

    public float overlap = 0.01f;   //overlap in height layers
    public float feather = 0.05f;   //perlin noise strenth at edges of height layers
    public float density = 0.5f;
}

