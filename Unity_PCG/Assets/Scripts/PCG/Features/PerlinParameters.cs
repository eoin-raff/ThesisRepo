using System;

[Serializable]
public class PerlinParameters
{
    public float xScale = 0.001f;
    public float yScale = 0.001f;
    public int xOffset = 0;
    public int yOffset = 0;
    public int octaves = 3;
    public float persistance = 0.5f;
    public float heightScale = 0.9f;
    public bool remove = false;
}

