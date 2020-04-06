using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    // Fractal Brownian Motion
    public static float fBM(float x, float y, int oct, float persistance)
    {
        //If no min and max are created, then just discard them here
        float _min = float.MaxValue;
        float _max = float.MinValue;
        return fBM(x, y, oct, persistance, ref _min, ref _max);
    }
    public static float fBM(float x, float y, int oct, float persistance, ref float min, ref float max)
    {
        float noiseHeight = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;

        for (int i = 0; i < oct; i++)
        {
            float perlinValue = Mathf.PerlinNoise(x * frequency, y * frequency);
            noiseHeight += perlinValue * amplitude;
            maxValue += amplitude;
            amplitude *= persistance;
            frequency *= 2; //lacunarity
        }
        if (noiseHeight > max)
        {
            max = noiseHeight;
        }
        if (noiseHeight < min)
        {
            min = noiseHeight;
        }

        return noiseHeight / maxValue;
    }
    public static float Map(float value, float originalMin, float originalMax, float targetMin, float targetMax)
    {
        //return (value - originalMin) * (targetMax - targetMin) / (originalMax - originalMin) + targetMin;
        float t = Mathf.InverseLerp(originalMin, originalMax, value);
        return Mathf.Lerp(targetMin, targetMax, t);
    }


    /// <summary>
    /// This function will try to return a component of a given type from a gameobject. 
    /// If the component does not exist, it will instead add it.
    /// </summary>
    /// <typeparam name="T">The type of component you wish to find/add</typeparam>
    /// <param name="gameObject">The GameObject which should have the component</param>
    /// <returns> Unity Engine Component of Type T</returns>
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T:UnityEngine.Component
    {

        T component = gameObject.GetComponent<T>();
        if (!component)
        {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }

    public static List<Vector2> GetNeighbors(Vector2 pos, int width, int height)
    {
        List<Vector2> neighbors = new List<Vector2>();
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (!(x == 0 && y == 0)) //don't include current position, kernel is only focused on neighbors
                {
                    //Find neighbors, clamped within boundaries of image
                    Vector2 neighborPos = new Vector2(Mathf.Clamp(pos.x + x, 0, width - 1),
                                                        Mathf.Clamp(pos.y + y, 0, height - 1));

                    // If this is not already in the kernel, then add it
                    if (!neighbors.Contains(neighborPos))
                    {
                        neighbors.Add(neighborPos);
                    }
                }
            }
        }
        return neighbors;
    }

    public static Vector3 RandomVector()
    {
        return RandomVector(Vector3.zero, Vector3.one);
    }
    public static Vector3 RandomVector(Vector3 a, Vector3 b)
    {
        float x = UnityEngine.Random.Range(a.x, b.x);
        float y = UnityEngine.Random.Range(a.y, b.y);
        float z = UnityEngine.Random.Range(a.z, b.z);

        return new Vector3(x, y, z);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public static void NormalizeVector(ref float[] v) //try with / without ref
    {
        float total = 0;
        for (int i = 0; i < v.Length; i++)
        {
            total += v[i];
        }
        for (int i = 0; i < v.Length; i++)
        {
            v[i] /= total;
        }
    }

    // Fisher-Yates Shuffle
    public static System.Random r = new System.Random();
    public static void Shuffle<T>(this IList<T> list) //using this keyword makes this function extend
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = r.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}
