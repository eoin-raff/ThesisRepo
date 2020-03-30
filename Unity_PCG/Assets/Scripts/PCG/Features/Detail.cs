using UnityEngine;


[System.Serializable]
public class Detail : TerrainFeature
{
    public GameObject prototype = null;
    public Texture2D prototypeTexture = null;

    public Color dryColor = Color.white;
    public Color healthyColor = Color.white;
    public Vector2 heightRange = Vector2.one;
    public Vector2 widthRange = Vector2.one;
    public float noiseSpread = 0.5f;

    public float overlap = 0.01f;   //overlap in height layers
    public float feather = 0.05f;   //perlin noise strenth at edges of height layers
    public float density = 0.5f;
}

