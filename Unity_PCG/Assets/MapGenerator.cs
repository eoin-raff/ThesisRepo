using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int MapWidth;
    public int MapHeight;
    public float NoiseScale;

    public int Octaves;
    [Range(0, 1)]
    public float Persistance;
    public float Lacunarity;

    public int Seed;
    public Vector2 Offset;

    public bool AutoUpdate;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateMap(MapWidth, MapHeight, Seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
    }

    private void OnValidate()
    {
        if (MapWidth < 1)
        {
            MapWidth = 1;
        }
        if (MapHeight < 1)
        {
            MapHeight = 1;
        }
        if (Lacunarity < 1)
        {
            Lacunarity = 1;
        }
        if (Octaves < 0)
        {
            Octaves = 0;
        }

    }
}
