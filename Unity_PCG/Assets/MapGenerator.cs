using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public DrawMode DrawMode;

    public int MapWidth;
    public int MapHeight;
    public float NoiseScale;

    public int Octaves;
    [Range(0, 1)]
    public float Persistance;
    public float Lacunarity;

    public int Seed;
    public Vector2 Offset;

    public float MeshHeightMultiplier;
    public AnimationCurve MeshHeightCurve;
    public bool AutoUpdate;

    public TerrainType[] Regions;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateMap(MapWidth, MapHeight, Seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);

        Color[] colorMap = new Color[MapHeight * MapWidth];
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < Regions.Length; i++)
                {
                    if (currentHeight <= Regions[i].Height)
                    {
                        colorMap[y * MapWidth + x] = Regions[i].Color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (DrawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightmap(noiseMap));
        }
        else if (DrawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, MapWidth, MapHeight));
        }
        else if (DrawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, MeshHeightMultiplier, MeshHeightCurve), TextureGenerator.TextureFromColorMap(colorMap, MapWidth, MapHeight));
        }
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

[System.Serializable]
public struct TerrainType
{
    public string Name;
    public float Height;
    public Color Color;
}

public enum DrawMode
{
    NoiseMap, ColorMap, Mesh
};