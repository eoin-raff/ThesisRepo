using System.Collections;
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
    public float[] timeBetweenEvents = new float[5];    // Add how much time should elapse from end of last SA until next area can be loaded

 
    public bool playerHasControl = false;               // Used to control cinematic sequences. Should also be accessed from other scripts to e.g. start the game.
    private bool lookForNextSA = false;

    public int withinSA;                                // How close should the player be to the SA before triggering cinematic sequence

    private Vector3 positionAtLastSA;
    public float distanceBetweenSAs;                    // How far should player travel before starting to look for next SA 

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
        int scale = requirementSA[numSA].scale;
        float slope = requirementSA[numSA].slope;
        float height = requirementSA[numSA].height;

        // Check VE within some range for places that fits the requirements and add them to this array
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

                WaitToStartCinematic(possibleSALocations[i]);                     // Start preparing for using cinematic

                eventNum++;         // Start looking into next SA in the coroutine

                positionAtLastSA = playerCam.transform.position;

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
    public int scale = 0;
    public float slope = 0.0f;
    public float height = 0.0f;
}
