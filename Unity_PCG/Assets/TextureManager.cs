using MED10.Architecture.Events;
using MED10.Architecture.Variables;
using MED10.PCG;
using MED10.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
    public TerrainGenerator terrainGenerator;
    //public StringVariable directory;
    //public IntVariable seed;    
    public string directory;
    public int seed;
    
    public void SaveHeightmapTexture()
    {
        seed = terrainGenerator.Seed;
        if (directory.Length <= 0)
        {
            directory = "Heightmaps_";// + DateTime.Today.ToString();        
        }
        string filename = "/HM_"+ seed;
        Utils.BitmapToPNG(terrainGenerator.GetHeightMap(false), filename);
    }

}
