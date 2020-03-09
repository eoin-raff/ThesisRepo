using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    private const float MaxHeightReduction = 1.9f;

    public enum NormalizeMode
    {
        Local,
        Global
    }
    public enum OffsetMode
    {
        Fixed,
        Rolling
    }
    //Overload methods to make Offset & Normalize modes optional parameters (defaults to NormalizeMode.Global and OffsetMode.Fixed)
    public static float[,] GenerateMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        return GenerateMap(mapWidth, mapHeight, seed, scale, octaves, persistance, lacunarity, offset, NormalizeMode.Global, OffsetMode.Fixed);
    }
    public static float[,] GenerateMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, OffsetMode offsetMode)
    {
        return GenerateMap(mapWidth, mapHeight, seed, scale, octaves, persistance, lacunarity, offset, NormalizeMode.Global, offsetMode);
    }
    public static float[,] GenerateMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        return GenerateMap(mapWidth, mapHeight, seed, scale, octaves, persistance, lacunarity, offset, normalizeMode, OffsetMode.Fixed);
    }
    public static float[,] GenerateMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode, OffsetMode offsetMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float frequency = 1f;
        float amplitude = 1f;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0f)
        {
            scale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2;
        float halfHeight = mapHeight / 2;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                frequency = 1f;
                amplitude = 1f;
                float noiseHeight = 0f;

                for (int i = 0; i < octaves; i++)
                {
                    // Applying the offsets at the end causes overall shape to change when adjusting offset.
                    // This is a good effect for clouds, but not for terrain.
                    float sampleX = 0;
                    float sampleY = 0;
                    if (offsetMode == OffsetMode.Fixed)
                    {
                        sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                        sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;
                    }
                    else //Rolling offset (good for clouds, maybe?)
                    {
                        sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                        sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
                    }


                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight/ MaxHeightReduction); //Magic Number is an estimate to reduce MaxPossibleHeight to a more likely to occur value
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseMap;
    }
}
