using System.Collections;
using UnityEngine;
using System;
using MED10.PCG;
using MED10.Utilities;
using System.Collections.Generic;
using System.Linq;

public class NarrativeManager : MonoBehaviour
{
    public GameObject player;        // Add the camera from the player object to check if a possible location is in sight

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
            Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.z);
            if (Input.GetKeyDown(KeyCode.G))
            {
                List<StagedAreaCandidatePosition> candidates = PossibleSpawnPoints(playerPos, new Vector2(5, 5), 0, 1, 0, 1);

                if (FindBestPosition(candidates, new Vector4(5, 1, 1, 2).normalized, out Vector2 spawnPosition))
                {
                    InstantiateStagedArea(spawnPosition);
                }
            }
        }
    }
    public void SpaceTime()                             // Call this method to initiate space-time behavior. Should probably be from another script
    {
        eventNum = 0;

        positionAtLastSA = player.transform.position;

        float currentTime = Time.time;                  // Time when they start looking for SA spawn
        timeCoroutine = TimeManager(eventNum, currentTime);
        StartCoroutine(timeCoroutine);
    }


    private IEnumerator TimeManager(int num, float startTime)            // Manage time before next event has to spawn
    {
        float distance = Vector3.Distance(player.transform.position, positionAtLastSA);

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
        //Vector2 scale = requirementSA[numSA].scale;
        //float minSlope = requirementSA[numSA].minSlope;
        //float maxSlope = requirementSA[numSA].maxSlope;
        //float minHeight = requirementSA[numSA].minHeight;
        //float maxHeight = requirementSA[numSA].maxHeight;

        //// Check VE within some range for places that fits the requirements and add them to this array
        //Vector2 playerPos = new Vector2(playerCam.transform.position.x, playerCam.transform.position.z);
        //Transform[] possibleSALocations =  //PossibleSpawnPoints(playerPos, scale, minSlope, maxSlope, minHeight, maxHeight);

        //// Find the most suited area and spawn assets
        //for (int i = 0; i < possibleSALocations.GetLength(0); i++)
        //{
        //    Vector3 screenPoint = playerCam.WorldToViewportPoint(possibleSALocations[i].position);

        //    if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
        //    {
        //        continue;       // If the area is witin the frustrum, do not spawn here. Could potentially lead to a lot of spawns behind the player?
        //    }
        //    else
        //    {
        //        Instantiate(stagedAreas[eventNum], possibleSALocations[i]);       // Spawn the assets in this location and break. Atm, it takes the first possible location and maybe not the best?

        //        WaitToStartCinematic(possibleSALocations[i]);                     // Start preparing for using cinematic

        //        eventNum++;         // Start looking into next SA in the coroutine

        //        positionAtLastSA = playerCam.transform.position;

        //        break;
        //    }
        //}
        throw new NotImplementedException();
    }


    private List<StagedAreaCandidatePosition> PossibleSpawnPoints(Vector2 playerPosition, Vector2 stagedAreaSize, float minHeight, float maxHeight, float minSlope, float maxSlope)
    {
        float targetHeight = minHeight + ((maxHeight - minHeight) / 2);
        float targetSlope = minSlope + ((maxSlope - minSlope) / 2);

        int r = 50;
        float[,] heightmap = terrainGenerator.GetHeightMap(false);
        int mappedY = (int)Utils.Map(playerPosition.x, 0, terrainGenerator.terrainData.size.x, 0, heightmap.GetLength(0));
        int mappedX = (int)Utils.Map(playerPosition.y, 0, terrainGenerator.terrainData.size.z, 0, heightmap.GetLength(1));


        List<StagedAreaCandidatePosition> candidates = new List<StagedAreaCandidatePosition>();

        //Search the area around the player on the HM
        for (int y = Mathf.Max(0, mappedY - r); y < Mathf.Min(heightmap.GetLength(1), mappedY + r); y++)
        {
            for (int x = Mathf.Max(0, mappedX - r); x < Mathf.Min(heightmap.GetLength(0), mappedX + r); x++)
            {
                if (IsValidPoint(minHeight, maxHeight, minSlope, maxSlope, heightmap, y, x))
                {
                    float totalScore = 0;
                    float heightScore = 0;
                    float slopeScore = 0;
                    Vector2 pointPosition = new Vector2(x, y);

                    for (int ny = -(int)stagedAreaSize.y / 2; ny < (int)stagedAreaSize.y / 2; ny++)
                    {
                        for (int nx = -(int)stagedAreaSize.x / 2; nx < (int)stagedAreaSize.x / 2; nx++)
                        {
                            totalScore += ScorePointValidity(x + nx, y + ny, heightmap, targetHeight, targetSlope, out float h, out float s);
                            heightScore += h;
                            slopeScore += s;
                        }
                    }

                    candidates.Add(new StagedAreaCandidatePosition
                    {
                        heightmapPosition = pointPosition,
                        worldPosition = new Vector3(
                            y / (float)terrainGenerator.terrainData.heightmapResolution * terrainGenerator.terrainData.size.z,
                            heightmap[(int)x, (int)y] * terrainGenerator.terrainData.size.y,
                            x / (float)terrainGenerator.terrainData.heightmapResolution * terrainGenerator.terrainData.size.x
                        ),
                        heightScore = heightScore,
                        slopeScore = slopeScore,
                        dstFromPlayer = Vector2.Distance(playerPosition, pointPosition)
                    });
                }
            }
        }

        // Search in an area of r radius from the position denoted by x and y which is the current player position
        // Find first 10 suitable areas that fulfill scale, height, and slope
        // Make sure a found area can't be found again
        // Return up to 10 areas where spawning can happen

        return candidates;
    }

    private bool FindBestPosition(List<StagedAreaCandidatePosition> candidates, Vector4 weights, out Vector2 position)
    {
        // candidates[0] is best fit in terms of height and slope, but this function should take other parameters such as distance and direction etc. into account
        // for now, lets just use [0]

        if (candidates.Count > 0)
        {
            float bestScore = float.MaxValue;
            StagedAreaCandidatePosition chosenCandidate = new StagedAreaCandidatePosition
            {
                heightmapPosition = Vector2.zero
            };
            for (int i = 0; i < candidates.Count; i++)
            {
                //Evaluate Candidates
                //                float directionToPlayer = Vector2.Dot((player.transform.position.XZ() - candidates[i].position).normalized, player.transform.forward.normalized);
                Vector3 directionToCandidate = (candidates[i].worldPosition - player.transform.position);
                float directionScore = Vector3.Dot(player.transform.forward.normalized, directionToCandidate.normalized);

                if (Mathf.Abs(directionScore) > 0.75f) //Area to ignore infront and behind you
                {
                    //ignore points behind you
                    Debug.DrawRay(player.transform.position, directionToCandidate, Color.red, 10f);

                    continue;
                }
                Debug.DrawRay(player.transform.position, directionToCandidate, Color.blue, 10f);

                float totalScore = (
                    (directionScore * weights.x) //Lower score when far from centre of view
                    + (candidates[i].heightScore * weights.y) //lower when closer to target
                    + (candidates[i].slopeScore * weights.z)  //lower when closer to target
                    + 1 - ((candidates[i].dstFromPlayer * -weights.w))
                    ) / 4;
                if (totalScore < bestScore)
                {
                    bestScore = totalScore;
                    chosenCandidate = candidates[i];
                    Debug.DrawRay(player.transform.position, directionToCandidate, Color.green, 1f);

                }


            }
            Vector3 dir = (chosenCandidate.worldPosition - player.transform.position);
            Debug.DrawRay(player.transform.position, dir, Color.cyan, 15f);
            //Debug.DrawRay(player.transform.position, player.transform.forward, Color.yellow, 15f);


            //Debug.Log(directionScore);
            position = chosenCandidate.heightmapPosition;
            //Debug.Log(Vector2.Dot((player.transform.position.XZ() - candidates[0].position).normalized, player.transform.forward.XZ().normalized));

            //Debug.DrawRay(player.transform.position, new Vector3(
            //    player.transform.forward.XZ().normalized.x, 
            //    player.transform.position.y, 
            //    player.transform.forward.XZ().normalized.y) * 10, 
            //    Color.red, 10f);

            //Debug.DrawRay(player.transform.position, new Vector3(
            //    (candidates[0].position.y - player.transform.position.x),
            //    player.transform.position.y,
            //    (candidates[0].position.x - player.transform.position.y)) * 10, Color.blue, 10f);

            return true;
        }
        Debug.LogWarning("No Suitable Candidates for staged area.", this);
        position = Vector2.zero;
        return false;
    }

    private void InstantiateStagedArea(Vector2 position)
    {
        Vector3 worldSpacePos = new Vector3(
            position.y / (float)terrainGenerator.terrainData.heightmapResolution * terrainGenerator.terrainData.size.z,
            heightmap[(int)position.x, (int)position.y] * terrainGenerator.terrainData.size.y,
            position.x / (float)terrainGenerator.terrainData.heightmapResolution * terrainGenerator.terrainData.size.x
            );
        terrainGenerator.Terraform((int)position.x, (int)position.y, 0.75f, new Vector2(5, 5)); //V2(5, 5) should be replaced with details from staged area parameters
        GameObject go = Instantiate(stagedAreas[0], worldSpacePos, Quaternion.identity);
    }

    private bool IsValidPoint(float minHeight, float maxHeight, float minSlope, float maxSlope, float[,] heightmap, int y, int x)
    {
        bool isInHeightRange = heightmap[x, y] < maxHeight && heightmap[x, y] > minHeight;
        float slope = terrainGenerator.terrainData.GetSteepness(
            x / (float)terrainGenerator.terrainData.alphamapResolution,
            y / (float)terrainGenerator.terrainData.alphamapResolution);
        bool isInSlopeRange = slope > minSlope && slope < maxSlope;
        return isInHeightRange && isInSlopeRange;
    }

    private float ScorePointValidity(int x, int y, float[,] hm, float targetHeight, float targetSlope, out float heightScore, out float slopeScore)
    {
        float height = hm[x, y];
        float slope = terrainGenerator.terrainData.GetSteepness(
            x / (float)terrainGenerator.terrainData.alphamapResolution,
            y / (float)terrainGenerator.terrainData.alphamapResolution);

        heightScore = Mathf.Abs(targetHeight - height);
        slopeScore = Mathf.Abs(targetSlope - slope);

        return heightScore + slopeScore;
    }

    private void CinematicSequence(Transform target)
    {
        playerHasControl = false;

        Vector3 lookAtPosition = target.transform.position;

        float speed = 5.0f;                 // this is the speed at which the camera moves

        player.transform.position = Vector3.Lerp(transform.position, lookAtPosition, speed);

        player.transform.LookAt(lookAtPosition);

        LookAtSA(5.0f);                     // Stare at the SA before moving again

        lookForNextSA = true;
    }


    private IEnumerator WaitToStartCinematic(Transform locationOfSA)        // Check if the player is close enough to the SA to start the cinematic sequence
    {

        if (player.transform.position.x >= locationOfSA.position.x - withinSA &&
            player.transform.position.x <= locationOfSA.position.x + withinSA &&
            player.transform.position.y >= locationOfSA.position.y - withinSA &&
            player.transform.position.y <= locationOfSA.position.y + withinSA)
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

[Serializable]
public struct StagedAreaCandidatePosition
{
    public Vector2 heightmapPosition;
    public Vector3 worldPosition;
    public float heightScore;
    public float slopeScore;
    public float dstFromPlayer;
}
