using MED10.Architecture.Events;
using MED10.Architecture.Variables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagedArea : MonoBehaviour
{
    private bool stagedAreaStarted = false;
    private bool stagedAreaEnded = false;

    //public IntVariable StagedAreaIndex;

    public Vector2 size;
    [Range(0, 1)]
    public float minHeight;
    [Range(0, 1)] 
    public float maxHeight;
    [Range(0, 90)]
    public float minSlope;
    [Range(0, 90)]
    public float maxSlope;
    [Range(0, 1)]
    public float flattenPower;

    public GameEvent EnteredStagedArea;
    public GameEvent ExitStagedArea;

    private void OnTriggerEnter(Collider other)
    {
        if (!stagedAreaStarted)
        {
//StagedAreaIndex.Value++;
            stagedAreaStarted = true;
            EnteredStagedArea.Raise();
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (stagedAreaStarted && !stagedAreaEnded)
        {
            stagedAreaEnded = true;
            ExitStagedArea.Raise();
        }
    }
}
