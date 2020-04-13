using UnityEngine;
using MED10.PCG;
using System.Collections.Generic;


public class BlobAnalysis : MonoBehaviour
{

    private float[,] inputPoints; // The input from the terrain
    
    public int[,] blobResult; // The output map with the labels

    private int[,] thresholdedPoints; // Output after being thresholded

    public float testLevel; // The level at which the ocean starts
    public float requiredSize; // The minimum size one blob needs to be to pass

    private List<int> neighborsY = new List<int>();
    private List<int> neighborsX = new List<int>();


    public void TestForIslands()    // Call this function to test whether a generated terrain can be used
    {
        inputPoints = GetTerrainPoints();       // Call method to get the terrain points from the TerrainGenerator script
        blobResult = new int[inputPoints.GetLength(0), inputPoints.GetLength(1)];
        
        thresholdedPoints = Threshold(inputPoints, testLevel);  // Threshold the terreainpoints to prepare for grassfire

        Debug.Log("Done Thresholding");

        SequentialGrassfire();      // Runs grassfire which labels blobs in blobResult variable

        Debug.Log("Done Labeling Blobs");
       
        bool isPassable = CalculateUsability(blobResult);   // Calculates the usability of the terrain by checking if any blobs are bigger than a certain threshold

        Debug.Log("Terrain is useful: " + isPassable);
    }


    private float[,] GetTerrainPoints() 
    {
        float[,] input = GetComponent<TerrainGenerator>().GetHeightMap(false);
        return input;
    }


    private int[,] Threshold(float[,] input, float threshold)
    {

        int[,] output = new int[input.GetLength(0), input.GetLength(1)];

        for (int y = 0; y < input.GetLength(0); y++)
        {
            for (int x = 0; x < input.GetLength(1); x++)
            {
                if (input[y, x] >= threshold)
                {
                    output[y, x] = 1;
                }
                else
                {
                    output[y, x] = 0;
                }
            }
        }
        return output;
    }


    void SequentialGrassfire()
    {
        Debug.Log("Running Sequential Grassfire!");
        
        int label = 1;

        for (int y = 1; y < thresholdedPoints.GetLength(0) - 1; y++)
        {
            for (int x = 1; x < thresholdedPoints.GetLength(1) - 1; x++)
            {
                if (thresholdedPoints[y, x] == 1)
                {
                    Grassfire(y, x, label);
                    for (int i = 0; i < neighborsY.Count; i++)
                    {
                        Grassfire(neighborsY[0], neighborsX[0], label);
                    }
                    label++;
                    neighborsX.Clear();
                    neighborsY.Clear();
                }
            }
        }

        Debug.Log("amount of blobs: " + label);
    }
    
    void Grassfire(int y, int x, int label)
    {

        blobResult[y, x] = label; // Label in output
        
        thresholdedPoints[y, x] = 0; // Burn in input

        if (y > 0 && thresholdedPoints[(y - 1), x] == 1) // above
        {
            blobResult[y - 1, x] = label; // Label in output
            thresholdedPoints[(y - 1), x] = 0; // Burn in input
            neighborsY.Add(y - 1); // add to the list of neighbors
            neighborsX.Add(x);
        }
        if (thresholdedPoints[(y + 1), x] == 1) // below
        {
            blobResult[y + 1, x] = label;
            thresholdedPoints[(y + 1), x] = 0;
            neighborsY.Add(y + 1);
            neighborsX.Add(x);
        }
        if (thresholdedPoints[y, (x - 1)] == 1) // Left
        {
            blobResult[y, x - 1] = label;
            thresholdedPoints[y, (x - 1)] = 0;
            neighborsY.Add(y);
            neighborsX.Add(x - 1);
        }
        if (thresholdedPoints[y, (x + 1)] == 1) // Right
        {
            blobResult[y, x + 1] = label;
            thresholdedPoints[y, (x + 1)] = 0;
            neighborsY.Add(y);
            neighborsX.Add(x + 1);
        }

    }  


    bool CalculateUsability(int[,] input)    // If one blob covers e.g. 75 % of the terrain we can pass the generated terrain
    {
        List<float> blobAreas = new List<float>();

        float fullArea = input.GetLength(0) * input.GetLength(1);

        for (int y = 1; y < input.GetLength(0) - 1; y++)
        {
            for (int x = 1; x < input.GetLength(1) - 1; x++)
            {
                if (blobAreas.Contains(blobResult[y, x]))
                {
                    blobAreas[blobResult[y, x]]++;
                }
                else
                {
                    blobAreas.Add(blobResult[y, x]);
                }
            }
        }

        float[] blobAreaPercentages = new float[blobAreas.Count];

        for (int i = 0; i < blobAreas.Count; i++)
        {
            blobAreaPercentages[i] = blobAreas[i] / fullArea;

            if (blobAreaPercentages[i] >= requiredSize)
            {
                Debug.Log("Percentage of area covered by blolb: " + blobAreaPercentages[i]);

                return true;
            }
        }
        return false;
    }




// Code for running a recursive grassfire. Should work, but throws a stack overflow!

/*    
private void RecursiveGrassfire()
{
    Debug.Log("Running Recursive grassfire");

    int label = 1;
    for (int y = 1; y < thresholdedPoints.GetLength(0) - 1; y++)
    {
        for (int x = 1; x < thresholdedPoints.GetLength(1) - 1; x++)
        {
            if (thresholdedPoints[y, x] == 1)
            {
                RecGrassfire(y, x, label);
                label++;
            }
        }
    }
}


void RecGrassfire(int y, int x, int label)
{
    blobResult[y, x] = label;

    if (y > 0 && thresholdedPoints[(y - 1), x] == 1)
        RecGrassfire(y - 1, x, label);
    if (thresholdedPoints[(y + 1), x] == 1)
        RecGrassfire(y + 1, x, label);
    if (thresholdedPoints[y, (x - 1)] == 1)
        RecGrassfire(y, x - 1, label);
    if (thresholdedPoints[y, (x + 1)] == 1)
        RecGrassfire(y, x + 1, label);
}*/
}
