using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdateableData
{
    public const int NumSupportedLODs = 5;
    public const int NumSupportedChunkSizes = 9;
    public const int NumSupportedFlatShadedChunkSizes = 3;
    public static readonly int[] SupportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    public float MeshScale = 2.5f;
    public bool UseFlatShading;

    [Range(0, NumSupportedChunkSizes - 1)]
    public int ChunkSizeIndex;
    [Range(0, NumSupportedFlatShadedChunkSizes - 1)]
    public int FlatShadedChunkSizeIndex;

    public int NumVertsPerLine
    {
        get
        {
            return SupportedChunkSizes[(UseFlatShading ? FlatShadedChunkSizeIndex : ChunkSizeIndex)] + 5;
        }
    }
    public float MeshWorldSize
    {
        get
        {
            return (NumVertsPerLine - 3) * MeshScale;
        }
    }
}
