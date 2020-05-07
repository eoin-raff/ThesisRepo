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
    public int[] weenieIndices;

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

    //public TerrainManager TerrainManager.Instance;
    float[,] heightmap;

    private List<StagedAreaCandidatePosition> temp_candidates;
    private bool candidatesReady = false;

    private Vector2 nextStagedAreaSpawnPosition;
    private bool foundPosition = false;

    public GameEvent StagedAreaCreated;
    private Vector2 playerPos;

    private IEnumerator cinematicCoroutine;
    private int saNum = -1;                                  // Which SA are we at (Can also be used for event in between SAs)
    private float timeAtLastSA;
    private StagedArea nextSA;
    private GameObject targetWeenie;
    public GameEvent SAStarted;
    public GameEvent SAEnded;

    Dictionary<StagedArea, List<StagedAreaCandidatePosition>> StagedAreaCandidates;
    private int weenieIndex = 0;

    private void Start()
    {
        StagedAreaCandidates = new Dictionary<StagedArea, List<StagedAreaCandidatePosition>>();
        FindStagedAreaCandidates();
        PrepareForNextSA();
        targetWeenie = stagedAreas[weenieIndices[weenieIndex]];
    }

    public void FindStagedAreaCandidates()
    {
        for (int i = 0; i < stagedAreas.Length; i++)
        {
            if (!stagedAreas[i].activeSelf && stagedAreas[i].GetComponent<StagedArea>().progress.Value < 1)
            {
                SearchForCandidates(stagedAreas[i].GetComponent<StagedArea>());
            }
        }
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
            heightmap = TerrainManager.Instance.GetHeightmap(false);
        }
        playerPos = new Vector2(player.transform.position.x, player.transform.position.z);


        Vector3 playerToWeenie = targetWeenie.transform.position - player.transform.position;
        Vector3 startingPoint = player.transform.position;
        if (nextSA.gameObject.activeSelf)
        {
            Vector3 playerToStagedArea = nextSA.transform.position - player.transform.position;
            Debug.DrawLine(player.transform.position, nextSA.transform.position, Color.magenta);
            if (Vector3.Angle(playerToWeenie, playerToStagedArea) > 100)
            {
                //&& it hasn't been visited
                nextSA.gameObject.SetActive(false);
            }
        }

        //Check if We need to spawn a new SA
        Debug.DrawLine(player.transform.position, targetWeenie.transform.position, Color.cyan);
        Debug.DrawRay(player.transform.position, player.transform.forward * 50f);
        if (lookForNextSA)
        {
            if (StagedAreaCandidates.ContainsKey(nextSA) && lookForNextSA)
            {
                List<StagedAreaCandidatePosition> nextSACandidates = StagedAreaCandidates[nextSA];
                List<StagedAreaCandidatePosition> bestCandidates = new List<StagedAreaCandidatePosition>();


                Ray ray = new Ray(startingPoint, playerToWeenie);
                foreach (var pos in nextSACandidates)
                {
                    Vector3 playerToCandidate = pos.worldPosition - player.transform.position;
                    float distanceFromPath = Vector3.Cross(ray.direction, pos.worldPosition - ray.origin).magnitude;

                    if (distanceFromPath < 20 && playerToCandidate.magnitude > 10 && playerToCandidate.magnitude < 100 && Vector3.Angle(playerToWeenie, playerToCandidate) < 35)
                    {
                        if (Physics.Raycast(new Ray(startingPoint, playerToCandidate), out RaycastHit hitinfo))
                        {
                            if (Vector3.Distance(hitinfo.point, pos.worldPosition) > 1)
                            {
                                //Something obscuring target, could spawn there?
                                Debug.DrawLine(player.transform.position, pos.worldPosition, Color.green);
                                bestCandidates.Add(pos);
                            }
                            else
                            {
                                Debug.DrawLine(player.transform.position, pos.worldPosition, Color.red);
                            }
                        }
                    }
                }
                if (bestCandidates.Count > 0)
                {
                    SetStagedAreaPosition(bestCandidates[0].heightmapPosition);
                }
            }
            if (Input.GetKeyDown(KeyCode.G) && foundPosition)
            {
                //lookForNextSA = false;
                CreateStagedArea();
            }
        }

        //TODO: Make sure lookForNextSA flags as true
        //if (saNum < stagedAreas.Length)
        //{
        //    if (lookForNextSA)
        //    {
        //        //Debug.Log("looking for SA " + saNum);
        //        // If enough time has passed since last SA
        //        if (Time.time - timeAtLastSA >= timeBetweenEvents[saNum])
        //        {
        //            //Debug.Log("enough time passed");
        //            float distance = Vector3.Distance(player.transform.position, positionAtLastSA);

        //            // If you are far enough away from last SA
        //            if (distance >= distanceBetweenSAs[saNum])
        //            {
        //                //Debug.Log("enough distance");
        //                lookForNextSA = false;
        //                // SearchForCandidates(playerPos);
        //            }
        //        }
        //    }
        //}

        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    SearchForCandidates(playerPos);
        //}
        //if (candidatesReady)
        //{
        //    if (candidates.Count > 0)
        //    {
        //        Debug.Log("Assessing Candidates");
        //        AssessCandidates();
        //    }
        //    else
        //    {
        //        Debug.Log("No Candidates");
        //        SearchForCandidates(playerPos);
        //    }

        //}
        //if (foundPosition)
        //{
        //    foundPosition = false;
        //    Debug.Log("Spawning SA " + saNum);
        //    CreateStagedArea();
        //}
    }

    internal void FindWeenieLocation(Vector3 position, int weenieIdx)
    {
        if (weenieIdx > stagedAreas.Length - 1)
        {
            return;
        }
        GameObject SAPrefab = stagedAreas[weenieIdx];
        StagedArea SA = stagedAreas[weenieIdx].GetComponent<StagedArea>();
        Vector2 stagedAreaSize = SA.size;
        StartCoroutine(TerrainManager.Instance.GetPainter().RemoveTreesInArea(position.XZ(), stagedAreaSize));
        int hmX = (int)Utils.Map(position.x, 0, TerrainManager.Instance.TerrainData.size.x, 0, TerrainManager.Instance.HeightmapResolution);
        int hmY = (int)Utils.Map(position.z, 0, TerrainManager.Instance.TerrainData.size.z, 0, TerrainManager.Instance.HeightmapResolution);
        StartCoroutine(TerrainManager.Instance.GetTerrainGenerator().FlattenAreaAroundPoint(hmY, hmX, 0.9f, stagedAreaSize, position, SAPrefab, SA.flattenType, SpawnWeenie));

        //SpawnWeenie(position, SAPrefab);
    }

    private static void SpawnWeenie(Vector3 position, GameObject SAPrefab)
    {
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
        StartCoroutine(FindBestPosition(temp_candidates, new Vector4(5, 1, 1, 2).normalized, SetStagedAreaPosition));
    }
    public void SearchForCandidates(StagedArea stagedArea)
    {
        temp_candidates = new List<StagedAreaCandidatePosition>();
        candidatesReady = false;
        //TODO: Get Values from SAs
        Debug.Log("Searching for Candidates for " + stagedArea.name);
        StartCoroutine(AssessSpawnPointsWithCallback(stagedArea, SetCandidatesInDictionary));
    }

    private IEnumerator AssessSpawnPointsWithCallback(StagedArea stagedArea, Action<StagedArea, List<StagedAreaCandidatePosition>> addCandidatesCallback)
    {
        float minHeight = stagedArea.minHeight;
        float maxHeight = stagedArea.maxHeight;
        float minSlope = stagedArea.minSlope;
        float maxSlope = stagedArea.maxSlope;

        Vector2 area = stagedArea.size;

        float targetHeight = minHeight + ((maxHeight - minHeight) / 2);
        float targetSlope = minSlope + ((maxSlope - minSlope) / 2);

        float[,] heightmap = TerrainManager.Instance.GetHeightmap(false);


        List<StagedAreaCandidatePosition> candidates = new List<StagedAreaCandidatePosition>();
        int progress = 0;
        stagedArea.progress.Value = progress;
        for (int y = (int)area.y / 2; y < heightmap.GetLength(1) - (int)(area.y / 2); y++)
        {
            for (int x = (int)area.x / 2; x < heightmap.GetLength(0) - (int)(area.x / 2); x++)
            {
                stagedArea.progress.Value = (float)++progress / (heightmap.GetLength(0) * heightmap.GetLength(1));
                if (IsValidPoint(minHeight, maxHeight, minSlope, maxSlope, heightmap, y, x))
                {
                    float totalScore = 0;
                    float heightScore = 0;
                    float slopeScore = 0;
                    Vector2 pointPosition = new Vector2(x, y);

                    for (int ny = -(int)area.y / 2; ny < (int)area.y / 2; ny++)
                    {
                        for (int nx = -(int)area.x / 2; nx < (int)area.x / 2; nx++)
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
                            y / (float)TerrainManager.Instance.TerrainData.heightmapResolution * TerrainManager.Instance.TerrainData.size.z,
                            heightmap[(int)x, (int)y] * TerrainManager.Instance.TerrainData.size.y,
                            x / (float)TerrainManager.Instance.TerrainData.heightmapResolution * TerrainManager.Instance.TerrainData.size.x
                        ),
                        heightScore = heightScore,
                        slopeScore = slopeScore,
                    });
                }

            }
            yield return null;
        }
        stagedArea.progress.Value = 0;
        stagedArea.candidatesReady = true;
        addCandidatesCallback(stagedArea, candidates);
        yield break;
    }

    private void SetCandidatesInDictionary(StagedArea stagedArea, List<StagedAreaCandidatePosition> candidates)
    {
        Debug.Log("Setting Candidates");
        candidatesReady = true;
        //temp_candidates = candidates_in;
        Debug.Log(string.Format("found {0} candidages for {1}", candidates.Count, stagedArea.name));
        StagedAreaCandidates.Add(stagedArea, candidates);
    }

    private List<StagedAreaCandidatePosition> PossibleSpawnPoints(Vector2 playerPosition, Vector2 stagedAreaSize, float minHeight, float maxHeight, float minSlope, float maxSlope)
    {
        float targetHeight = minHeight + ((maxHeight - minHeight) / 2);
        float targetSlope = minSlope + ((maxSlope - minSlope) / 2);

        int r = 50;
        float[,] heightmap = TerrainManager.Instance.GetHeightmap(false);
        int mappedY = (int)Utils.Map(playerPosition.x, 0, TerrainManager.Instance.TerrainData.size.x, 0, heightmap.GetLength(0));
        int mappedX = (int)Utils.Map(playerPosition.y, 0, TerrainManager.Instance.TerrainData.size.z, 0, heightmap.GetLength(1));


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
                            y / (float)TerrainManager.Instance.TerrainData.heightmapResolution * TerrainManager.Instance.TerrainData.size.z,
                            heightmap[(int)x, (int)y] * TerrainManager.Instance.TerrainData.size.y,
                            x / (float)TerrainManager.Instance.TerrainData.heightmapResolution * TerrainManager.Instance.TerrainData.size.x
                        ),
                        heightScore = heightScore,
                        slopeScore = slopeScore,
                    });
                }
            }
        }
        return candidates;
    }

    private IEnumerator FindBestPosition(List<StagedAreaCandidatePosition> candidates, Vector3 weights, Action<Vector2> setPositionCallback)
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

                ) / 3;
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
            position.y / (float)TerrainManager.Instance.HeightmapResolution * TerrainManager.Instance.TerrainData.size.z,
            heightmap[(int)position.x, (int)position.y] * TerrainManager.Instance.TerrainData.size.y,
            position.x / (float)TerrainManager.Instance.HeightmapResolution * TerrainManager.Instance.TerrainData.size.x
            );
        InstantiateStagedArea(worldSpacePos);
    }

    public void InstantiateStagedArea(Vector3 position)
    {
        if (saNum < stagedAreas.Length)
        {

            GameObject stagedArea = stagedAreas[saNum];
            StagedArea nextSA = stagedArea.GetComponent<StagedArea>();
            if (!stagedArea.activeSelf)
            {
                Debug.Log("Activating " + stagedArea.name);

                Vector2 stagedAreaSize = nextSA.size;
                StartCoroutine(TerrainManager.Instance.GetPainter().RemoveTreesInArea(position.XZ(), stagedAreaSize));
                int hmX = (int)Utils.Map(position.x, 0, TerrainManager.Instance.TerrainData.size.x, 0, TerrainManager.Instance.HeightmapResolution);
                int hmY = (int)Utils.Map(position.z, 0, TerrainManager.Instance.TerrainData.size.z, 0, TerrainManager.Instance.HeightmapResolution);
                StartCoroutine(TerrainManager.Instance.GetTerrainGenerator().FlattenAreaAroundPoint(hmY, hmX, nextSA.flattenPower, stagedAreaSize, position, stagedArea, nextSA.flattenType, SpawnWeenie));
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
        float slope = TerrainManager.Instance.TerrainData.GetSteepness(
            x / (float)TerrainManager.Instance.TerrainData.alphamapResolution,
            y / (float)TerrainManager.Instance.TerrainData.alphamapResolution);
        bool isInSlopeRange = slope > minSlope && slope < maxSlope;
        return isInHeightRange && isInSlopeRange;
    }

    private float ScorePointValidity(int x, int y, float[,] hm, float targetHeight, float targetSlope, out float heightScore, out float slopeScore)
    {
        float height = hm[x, y];
        float slope = TerrainManager.Instance.TerrainData.GetSteepness(
            x / (float)TerrainManager.Instance.TerrainData.alphamapResolution,
            y / (float)TerrainManager.Instance.TerrainData.alphamapResolution);

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
}
