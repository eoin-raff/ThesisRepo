using System.Collections;
using UnityEngine;
using System;
using MED10.PCG;
using MED10.Utilities;
using System.Collections.Generic;

public class NarrativeManager : MonoBehaviour
{
    public bool DEBUG = true;
    public Camera playerCam;        // Add the camera from the player object to check if a possible location is in sight

    // Prefabs for staged areas - Should be designed first and then instatiated at a possible location
    public GameObject[] stagedAreas = new GameObject[5];

    // Requirements for placement of SAs
    public SAParameters[] requirementSA = new SAParameters[5];

    // Time management variables
    private IEnumerator timeCoroutine;
    private int eventNum;                               // Which event are we at
    public float[] timeBetweenEvents = new float[5];    // Add how much time should elapse from end of last SA until next area can be loaded


    public bool playerHasControl = false;               // Used to control cinematic sequences. Should also be accessed from other scripts to e.g. start the game.
    private bool lookForNextSA = false;

    public int withinSA;                                // How close should the player be to the SA before triggering cinematic sequence

    private Vector3 positionAtLastSA;
    public float distanceBetweenSAs;                    // How far should player travel before starting to look for next SA 

    public TerrainGenerator terrainGenerator;
    float[,] heightmap;
    private void Start()
    {
        Debug.Assert(terrainGenerator != null, "No terrain generator assigned", this);
    }

    private void Update()
    {
        if (true)
        {
            if (heightmap == null)
            {
                heightmap = terrainGenerator.GetHeightMap(false);
            }
            Vector2 playerPos = new Vector2(playerCam.transform.position.x, playerCam.transform.position.z);
            if (Input.GetKeyDown(KeyCode.G))
            {
                PossibleSpawnPoints(playerPos, new Vector2(5, 5), 0, 1, 0, 1);
            }
        }
    }
    public void SpaceTime()                             // Call this method to initiate space-time behavior. Should probably be from another script
    {
        eventNum = 0;

        positionAtLastSA = playerCam.transform.position;

        float currentTime = Time.time;                  // Time when they start looking for SA spawn
        timeCoroutine = TimeManager(eventNum, currentTime);
        StartCoroutine(timeCoroutine);
    }


    private IEnumerator TimeManager(int num, float startTime)            // Manage time before next event has to spawn
    {
        float distance = Vector3.Distance(playerCam.transform.position, positionAtLastSA);

        if (distance >= distanceBetweenSAs)
        {
            if (Time.time - startTime >= timeBetweenEvents[num] && lookForNextSA == true)
            {
                CreateStagedArea(num);  // We should maybe also look for how long they have travelled?

                startTime = Time.time;                                      // If a SA is spawned, not the time and wait for that amount of time before looking for the next SA
                yield return TimeManager(eventNum, startTime);
            }
            else
            {
                yield return TimeManager(eventNum, startTime);
            }
        }
    }


    void CreateStagedArea(int numSA)
    {
        Vector2 scale = requirementSA[numSA].scale;
        float minSlope = requirementSA[numSA].minSlope;
        float maxSlope = requirementSA[numSA].maxSlope;
        float minHeight = requirementSA[numSA].minHeight;
        float maxHeight = requirementSA[numSA].maxHeight;

        // Check VE within some range for places that fits the requirements and add them to this array
        Vector2 playerPos = new Vector2(playerCam.transform.position.x, playerCam.transform.position.z);
        Transform[] possibleSALocations = PossibleSpawnPoints(playerPos, scale, minSlope, maxSlope, minHeight, maxHeight);

        // Find the most suited area and spawn assets
        for (int i = 0; i < possibleSALocations.GetLength(0); i++)
        {
            Vector3 screenPoint = playerCam.WorldToViewportPoint(possibleSALocations[i].position);

            if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
            {
                continue;       // If the area is witin the frustrum, do not spawn here. Could potentially lead to a lot of spawns behind the player?
            }
            else
            {
                Instantiate(stagedAreas[eventNum], possibleSALocations[i]);       // Spawn the assets in this location and break. Atm, it takes the first possible location and maybe not the best?

                WaitToStartCinematic(possibleSALocations[i]);                     // Start preparing for using cinematic

                eventNum++;         // Start looking into next SA in the coroutine

                positionAtLastSA = playerCam.transform.position;

                break;
            }
        }
    }


    Transform[] PossibleSpawnPoints(Vector2 playerPosition, Vector2 stagedAreaSize, float minHeight, float maxHeight, float minSlope, float maxSlope)
    {
        Transform[] possiblePlaces = new Transform[10];

        float targetHeight = minHeight + ((maxHeight - minHeight) / 2);
        float targetSlope = minSlope + ((maxSlope - minSlope) / 2);

        int r = 15;
        float[,] heightmap = terrainGenerator.GetHeightMap(false);
        int mappedX = (int)Utils.Map(playerPosition.x, 0, terrainGenerator.terrainData.size.x, 0, heightmap.GetLength(0));
        int mappedY = (int)Utils.Map(playerPosition.y, 0, terrainGenerator.terrainData.size.z, 0, heightmap.GetLength(1));

        //Search the area around the player on the HM
        // Sorted dictionaries are not very performant, so it may be better to split into lists and sort manually
        //SortedDictionary<float, Vector2> positionsByScore = new SortedDictionary<float, Vector2>();
        float bestScore = float.MaxValue;
        Vector2 bestPosition = Vector2.zero;
        int validPoints = 0;

        for (int y = Mathf.Max(0, mappedY - r); y < Mathf.Min(heightmap.GetLength(1), mappedY + r); y++)
        {
            for (int x = Mathf.Max(0, mappedX - r); x < Mathf.Min(heightmap.GetLength(0), mappedX + r); x++)
            {
                //Should search the areas around player
                float totalScore = 0;
                if (isValidPoint(minHeight, maxHeight, minSlope, maxSlope, heightmap, y, x))
                {
                    validPoints++;   
                    for (int ny = -(int)stagedAreaSize.y / 2; ny < (int)stagedAreaSize.y / 2; ny++)
                    {
                        for (int nx = -(int)stagedAreaSize.x / 2; nx < (int)stagedAreaSize.x / 2; nx++)
                        {
                            totalScore += scorePointValidity(x + nx, y + ny, heightmap, targetHeight, targetSlope);
                        }
                    }
                    if (totalScore < bestScore)
                    {
                        bestScore = totalScore;
                        bestPosition = new Vector2(x, y);
                    }
                }

                //positionsByScore.Add(totalScore, new Vector2(x, y));
            }
        }
        if (validPoints > 0)
        {
            Debug.Log(String.Format("Position: {0}, Score: {1}, height: {2}", bestPosition, bestScore, heightmap[(int)bestPosition.x, (int)bestPosition.y]));

            float worldSpaceX = terrainGenerator.terrainData.size.x * (bestPosition.x / terrainGenerator.terrainData.heightmapResolution);
            float worldSpaceY = terrainGenerator.terrainData.size.y * heightmap[(int)bestPosition.x, (int)bestPosition.y];
            float worldSpaceZ = terrainGenerator.terrainData.size.z * (bestPosition.y / terrainGenerator.terrainData.heightmapResolution);

            terrainGenerator.Terraform((int)bestPosition.x, (int)bestPosition.y, new Vector2(5, 5));
            GameObject go = Instantiate(stagedAreas[0], new Vector3(worldSpaceZ, worldSpaceY, worldSpaceX), Quaternion.identity);
            
        }
        else
        {
            Debug.LogWarning("No valid points found");
        }
        //Debug.Log(positionsByScore[positionsByScore.])

        // Search in an area of r radius from the position denoted by x and y which is the current player position
        // Find first 10 suitable areas that fulfill scale, height, and slope
        // Make sure a found area can't be found again
        // Return up to 10 areas where spawning can happen

        return possiblePlaces;               // Return center of found area
    }

    private bool isValidPoint(float minHeight, float maxHeight, float minSlope, float maxSlope, float[,] heightmap, int y, int x)
    {
        bool isInHeightRange = heightmap[x, y] < maxHeight && heightmap[x, y] > minHeight;
        float slope = terrainGenerator.terrainData.GetSteepness(
            x / (float)terrainGenerator.terrainData.alphamapResolution,
            y / (float)terrainGenerator.terrainData.alphamapResolution);
        bool isInSlopeRange = slope > minSlope && slope < maxSlope;
        return isInHeightRange && isInSlopeRange;
    }

    private float scorePointValidity(int x, int y, float[,] hm, float targetHeight, float targetSlope)
    {
        float height = hm[x, y];
        float slope = terrainGenerator.terrainData.GetSteepness(
            x / (float)terrainGenerator.terrainData.alphamapResolution,
            y / (float)terrainGenerator.terrainData.alphamapResolution);

        float score_h = Mathf.Abs(targetHeight - height);
        float score_s = Mathf.Abs(targetSlope - slope);

        return score_h + score_s;
    }

    private void CinematicSequence(Transform target)
    {
        playerHasControl = false;

        Vector3 lookAtPosition = target.transform.position;

        float speed = 5.0f;                 // this is the speed at which the camera moves

        playerCam.transform.position = Vector3.Lerp(transform.position, lookAtPosition, speed);

        playerCam.transform.LookAt(lookAtPosition);

        LookAtSA(5.0f);                     // Stare at the SA before moving again

        lookForNextSA = true;
    }


    private IEnumerator WaitToStartCinematic(Transform locationOfSA)        // Check if the player is close enough to the SA to start the cinematic sequence
    {

        if (playerCam.transform.position.x >= locationOfSA.position.x - withinSA &&
            playerCam.transform.position.x <= locationOfSA.position.x + withinSA &&
            playerCam.transform.position.y >= locationOfSA.position.y - withinSA &&
            playerCam.transform.position.y <= locationOfSA.position.y + withinSA)
        {
            CinematicSequence(locationOfSA);
            yield return new WaitForSeconds(1.0f);
        }
        else
        {
            yield return WaitToStartCinematic(locationOfSA);
        }
    }


    private IEnumerator LookAtSA(float time)                                // Controls how long the cinematic sequence lasts
    {
        yield return new WaitForSeconds(time);
        playerHasControl = true;
    }
}


// Class for holding the parameter of the staged areas
[Serializable]
public class SAParameters : TableItem
{
    public Vector2 scale = Vector2.zero;
    public float minSlope = 0.0f;
    public float maxSlope = 1.0f;
    public float minHeight = 0.0f;
    public float maxHeight = 1.0f;
}
