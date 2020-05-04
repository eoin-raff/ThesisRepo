using System.Collections;
using UnityEngine;
using System;
using MED10.PCG;
using MED10.Utilities;
using System.Collections.Generic;
using MED10.Architecture.Events;

public class SpaceTimeManager : MonoBehaviour
{

    public GameObject player;

    public NarrativeManager narrativeManager;

    public GameEvent StartSASearch;

    private IEnumerator cinematicCoroutine;
    private int saNum = 0;                                  // Which SA are we at (Can also be used for event in between SAs)
    public float[] timeBetweenEvents = new float[6];        // Add how much time should elapse from end of last SA until next area can be loaded

    public bool playerHasControl = false;                   // Used to control cinematic sequences. Should also be accessed from other scripts to e.g. start the game.
    private bool lookForNextSA = false;

    public float withinSA;                                    // How close should the player be to the SA before triggering cinematic sequence

    private Vector3 positionAtLastSA;
    public float[] distanceBetweenSAs;                      // How far should player travel before starting to look for next SA 
    private float timeAtLastSA;

    public TerrainGenerator terrainGenerator;
    float[,] heightmap;


    private void Start()
    {
        saNum = 0;
        positionAtLastSA = player.transform.position;
        lookForNextSA = true;
        timeAtLastSA = Time.time;
    }

    private void Update()
    {

        if (heightmap == null)
        {
            heightmap = terrainGenerator.GetHeightMap(false);
        }


        if (lookForNextSA)
        {
            // If enough time has passed since last SA
            if (Time.time - timeAtLastSA >= timeBetweenEvents[saNum])
            {
                float distance = Vector3.Distance(player.transform.position, positionAtLastSA);
            //    Debug.Log("Current Distance: " + distance);

                // If you are far enough away from last SA
                if (distance >= distanceBetweenSAs[saNum])
                {
                    StartSASearch.Raise();
                    /*
                    //Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.z);

                    //List<StagedAreaCandidatePosition> candidates = narrativeManager.PossibleSpawnPoints(playerPos, new Vector2(5, 5), 0, 1, 0, 1);

                    //if (narrativeManager.FindBestPosition(candidates, new Vector4(5, 1, 1, 2).normalized, out Vector2 spawnPosition))
                    //{
                    //    lookForNextSA = false;

                    //    narrativeManager.InstantiateStagedArea(spawnPosition);

                    //    WaitToStartCinematic(spawnPosition);

                    //    cinematicCoroutine = WaitToStartCinematic(spawnPosition);

                    //    if (saNum <= 5)
                    //    {
                    //        StartCoroutine(cinematicCoroutine);
                    //        Debug.Log("Starting coroutine " + saNum);
                    //    }
                    //    else
                    //    {
                    //        StopCoroutine(cinematicCoroutine);
                    //    }


                    //    Debug.Log("SA " + saNum + " Instantiated!");

                    //    positionAtLastSA = spawnPosition;

                    //    saNum++;
                    //    //lookForNextSA = true;                             // Move this to cinematicsequence if that gets working
                    //    timeAtLastSA = Time.time;
                    //}*/
                }
            }
        }
    }


    private IEnumerator WaitToStartCinematic(Vector2 locationOfSA)        // Check if the player is close enough to the SA to start the cinematic sequence
    {
        while (lookForNextSA == false)
        {
            Vector3 worldSpacePos = new Vector3(
            locationOfSA.y / (float)terrainGenerator.terrainData.heightmapResolution * terrainGenerator.terrainData.size.z,
            heightmap[(int)locationOfSA.x, (int)locationOfSA.y] * terrainGenerator.terrainData.size.y,
            locationOfSA.x / (float)terrainGenerator.terrainData.heightmapResolution * terrainGenerator.terrainData.size.x
            );

            float distance = Vector3.Distance(player.transform.position, worldSpacePos);
            Debug.Log(distance);

            if (distance <= withinSA)
            {
                //LookAtSA(5.0f, worldSpacePos);                     // Stare at the SA before moving again
                Debug.Log("looked at SA");
                lookForNextSA = true;

                yield return null;

            }
            else
            {
                Debug.Log("NOT starting cinematic sequence...");
                yield return null;
            }
        }
    }


    private void LookAtSA(float time, Vector2 target)                                
    {
        float counter = 0.0f;

        while (counter <= time)
        {
            player.transform.LookAt(target);
            counter = counter + 0.01f;
        }

        playerHasControl = true;
    }
}
