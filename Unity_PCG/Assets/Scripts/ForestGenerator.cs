using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ForestGenerator
{
    public static List<Vector3> GenerateForestSpawnPoints(Vector2 centre, HeightMap heightMap, TreeSettings treeSettings)
    {
       return PoissonDiscSampling.GeneratePoints(treeSettings.Radius, centre, heightMap, treeSettings.RejectionSamples);
    }
}
