using System.Collections;
using MED10.PCG;
using MED10.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public Camera playerCam;
    public float targetDistance = 100;


    [SerializeField]
    private TerrainManager terrainManager;    
    [SerializeField]
    private NarrativeManager narrativeManager;
    public float weenieHeight;

    public void FindShorePosition()
    {
        float[,] heightMap = terrainManager.GetHeightmap(false);
        int resolution = terrainManager.HeightmapResolution;
        float waterHeight = terrainManager.GetPainter().waterHeight;
        float highestValue = float.MinValue;
        Vector2 highestPoint = Vector2.zero;
        List<Vector3> shorePositions = new List<Vector3>();
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                #region Find Highest Point
                if (heightMap[x, y] > highestValue)
                {
                    highestValue = heightMap[x, y];
                    highestPoint = new Vector2(x, y);
                }
                #endregion
                #region Find Shoreline
                Vector2 location = new Vector2(x, y);
                List<Vector2> neighbors = Utils.GetNeighbors(location, resolution, resolution);
                foreach (Vector2 n in neighbors)
                {
                    if (heightMap[x, y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)
                    {
                        float posX = Utils.Map(x, 0, resolution, 0, terrainManager.TerrainData.size.x);
                        float posY = heightMap[x, y] * terrainManager.TerrainData.size.y;
                        float posZ = Utils.Map(y, 0, resolution, 0, terrainManager.TerrainData.size.z);

                        Vector3 position = new Vector3(posZ, posY, posX);
                        shorePositions.Add(position);
                        //////Find good position
                        //Instantiate(PlayerPrefab, position, Quaternion.identity);
                        //break;
                    }
                } 
                #endregion
            }
        }

        float posx = Utils.Map(highestPoint.x, 0, resolution, 0, terrainManager.TerrainData.size.x);
        float posy = heightMap[(int)highestPoint.x, (int)highestPoint.y] * terrainManager.TerrainData.size.y;
        float posz = Utils.Map(highestPoint.y, 0, resolution, 0, terrainManager.TerrainData.size.z);

        Vector3 highestPointWorldSpace = new Vector3(posz, posy, posx);

        //narrativeManager.InstantiateStagedArea (highestPointWorldSpace);
        narrativeManager.SpawnWeenie(highestPointWorldSpace);
        Vector3 bestPlayerSpawn = Vector3.zero;
        float closestDistance = float.MaxValue;
        float distanceFromTower = float.MaxValue;


        foreach (Vector3 position in shorePositions)
        {
            Vector3 origin = highestPointWorldSpace + (Vector3.up * weenieHeight);
            Vector3 direction = position - origin;
            if (Physics.Raycast(origin, direction, out RaycastHit hitinfo))
            {
                if (Vector3.Distance(hitinfo.point, position) < 1)
                {
                    float distance = Vector3.Distance(highestPointWorldSpace, position);
                    float distanceFromTarget = Mathf.Abs(targetDistance - distance);
                    if (distanceFromTarget < closestDistance)
                    {
                        distanceFromTower = distance;
                        closestDistance = distanceFromTarget;
                        bestPlayerSpawn = position;
                        //Debug.DrawRay(origin, direction, Color.green, 10f);
                    }
                    else
                    {
//                        Debug.DrawRay(origin, direction, Color.blue, 5f);
                    }
                }
                else
                {
                    //Debug.DrawRay(origin, direction, Color.red, 5f);
                }
            }
        }
        PlayerPrefab.transform.position = bestPlayerSpawn + Vector3.up;
        //TODO: Look at weenie at launch
//        playerCam.transform.LookAt(highestPointWorldSpace, Vector3.up);
        PlayerPrefab.SetActive(true);
    }
}
