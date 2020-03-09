using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float SquareThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    const float colliderGenerationDistanceThreshold = 5;

    public int ColliderLODIndex;
    public LODInfo[] DetailLevels;
    public static float MaxViewDistance;

    public Transform Viewer;
    public Material MapMaterial;

    public static Vector2 ViewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator MapGenerator;
    float chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> VisibleTerrainChunks = new List<TerrainChunk>();

    private void Start()
    {
        MapGenerator = FindObjectOfType<MapGenerator>();
        MaxViewDistance = DetailLevels[DetailLevels.Length - 1].VisibleDistanceThreshold;
        chunkSize = MapGenerator.MeshSettings.MeshWorldSize;
        chunksVisibleInViewDistance = Mathf.RoundToInt( MaxViewDistance / chunkSize);

        UpdateVisibleChunks();

    }

    private void Update()
    {
        ViewerPosition = new Vector2(Viewer.position.x, Viewer.position.z);
        if (ViewerPosition != viewerPositionOld)
        {
            foreach (TerrainChunk terrainChunk in VisibleTerrainChunks)
            {
                terrainChunk.UpdateCollisionMesh();
            }
        }
        if ((viewerPositionOld - ViewerPosition).sqrMagnitude > SquareThresholdForChunkUpdate )
        {
            viewerPositionOld = ViewerPosition;
            UpdateVisibleChunks(); 
        }
    }

    private void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

        for (int i = VisibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(VisibleTerrainChunks[i].Coordinate);
            VisibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(ViewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(ViewerPosition.y / chunkSize);

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
                        terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(
                            viewedChunkCoord, 
                            chunkSize, 
                            DetailLevels,
                            ColliderLODIndex,
                            transform, 
                            MapMaterial));
                    }
                }
            }
        }
    }

    public class TerrainChunk
    {
        public Vector2 Coordinate;

        GameObject meshObject;
        Vector2 sampleCentre;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        int colliderLODIndex;

        HeightMap mapData;
        bool mapDataReceived;

        int previousLODIndex = -1;
        bool hasSetCollider;

        public TerrainChunk(Vector2 coord, float meshWorldSize, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material)
        {
            Coordinate = coord;
            this.detailLevels = detailLevels;
            this.colliderLODIndex = colliderLODIndex;
            sampleCentre = coord * meshWorldSize / MapGenerator.MeshSettings.MeshScale;
            Vector2 position = coord * meshWorldSize;
            bounds = new Bounds(position, Vector3.one * meshWorldSize); 

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshObject.transform.position = new Vector3(position.x, 0, position.y);
            meshObject.transform.parent = parent;
            SetVisibile(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].LOD);
                lodMeshes[i].UpdateCallback += UpdateTerrainChunk;
                if (i == colliderLODIndex)
                {
                    lodMeshes[i].UpdateCallback += UpdateCollisionMesh;
                }
            }

            MapGenerator.RequestHeightMap(sampleCentre, OnMapDataRecieved);
        }

        void OnMapDataRecieved(HeightMap mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(ViewerPosition));

                bool wasVisible = IsVisible();
                bool visibile = viewerDistanceFromNearestEdge <= MaxViewDistance;

                if (visibile)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDistanceFromNearestEdge > detailLevels[i].VisibleDistanceThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.HasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.Mesh;

                        }
                        else if (!lodMesh.HasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        } 
                    }
                    //Only generate collision if mesh is close enough to be rendered as higest LOD
                }
                if (wasVisible != visibile)
                {
                    if (visibile)
                    {
                        VisibleTerrainChunks.Add(this);
                    }
                    else
                    {
                        VisibleTerrainChunks.Remove(this);
                    }
                    SetVisibile(visibile);
                }
            }
        }

        public void UpdateCollisionMesh()
        {
            if (hasSetCollider)
            {
                return;
            }
            float sqrDstFromViewerToEdge = bounds.SqrDistance(ViewerPosition);

            if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].SqrVisibleDistanceThreshold)
            {
                if (!lodMeshes[colliderLODIndex].HasRequestedMesh)
                {
                    lodMeshes[colliderLODIndex].RequestMesh(mapData);
                }
            }

            if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold*colliderGenerationDistanceThreshold)
            {
                if (lodMeshes[colliderLODIndex].HasMesh)
                {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].Mesh;
                    hasSetCollider = true;
                }
            }
        }

        public void SetVisibile(bool visibile)
        {
            meshObject.SetActive(visibile);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LODMesh
    {
        public Mesh Mesh;
        public bool HasRequestedMesh;
        public bool HasMesh;
        int lod;
        public event System.Action UpdateCallback;

        public LODMesh(int lod)
        {
            this.lod = lod;

        }

        void OnMeshDataReceived(MeshData meshData)
        {
            Mesh = meshData.CreateMesh();
            HasMesh = true;

            UpdateCallback();
        }

        public void RequestMesh(HeightMap mapData)
        {
            HasRequestedMesh = true;
            MapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        [Range(0,MeshSettings.NumSupportedLODs-1)]
        public int LOD;
        public float VisibleDistanceThreshold;

        public float SqrVisibleDistanceThreshold { get { return VisibleDistanceThreshold * VisibleDistanceThreshold; } }
    }
}
