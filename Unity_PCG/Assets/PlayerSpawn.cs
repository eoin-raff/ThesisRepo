using MED10.Architecture.Events;
using MED10.PCG;
using MED10.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public Camera playerCam;
    public float targetDistance = 100;
  
    [SerializeField]
    private NarrativeManager narrativeManager;
    public float weenieHeight;

    public GameEvent playerInstantiated;

    public void InstantiatePlayer()
    {
        StartCoroutine(SpawnAtShoreVisibleFromWeenie());
    }

    public IEnumerator SpawnAtShoreVisibleFromWeenie()
    {
        float[,] heightMap = TerrainManager.Instance.GetHeightmap(false);
        int resolution = TerrainManager.Instance.HeightmapResolution;
        float waterHeight = TerrainManager.Instance.GetPainter().waterHeight;
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
                        float posX = Utils.Map(x, 0, resolution, 0, TerrainManager.Instance.TerrainData.size.x);
                        float posY = heightMap[x, y] * TerrainManager.Instance.TerrainData.size.y;
                        float posZ = Utils.Map(y, 0, resolution, 0, TerrainManager.Instance.TerrainData.size.z);

                        Vector3 position = new Vector3(posZ, posY, posX);
                        shorePositions.Add(position);
                    }
                }
                #endregion
            }
            
            //yield return null;
        }

        float posx = Utils.Map(highestPoint.x, 0, resolution, 0, TerrainManager.Instance.TerrainData.size.x);
        float posy = heightMap[(int)highestPoint.x, (int)highestPoint.y] * TerrainManager.Instance.TerrainData.size.y;
        float posz = Utils.Map(highestPoint.y, 0, resolution, 0, TerrainManager.Instance.TerrainData.size.z);

        Vector3 highestPointWorldSpace = new Vector3(posz, posy, posx);

        //TODO: Include Terraforming here
        narrativeManager.FindWeenieLocation(highestPointWorldSpace, 2);
        Vector3 bestPlayerSpawn = Vector3.zero;
        float closestDistance = float.MaxValue;
        float distanceFromTower = float.MaxValue;

        List<Vector3> possibleBoatSpawns = new List<Vector3>();

        foreach (Vector3 position in shorePositions)
        {
            Vector3 weenieTopOrigin = highestPointWorldSpace + (Vector3.up * weenieHeight);
            Vector3 playerHeightAtTower = highestPointWorldSpace + (Vector3.up * 1.8f);
            Vector3 direction = position - weenieTopOrigin;


            #region Find Player Spawn
            if (Physics.Raycast(weenieTopOrigin, direction, out RaycastHit playerSpawnHitInfo))
            {
                if (Vector3.Distance(playerSpawnHitInfo.point, position) < 1)
                {
                    float distance = Vector3.Distance(highestPointWorldSpace, position);
                    float distanceFromTarget = Mathf.Abs(targetDistance - distance);
                    if (distanceFromTarget < closestDistance)
                    {
                        distanceFromTower = distance;
                        closestDistance = distanceFromTarget;
                        bestPlayerSpawn = position;
                        Debug.DrawRay(weenieTopOrigin, direction, Color.green, 10f);
                    }
                }
            }
            #endregion

            if (Physics.Raycast(weenieTopOrigin, direction, out RaycastHit boatSpawnHitInfo))
            {
                if (Vector3.Distance(boatSpawnHitInfo.point, position) < 1)
                {
                    possibleBoatSpawns.Add(boatSpawnHitInfo.point);
                    Debug.DrawRay(weenieTopOrigin, direction, Color.blue, 5f);
                }
            }
        }
        float bestBoatDistance = 0;
        Vector3 bestBoatPosition = Vector3.zero;

        foreach (Vector3 position in possibleBoatSpawns)
        {
            float distanceFromPlayerSpawn = Vector3.Distance(position, bestPlayerSpawn);
            if (distanceFromPlayerSpawn > bestBoatDistance)
            {
                bestBoatPosition = position;
                bestBoatDistance = distanceFromPlayerSpawn;
            }
        }
        //Need to clarify which weenie
        narrativeManager.FindWeenieLocation(bestBoatPosition, 4);

        PlayerPrefab.transform.position = bestPlayerSpawn + Vector3.up;
        //TODO: Look at weenie at launch
//        playerCam.transform.LookAt(highestPointWorldSpace, Vector3.up);
        PlayerPrefab.SetActive(true);

        playerInstantiated.Raise();
        yield break;
    }
}
