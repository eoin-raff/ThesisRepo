using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    // Fractal Brownian Motion
    public static float fBM(float x, float y, int oct, float persistance)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        for (int i = 0; i < oct; i++)
        {
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistance;
            frequency *= 2; //lacunarity
        }

        return total / maxValue;
    }
    public static float Map(float value, float originalMin, float originalMax, float targetMin, float targetMax)
    {
        //return (value - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
        float t = Mathf.InverseLerp(originalMin, originalMax, value);
        return Mathf.Lerp(targetMin, targetMax, t);
    }

}
