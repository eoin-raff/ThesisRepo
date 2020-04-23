using MED10.PCG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator : MonoBehaviour
{
    public TerrainGenerator terrainGenerator;
    public RuntimeGeneration runtimeGenerator;
    public TextureManager textureManager;

    public int N;
    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        terrainGenerator.seedType = TerrainGenerator.SeedType.Fixed;
        for (int i = 0; i < N; i++)
        {
            Debug.Log("Generation " + i);
            terrainGenerator.Seed = i;
            runtimeGenerator.Generate();
            textureManager.SaveHeightmapTexture();
        }
    }

}
