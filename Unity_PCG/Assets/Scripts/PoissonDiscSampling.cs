using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public static class PoissonDiscSampling
{
    public static List<Vector3> GeneratePoints(float radius, Vector2 centre, HeightMap heightmap, int numSamplesBeforeRejection = 30)
    {
        float testRad = radius;
        HeightMap testHM = heightmap;
        float[,] testVals = heightmap.Values;
        int testX = testVals.GetLength(0);
        int testY = testVals.GetLength(1);
        Vector2 testV2 = new Vector2(testX, testY);


        List<Vector2> points2D = GeneratePoints(radius, new Vector2(heightmap.Values.GetLength(0), heightmap.Values.GetLength(1)), numSamplesBeforeRejection);
        List<Vector3> points3D = new List<Vector3>();
        foreach (Vector2 vector2 in points2D)
        {
            //convert to vector3
            Vector3 newPoint = new Vector3(
                vector2.x + (centre.x) - heightmap.Values.GetLength(0)/2,
                heightmap.MaxValue + 1,
                vector2.y + (centre.y) - heightmap.Values.GetLength(1)/2);
            points3D.Add(newPoint);
        }
        return points3D;
    }

    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30)
    {
        Random rand = new Random();
        float cellSize = radius / Mathf.Sqrt(2);

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        spawnPoints.Add(sampleRegionSize/2); //just start in the middle, could be random either
        while (spawnPoints.Count > 0)
        {
            int spawnIndex = rand.Next(spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = (float)rand.NextDouble() * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCentre + dir * Mathf.Lerp(radius, 2*radius, (float)rand.NextDouble());
                if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)((candidate.y) / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }
        return points;
    }

    static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        //Check if the candidate is within the sample region
        if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
        {
            // Search in a 5x5 grid, where the grid space containing the candidate is the centre
            // The radius of the disc is the diagonal of the cell.
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    // The grid contains point indexes + 1
                    // i.e. 0 means no point, 1 means points[0], 2 means points[1] etc...
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        //if there is a tree, check if it is within the disk radius.
                        //it it is, return false as this is not a valid point
                        float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                        if (sqrDst < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }
            // After searching the 5x5 grid, if you have found no points within the radius,
            // return true, as this is a valid point
            return true;
        }
        // If it is not in the sample region then it is not a valid point
        return false;
    }
}
