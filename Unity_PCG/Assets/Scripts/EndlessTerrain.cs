using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float SquareThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODInfo[] DetailLevels;
    public static float MaxViewDistance;

    public Transform Viewer;
    public Material MapMaterial;

    public static Vector2 ViewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator MapGenerator;
    int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> TerrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        MapGenerator = FindObjectOfType<MapGenerator>();
        MaxViewDistance = DetailLevels[DetailLevels.Length - 1].VisibleDistanceThreshold;
        chunkSize = MapGenerator.MapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt( MaxViewDistance / chunkSize);

        UpdateVisibleChunks();

    }

    private void Update()
    {
        ViewerPosition = new Vector2(Viewer.position.x, Viewer.position.z) / MapGenerator.TerrainData.UniformScale;
        if ((viewerPositionOld - ViewerPosition).sqrMagnitude > SquareThresholdForChunkUpdate )
        {
            viewerPositionOld = ViewerPosition;
            UpdateVisibleChunks(); 
        }
    }

    private void UpdateVisibleChunks()
    {
        for (int i = 0; i < TerrainChunksVisibleLastUpdate.Count; i++)
        {
            TerrainChunksVisibleLastUpdate[i].SetVisibile(false);
        }
        TerrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(ViewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(ViewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset < chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset < chunksVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
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
                        transform, 
                        MapMaterial));
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        LODMesh collisionLODMesh;

        MapData mapData;
        bool mapDataReceived;

        int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;
            position = coord * size;
            bounds = new Bounds(position, Vector3.one * size); 
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshObject.transform.position = positionV3 * MapGenerator.TerrainData.UniformScale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * MapGenerator.TerrainData.UniformScale;
            SetVisibile(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].LOD, UpdateTerrainChunk);
                if (detailLevels[i].UseForCollider)
                {
                    collisionLODMesh = lodMeshes[i];
                }
            }

            MapGenerator.RequestMapData(position, OnMapDataRecieved);
        }

        void OnMapDataRecieved(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.MapChunkSize, MapGenerator.MapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(ViewerPosition));
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
                    if (lodIndex == 0)
                    {
                        if (collisionLODMesh.HasMesh)
                        {
                            meshCollider.sharedMesh = collisionLODMesh.Mesh;
                        }
                        else if (!collisionLODMesh.HasRequestedMesh)
                        {
                            collisionLODMesh.RequestMesh(mapData);
                        }
                    }

                    TerrainChunksVisibleLastUpdate.Add(this);
                    
                }

                SetVisibile(visibile);
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
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            Mesh = meshData.CreateMesh();
            HasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            HasRequestedMesh = true;
            MapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int LOD;
        public float VisibleDistanceThreshold;
        public bool UseForCollider;
    }
}
