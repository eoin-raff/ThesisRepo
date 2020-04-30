using System.Collections;
using MED10.PCG;
using MED10.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public GameObject PlayerPrefab;

    public void FindShorePosition(TerrainGenerator terrain)
    {

        //float[,] heightMap = terrain.GetHeightMap(false);
        //int resolution = terrain.terrainManager.HeightmapResolution;
        //for (int y = 0; y < resolution; y++)
        //{
        //    for (int x = 0; x < resolution; x++)
        //    {
        //        Vector2 location = new Vector2(x, y);
        //        List<Vector2> neighbors = Utils.GetNeighbors(location, resolution, resolution);
        //        foreach (Vector2 n in neighbors)
        //        {
        //            if (heightMap[x, y] < terrain.waterHeight && heightMap[(int)n.x, (int)n.y] > terrain.waterHeight)
        //            {
        //                float posX = Utils.Map( x, 0, resolution, 0, terrain.terrainData.size.x);
        //                float posY = heightMap[x, y] * terrain.terrainData.size.y;
        //                float posZ = Utils.Map( y, 0, resolution, 0, terrain.terrainData.size.z);

        //                Vector3 position = new Vector3(posX, posY, posZ);

        //                ////Find good position
        //                Instantiate(PlayerPrefab, position, Quaternion.identity);
        //                break;
        //            }
        //        }
        //    }
        //}
        //terrainManager.SetHeightmap(heightMap)

    }
}
