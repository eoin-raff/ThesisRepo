using System.Collections;
using UnityEngine;
using System;
using MED10.PCG;
using MED10.Utilities;
using System.Collections.Generic;
using MED10.Architecture.Events;

public class NarrativeManager : MonoBehaviour
{
    public GameObject player;        // Add the camera from the player object to check if a possible location is in sight

    // Prefabs for staged areas - Should be designed first and then instatiated at a possible location
    public GameObject[] stagedAreas = new GameObject[5];
    private StagedArea nextSA;
    public int weenieIdx = 2;

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
    public float[] distanceBetweenSAs;                    // How far should player travel before starting to look for next SA 

    public TerrainManager terrainManager;
    float[,] heightmap;

    private List<StagedAreaCandidatePosition> candidates;
    private bool candidatesReady = false;

    private Vector2 nextStagedAreaSpawnPosition;
    private bool foundPosition = false;

    public GameEvent StagedAreaCreated;
    private Vector2 playerPos;

    private IEnumerator cinematicCoroutine;
    private int saNum = -1;                                  // Which SA are we at (Can also be used for event in between SAs)
    private float timeAtLastSA;

    public GameEvent SAStarted;
    public GameEvent SAEnded;

    private void Start()
    {
        Debug.Assert(terrainManager != null, "No terrain manager assigned", this);
        PrepareForNextSA();
    }

    public void PrepareForNextSA()
    {
        if (saNum < stagedAreas.Length - 1)
        {
            positionAtLastSA = player.transform.position;
            lookForNextSA = true;
            timeAtLastSA = Time.time;
            nextSA = stagedAreas[++saNum].GetComponent<StagedArea>();
        }
    }

    private void Update()
    {
        if (heightmap == null)
        {
            heightmap = terrainManager.GetHeightmap(false);
        }
        playerPos = new Vector2(player.transform.position.x, player.transform.position.z);

        //TODO: Make sure lookForNextSA flags as true
        if (saNum < stagedAreas.Length)
        {
            if (lookForNextSA)
            {
                //Debug.Log("looking for SA " + saNum);
                // If enough time has passed since last SA
                if (Time.time - timeAtLastSA >= timeBetweenEvents[saNum])
                {
                    //Debug.Log("enough time passed");
                    float distance = Vector3.Distance(player.transform.position, positionAtLastSA);

                    // If you are far enough away from last SA
                    if (distance >= distanceBetweenSAs[saNum])
                    {
                        //Debug.Log("enough distance");
                        lookForNextSA = false;
                        // SearchForCandidates(playerPos);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            SearchForCandidates(playerPos);
        }
        if (candidatesReady)
        {
            if (candidates.Count > 0)
            {
                Debug.Log("Assessing Candidates");
                AssessCandidates();
            }
            else
            {
                Debug.Log("No Candidates");
                SearchForCandidates(playerPos);
            }

        }
        if (foundPosition)
        {
            foundPosition = false;
            Debug.Log("Spawning SA " + saNum);
            CreateStagedArea();
        }
    }

    internal void SpawnWeenie(Vector3 position, int weenieIdx)
    {
        if (weenieIdx > stagedAreas.Length - 1)
        {
            return;
        }
        GameObject SAPrefab = stagedAreas[weenieIdx];
        StagedArea SA = stagedAreas[weenieIdx].GetComponent<StagedArea>();
        Vector2 stagedAreaSize = SA.size; //V2(5, 5) should be replaced with details from staged area parameters
        StartCoroutine(terrainManager.GetPainter().RemoveTreesInArea(position.XZ(), stagedAreaSize));
        int hmX =(int)Utils.Map(position.x, 0, terrainManager.TerrainData.size.x, 0, terrainManager.HeightmapResolution);
        int hmY =(int)Utils.Map(position.z, 0, terrainManager.TerrainData.size.z, 0, terrainManager.HeightmapResolution);
        StartCoroutine(terrainManager.GetTerrainGenerator().FlattenAreaAroundPoint(hmY, hmX, 0.9f, stagedAreaSize)); 
        
        SAPrefab.transform.position = position;
        SAPrefab.SetActive(true);
    }

    private void CreateStagedArea()
    {
        InstantiateStagedArea(nextStagedAreaSpawnPosition);
        foundPosition = false;
    }
    private void AssessCandidates()
    {
        candidatesReady = false;
        foundPosition = false;
        StartCoroutine(FindBestPosition(candidates, new Vector4(5, 1, 1, 2).normalized, SetStagedAreaPosition));
    }
    public void SearchForCandidates(Vector2 playerPos)
    {
        candidates = new List<StagedAreaCandidatePosition>();
        candidatesReady = false;
        //TODO: Get Values from SAs
        Debug.Log("Searching for Candidates");
        StartCoroutine(AssessSpawnPointsWithCallback(playerPos, nextSA.size, nextSA.minHeight, nextSA.maxHeight, nextSA.minSlope, nextSA.maxSlope, SetCandidates));
    }

    private IEnumerator AssessSpawnPointsWithCallback(Vector2 playerPosition, Vector2 stagedAreaSize, float minHeight, float maxHeight, float minSlope, float maxSlope, Action<List<StagedAreaCandidatePosition>> addCandidatesCallback)
    {
        float targetHeight = minHeight + ((maxHeight - minHeight) / 2);
        float targetSlope = minSlope + ((maxSlope - minSlope) / 2);

        int r = 50;
        float[,] heightmap = terrainManager.GetHeightmap(false);
        int mappedY = (int)Utils.Map(playerPosition.x, 0, terrainManager.TerrainData.size.x, 0, heightmap.GetLength(0));
        int mappedX = (int)Utils.Map(playerPosition.y, 0, terrainManager.TerrainData.size.z, 0, heightmap.GetLength(1));


        List<StagedAreaCandidatePosition> candidates = new List<StagedAreaCandidatePosition>();

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
                            y / (float)terrainManager.TerrainData.heightmapResolution * terrainManager.TerrainData.size.z,
                            heightmap[(int)x, (int)y] * terrainManager.TerrainData.size.y,
                            x / (float)terrainManager.TerrainData.heightmapResolution * terrainManager.TerrainData.size.x
                        ),
                        heightScore = heightScore,
                        slopeScore = slopeScore,
                        dstFromPlayer = Vector2.Distance(playerPosition, pointPosition)
                    });
                }

            }
            yield return null;
        }
        addCandidatesCallback(candidates);
        yield break;
    }

    private void SetCandidates(List<StagedAreaCandidatePosition> candidates)
    {
        Debug.Log("Setting Candidates");
        candidatesReady = true;
        this.candidates = candidates;
    }

    private List<StagedAreaCandidatePosition> PossibleSpawnPoints(Vector2 playerPosition, Vector2 stagedAreaSize, float minHeight, float maxHeight, float minSlope, float maxSlope)
    {
        float targetHeight = minHeight + ((maxHeight - minHeight) / 2);
        float targetSlope = minSlope + ((maxSlope - minSlope) / 2);

        int r = 50;
        float[,] heightmap = terrainManager.GetHeightmap(false);
        int mappedY = (int)Utils.Map(playerPosition.x, 0, terrainManager.TerrainData.size.x, 0, heightmap.GetLength(0));
        int mappedX = (int)Utils.Map(playerPosition.y, 0, terrainManager.TerrainData.size.z, 0, heightmap.GetLength(1));


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
                            y / (float)terrainManager.TerrainData.heightmapResolution * terrainManager.TerrainData.size.z,
                            heightmap[(int)x, (int)y] * terrainManager.TerrainData.size.y,
                            x / (float)terrainManager.TerrainData.heightmapResolution * terrainManager.TerrainData.size.x
                        ),
                        heightScore = heightScore,
                        slopeScore = slopeScore,
                        dstFromPlayer = Vector2.Distance(playerPosition, pointPosition)
                    });
                }
            }
        }
        return candidates;
    }

    private IEnumerator FindBestPosition(List<StagedAreaCandidatePosition> candidates, Vector4 weights, Action<Vector2> setPositionCallback)
    {
        float bestScore = float.MaxValue;
        StagedAreaCandidatePosition chosenCandidate = new StagedAreaCandidatePosition
        {
            heightmapPosition = Vector2.zero
        };
        for (int i = 0; i < candidates.Count; i++)
        {
            //Evaluate Candidates

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
            yield return null;
        }
        Vector3 dir = (chosenCandidate.worldPosition - player.transform.position);
        Debug.DrawRay(player.transform.position, dir, Color.cyan, 15f);
        setPositionCallback(chosenCandidate.heightmapPosition);
        yield break;
    }

    private void SetStagedAreaPosition(Vector2 position)
    {
        Debug.Log("Found best position.");
        foundPosition = true;
        nextStagedAreaSpawnPosition = position;
    }

    public void InstantiateStagedArea(Vector2 position)
    {
        Vector3 worldSpacePos = new Vector3(
            position.y / (float)terrainManager.HeightmapResolution * terrainManager.TerrainData.size.z,
            heightmap[(int)position.x, (int)position.y] * terrainManager.TerrainData.size.y,
            position.x / (float)terrainManager.HeightmapResolution * terrainManager.TerrainData.size.x
            );
        InstantiateStagedArea(worldSpacePos);
    }

    public void InstantiateStagedArea(Vector3 position)
    {
        if (saNum < stagedAreas.Length)
        {

            GameObject stagedArea = stagedAreas[saNum];
            if (!stagedArea.activeSelf)
            {
                Debug.Log("Activating area");

                Vector2 stagedAreaSize = new Vector2(10, 10); //V2(5, 5) should be replaced with details from staged area parameters
                StartCoroutine(terrainManager.GetPainter().RemoveTreesInArea(position.XZ(), stagedAreaSize));

                StartCoroutine(terrainManager.GetTerrainGenerator().FlattenAreaAroundPoint((int)position.x, (int)position.y, nextSA.flattenPower, stagedAreaSize));

                stagedArea.transform.position = position;
                stagedArea.SetActive(true);
            }
            else
            {
                Debug.Log("Area already active");
            }
        }
    }

    private bool IsValidPoint(float minHeight, float maxHeight, float minSlope, float maxSlope, float[,] heightmap, int y, int x)
    {
        bool isInHeightRange = heightmap[x, y] < maxHeight && heightmap[x, y] > minHeight;
        float slope = terrainManager.TerrainData.GetSteepness(
            x / (float)terrainManager.TerrainData.alphamapResolution,
            y / (float)terrainManager.TerrainData.alphamapResolution);
        bool isInSlopeRange = slope > minSlope && slope < maxSlope;
        return isInHeightRange && isInSlopeRange;
    }

    private float ScorePointValidity(int x, int y, float[,] hm, float targetHeight, float targetSlope, out float heightScore, out float slopeScore)
    {
        float height = hm[x, y];
        float slope = terrainManager.TerrainData.GetSteepness(
            x / (float)terrainManager.TerrainData.alphamapResolution,
            y / (float)terrainManager.TerrainData.alphamapResolution);

        heightScore = Mathf.Abs(targetHeight - height);
        slopeScore = Mathf.Abs(targetSlope - slope);

        return heightScore + slopeScore;
    }

    private IEnumerator WaitToStartCinematic(Vector2 locationOfSA)        // Check if the player is close enough to the SA to start the cinematic sequence
    {
        while (lookForNextSA == false)
        {
            Vector3 SAPosition = stagedAreas[saNum].transform.position;

            float distance = Vector3.Distance(player.transform.position, SAPosition);

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
