using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[CreateAssetMenu()]
public class TextureData : UpdateableData
{
    const int textureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;
    public Layer[] Layers;

    float savedMinHeight;
    float savedMaxHeight;

    public void ApplyToMaterial(Material material)
    {
        material.SetInt("layerCount", Layers.Length);
        material.SetColorArray("baseColors", Layers.Select(x=>x.Tint).ToArray());
        material.SetFloatArray("baseStartHeights", Layers.Select(x => x.StartHeight).ToArray());
        material.SetFloatArray("baseBlends", Layers.Select(x => x.BlendStrength).ToArray());
        material.SetFloatArray("baseColorStrengths", Layers.Select(x => x.TintStrength).ToArray());
        material.SetFloatArray("baseTextureScales", Layers.Select(x => x.TextureScale).ToArray());
        Texture2DArray texturesArray = GenerateTextureArray(Layers.Select(x => x.Texture).ToArray());
        material.SetTexture("baseTextures", texturesArray);

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

    Texture2DArray GenerateTextureArray(Texture2D[] textures)
    {
        Texture2DArray textureArray = new Texture2DArray(
            textureSize,
            textureSize,
            textures.Length,
            textureFormat,
            true
            );
        for (int i = 0; i < textures.Length; i++)
        {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }
        textureArray.Apply() ;
        return textureArray;
    }

    [System.Serializable]
    public class Layer
    {
        public Texture2D Texture;
        public Color Tint;
        [Range(0,1)]
        public float TintStrength;
        [Range(0, 1)]
        public float StartHeight;
        [Range(0, 1)]
        public float BlendStrength;
        public float TextureScale;
    }
}
