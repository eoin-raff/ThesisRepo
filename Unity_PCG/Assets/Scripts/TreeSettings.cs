using UnityEngine;

[CreateAssetMenu()]
public class TreeSettings : ScriptableObject
{
    public GameObject[] Prefabs;
    public GameObject[] SpawnPosibility;

    public int RejectionSamples = 30;

    public float Radius = 5;

}