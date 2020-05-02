using System.Collections;
using UnityEngine;
using System;
using MED10.PCG;
using MED10.Utilities;
using System.Collections.Generic;

public class SpaceTimeManager : MonoBehaviour
{

    public GameObject player;

    public NarrativeManager narrativeManager;

    private IEnumerator cinematicCoroutine;
    private int saNum = 1;                                  // Which SA are we at (Can also be used for event in between SAs)
    public float[] timeBetweenEvents = new float[6];        // Add how much time should elapse from end of last SA until next area can be loaded

    public bool playerHasControl = false;                   // Used to control cinematic sequences. Should also be accessed from other scripts to e.g. start the game.
    private bool lookForNextSA = false;

    public int withinSA;                                    // How close should the player be to the SA before triggering cinematic sequence

    private Vector3 positionAtLastSA;
    public float[] distanceBetweenSAs;                      // How far should player travel before starting to look for next SA 
    private float timeAtLastSA;


    private void Start()
    {
        positionAtLastSA = player.transform.position;
        lookForNextSA = true;
        timeAtLastSA = Time.time;
    }

    private void Update()
    {
        if (lookForNextSA)
        {
            if (Time.time - timeAtLastSA >= timeBetweenEvents[saNum])
            {
                float distance = Vector3.Distance(player.transform.position, positionAtLastSA);
                Debug.Log("Current Distance: " + distance);

                if (distance >= distanceBetweenSAs[saNum])
                {
                    lookForNextSA = false;
                    SpaceTime();
                }
            }
        }
    }

    public void SpaceTime()                             // Call this method to initiate space-time behavior. Should probably be from another script
    {
        Debug.Log("Starting Space-Time");

        Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.z);

        List<StagedAreaCandidatePosition> candidates = narrativeManager.PossibleSpawnPoints(playerPos, new Vector2(5, 5), 0, 1, 0, 1);

        if (narrativeManager.FindBestPosition(candidates, new Vector4(5, 1, 1, 2).normalized, out Vector2 spawnPosition))
        {
            narrativeManager.InstantiateStagedArea(spawnPosition);
            positionAtLastSA = player.transform.position;
            //cinematicCoroutine = WaitToStartCinematic(spawnPosition);
            //StartCoroutine(cinematicCoroutine);
            Debug.Log("SA " + saNum + " Instantiated!");
            saNum++;
            lookForNextSA = true;                             // Move this to cinematicsequence if that gets working
            timeAtLastSA = Time.time;
        }
    }


    private IEnumerator WaitToStartCinematic(Vector2 locationOfSA)        // Check if the player is close enough to the SA to start the cinematic sequence
    {

        if (player.transform.position.x >= locationOfSA.x - withinSA &&
            player.transform.position.x <= locationOfSA.x + withinSA &&
            player.transform.position.y >= locationOfSA.y - withinSA &&
            player.transform.position.y <= locationOfSA.y + withinSA)
        {
            Debug.Log("Starting cinematic sequence...");
            CinematicSequence(locationOfSA);
            yield return null;
        }
        else
        {
            Debug.Log("NOT starting cinematic sequence...");
            yield return WaitToStartCinematic(locationOfSA);
        }
    }


    private void CinematicSequence(Vector2 target)
    {
        playerHasControl = false;

        float speed = 5.0f;                 // this is the speed at which the camera moves

        player.transform.position = Vector3.Lerp(transform.position, target, speed);


        LookAtSA(5.0f, target);                     // Stare at the SA before moving again
        Debug.Log("looked at SA");
        lookForNextSA = true;
    }


    private IEnumerator LookAtSA(float time, Vector2 target)                                // Controls how long the cinematic sequence lasts
    {
        player.transform.LookAt(target);
        yield return new WaitForSeconds(time);
        playerHasControl = true;
    }

}
