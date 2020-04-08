using System;
using UnityEngine;

[Obsolete()]
public static class HeightMapGenerator
{
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre)
    {
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.NoiseSettings, sampleCentre);

        // Animation curves will return incorrect values if accessed from different places simultaniously.
        // Since this may be called from multiple threads at the same time, we need to make a threadsafe copy of the height curve
        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.HeightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]) * settings.HeightMultiplier;

                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }

        }
        return new HeightMap(values, minValue, maxValue);
    }

}

[Obsolete()]
public struct HeightMap
{
    public readonly float[,] Values;
    public readonly float MinValue;
    public readonly float MaxValue;

    public HeightMap(float[,] Values, float MinValue, float MaxValue)
    {
        this.Values = Values;
        this.MinValue = MinValue;
        this.MaxValue = MaxValue;
    }
}