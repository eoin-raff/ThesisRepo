using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TextureData : UpdateableData
{
    public Color[] BaseColors;
    [Range(0, 1)]
    public float[] BaseStartHeights;

    float savedMinHeight;
    float savedMaxHeight;

    public void ApplyToMaterial(Material material)
    {
        material.SetInt("baseColorCount", BaseColors.Length);
        material.SetColorArray("baseColors", BaseColors);
        material.SetFloatArray("baseStartHeights", BaseStartHeights);
        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        //material.SetInt("baseColorCount", BaseColors.Length);
        //material.SetColorArray("baseColors", BaseColors);
        //material.SetFloatArray("baseStartHeights", BaseStartHeights);

        savedMaxHeight = maxHeight;
        savedMinHeight = minHeight;
        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }
}
