using UnityEngine;

[System.Serializable]
public class Vegetation : TerrainFeature
{
    public GameObject mesh = null;
    public float pivotOffset = 0.0f;

    public float minScale = 0.7f;
    public float maxScale = 1.0f;
    public Color color1 = Color.white;
    public Color color2 = Color.white;
    public Color lightColor = Color.white;
    public float minRotation = 0;
    public float maxRotation = 360;
    public float density = 0.5f;
}

