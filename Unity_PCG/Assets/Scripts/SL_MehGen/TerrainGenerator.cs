using System;
using System.Collections.Generic;
using UnityEngine;

[Obsolete()]
public class TerrainGenerator : MonoBehaviour
{

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float SquareThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;


    public int ColliderLODIndex;
    public LODInfo[] DetailLevels;

    public MeshSettings MeshSettings;
    public HeightMapSettings HeightMapSettings;
    public TextureData TextureSettings;

    public Transform Viewer;
    public Material TerrainMaterial;

    Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    float chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private void Start()
    {
        TextureSettings.ApplyToMaterial(TerrainMaterial);
        TextureSettings.UpdateMeshHeights(TerrainMaterial, HeightMapSettings.MinHeight, HeightMapSettings.MaxHeight);


        float maxViewDistance = DetailLevels[DetailLevels.Length - 1].VisibleDistanceThreshold;
        chunkSize = MeshSettings.MeshWorldSize;
        chunksVisibleInViewDistance = Mathf.RoundToInt( maxViewDistance / chunkSize);

        UpdateVisibleChunks();

    }

    private void Update()
    {
        viewerPosition = new Vector2(Viewer.position.x, Viewer.position.z);
        if (viewerPosition != viewerPositionOld)
        {
            foreach (TerrainChunk terrainChunk in visibleTerrainChunks)
            {
                terrainChunk.UpdateCollisionMesh();
            }
        }
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > SquareThresholdForChunkUpdate )
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks(); 
        }
    }

    private void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].Coordinate);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset < chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset < chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk newChunk = new TerrainChunk(
                            viewedChunkCoord, 
                            HeightMapSettings, 
                            MeshSettings,
                            DetailLevels,
                            ColliderLODIndex,
                            transform, 
                            Viewer.transform,
                            TerrainMaterial);

                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.OnVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }
            }
        }
    }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible)
        {
            visibleTerrainChunks.Add(chunk);
        }
        else
        {
            visibleTerrainChunks.Remove(chunk);
        }
    }
}

[Obsolete()]
[System.Serializable]
public struct LODInfo
{
    [Range(0, MeshSettings.NumSupportedLODs - 1)]
    public int LOD;
    public float VisibleDistanceThreshold;

    public float SqrVisibleDistanceThreshold { get { return VisibleDistanceThreshold * VisibleDistanceThreshold; } }
}