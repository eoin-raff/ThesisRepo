using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NarrativeManager : MonoBehaviour
{
    public Camera playerCam;        // Add the camera from the player object to check if a possible location is in sight

    // Prefabs for staged areas - Should be designed first and then instatiated at a possible location
    public GameObject[] stagedAreas = new GameObject[5];      

    // Requirements for placement of SAs
    public SAParameters[] requirementSA = new SAParameters[5];

    // Time management variables
    private IEnumerator timeCoroutine;
    private int eventNum;                               // Which event are we at
    public float[] timeBetweenEvents = new float[5];    // Add how much time should elapse from start until this staged area can be loaded


    public void SpaceTime()                             // Call this method to initiate space-time behavior. Should probably be from another script
    {
        eventNum = 0;
        timeCoroutine = TimeManager(eventNum);
        StartCoroutine(timeCoroutine);
    }


    private IEnumerator TimeManager(int num)            // Manage time before next event has to spawn
    { 

        if (Time.timeSinceLevelLoad >= timeBetweenEvents[num])
        {
            CreateStagedArea(num);

            yield return TimeManager(eventNum);
        }
    }


    void CreateStagedArea(int numSA)
    {
        int scale = requirementSA[numSA].scale;
        float slope = requirementSA[numSA].slope;
        float height = requirementSA[numSA].height;

        // Check VE within some range for places that fits the requirements and add them to this list
        Transform[] possibleSALocations = PossibleSpawnPoints(playerCam.transform.position.x, playerCam.transform.position.y, scale, slope, height);

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

                eventNum++;         // Start looking into next SA in the coroutine

                break;
            }
        }
    }


    Transform[] PossibleSpawnPoints(float x, float y, int scale, float height, float slope)
    {
        Transform[] possiblePlaces = new Transform[10];

        int r = 50;

        // Search in an area of r radius from the position denoted by x and y which is the current player position
        // Find first 10 suitable areas that fulfill scale, height, and slope
        // Make sure a found area can't be found again
        // Return up to 10 areas where spawning can happen

        return possiblePlaces;               // Return center of found area
    }
}


// Class for holding the parameter of the staged areas
[Serializable]
public class SAParameters : TableItem
{
    public int scale = 0;
    public float slope = 0.0f;
    public float height = 0.0f;
}
