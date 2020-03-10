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

    private void Awake()
    {
        //points = PoissonDiscSampling.GeneratePoints(Radius, regionSize, rejectionSamples);
    }

    private void Start()
    {
        Debug.Assert(points != null, "No Points", this);
        if (points!=null)
        {
            foreach (Vector2 point in points)
            {
                Instantiate(
                    GameObject.CreatePrimitive(PrimitiveType.Sphere),
                    new Vector3(point.x, 0, point.y),
                    Quaternion.identity,
                    transform);
            }
        }
    }
}
