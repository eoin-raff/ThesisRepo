using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonTest : MonoBehaviour
{
    public float Radius = 1f;
    public Vector2 regionSize = Vector2.one;
    public int rejectionSamples = 30;
    public float displayRadius = 1f;

    List<Vector2> points;

    private void OnValidate()
    {
        Debug.Log("Generating Poisson Points");
        points = PoissonDiscSampling.GeneratePoints(Radius, regionSize, rejectionSamples);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(regionSize / 2, regionSize);
        Debug.Assert(points != null, "No Points");
        if (points!=null)
        {
            Debug.Log(points.Count);
            foreach (Vector2 point in points)
            {
                Gizmos.DrawSphere(point, displayRadius);
            }
        }
    }
}
