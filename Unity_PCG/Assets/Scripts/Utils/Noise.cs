using System;
using UnityEngine;

namespace MED10.Utilities
{
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
        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre)
        {
            float[,] noiseMap = new float[mapWidth, mapHeight];

            System.Random prng = new System.Random(settings.Seed);
            Vector2[] octaveOffsets = new Vector2[settings.Octaves];

            float maxPossibleHeight = 0;
            float frequency = 1f;
            float amplitude = 1f;

            for (int i = 0; i < settings.Octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + settings.Offset.x + sampleCentre.x;
                float offsetY = prng.Next(-100000, 100000) - settings.Offset.y - sampleCentre.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= settings.Persistance;
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

                    for (int i = 0; i < settings.Octaves; i++)
                    {
                        // Applying the offsets at the end causes overall shape to change when adjusting offset.
                        // This is a good effect for clouds, but not for terrain.
                        float sampleX = 0;
                        float sampleY = 0;
                        if (settings.OffsetMode == OffsetMode.Fixed)
                        {
                            sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.Scale * frequency;
                            sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.Scale * frequency;
                        }
                        else //Rolling offset (good for clouds, maybe?)
                        {
                            sampleX = (x - halfWidth) / settings.Scale * frequency + octaveOffsets[i].x;
                            sampleY = (y - halfHeight) / settings.Scale * frequency + octaveOffsets[i].y;
                        }


                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                        noiseHeight += perlinValue * amplitude;

                        amplitude *= settings.Persistance;
                        frequency *= settings.Lacunarity;
                    }

                    if (noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }

                    if (noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }
                    noiseMap[x, y] = noiseHeight;
                    if (settings.NormalizeMode == NormalizeMode.Global)
                    {
                        float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / MaxHeightReduction); //Magic Number is an estimate to reduce MaxPossibleHeight to a more likely to occur value
                        noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                    }
                }
            }
            if (settings.NormalizeMode == NormalizeMode.Local)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    for (int x = 0; x < mapWidth; x++)
                    {
                        noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                    }
                }
            }

            return noiseMap;
        }
    }

    [System.Serializable]
    public class NoiseSettings
    {
        public Noise.NormalizeMode NormalizeMode;
        public Noise.OffsetMode OffsetMode;
        public float Scale = 50;

        public int Octaves = 6;
        [Range(0, 1)]
        public float Persistance = 0.6f;
        public float Lacunarity = 2;

        public int Seed;
        public Vector2 Offset;

        public void ValidateValues()
        {
            Scale = Mathf.Max(Scale, 0.01f);
            Octaves = Mathf.Max(Octaves, 1);
            Lacunarity = Mathf.Max(Lacunarity, 1f);
            Persistance = Mathf.Clamp01(Persistance);

        }
    } 
}
