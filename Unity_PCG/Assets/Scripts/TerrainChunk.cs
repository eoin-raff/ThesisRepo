using System.Collections.Generic;
using UnityEngine;


public class TerrainChunk
{
    const float colliderGenerationDistanceThreshold = 5;
    public event System.Action<TerrainChunk, bool> OnVisibilityChanged;
    public Vector2 Coordinate;

    GameObject meshObject;
    Vector2 sampleCentre;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    TreeSettings treeSettings;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    int colliderLODIndex;

    HeightMap heightMap;
    bool heightMapReceived;

    int previousLODIndex = -1;
    bool hasSetCollider;
    float maxViewDistance;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    Transform viewer;
    private bool HasForest;
    public List<Vector3> TreeSpawnPoints;
    private bool HasRequestedForest;

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, TreeSettings treeSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material)
    {
        Coordinate = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.treeSettings = treeSettings;
        this.viewer = viewer;

        sampleCentre = coord * meshSettings.MeshWorldSize / meshSettings.MeshScale;
        Vector2 position = coord * meshSettings.MeshWorldSize;
        bounds = new Bounds(position, Vector3.one * meshSettings.MeshWorldSize);

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

        maxViewDistance = detailLevels[detailLevels.Length - 1].VisibleDistanceThreshold;

        TreeSpawnPoints = new List<Vector3>();

    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(
            () => HeightMapGenerator.GenerateHeightMap(meshSettings.NumVertsPerLine, meshSettings.NumVertsPerLine, heightMapSettings, sampleCentre),
         OnHeightMapRecieved
        );
    }

    void OnHeightMapRecieved(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;

        UpdateTerrainChunk();
    }

    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public void UpdateTerrainChunk()
    {
        if (heightMapReceived)
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

            bool wasVisible = IsVisible();
            bool visibile = viewerDistanceFromNearestEdge <= maxViewDistance;

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
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }
                //Only generate collision if mesh is close enough to be rendered as higest LOD
            }
            if (wasVisible != visibile)
            {
                SetVisibile(visibile);
                if (OnVisibilityChanged != null)
                {
                    OnVisibilityChanged(this, visibile);
                }
            }
            if (!HasRequestedForest)
            {
                RequestPoisson(heightMap, treeSettings);
            }
        }
    }

    public void RequestPoisson(HeightMap heightMap, TreeSettings treeSettings)
    {
        HasRequestedForest = true;
        ThreadedDataRequester.RequestData(
            () => ForestGenerator.GenerateForestSpawnPoints(sampleCentre, heightMap, treeSettings),
            OnForestDataReceived);
    }

    private void OnForestDataReceived(object obj)
    {
        HasForest = true;
        TreeSpawnPoints = (List<Vector3>)obj;
        if (hasSetCollider)
        {
            PopulateFoliage();
        }
    }

    public void PopulateFoliage()
    {
        HeightMap heightMap = this.heightMap;
        float radius = treeSettings.Radius;
        int rejectionSamples = treeSettings.RejectionSamples;
        List<Vector3> unusedPoints = new List<Vector3>();
        foreach (Vector3 point in TreeSpawnPoints)
        {
            Vector3 spawnPoint;
            RaycastHit hit;
            Ray ray = new Ray(point, Vector3.down);
            if (Physics.Raycast(ray, out hit, heightMap.MaxValue + 2))
            {
                spawnPoint = hit.point;
            }
            else
            {
                unusedPoints.Add(point);
                continue;
            }


            int idx = UnityEngine.Random.Range(0, treeSettings.Prefabs.Length - 1);
            GameObject.Instantiate(
                treeSettings.Prefabs[idx],
                spawnPoint,
                Quaternion.identity,
                meshObject.transform);
        }
        foreach (Vector3 point in unusedPoints)
        {
            TreeSpawnPoints.Remove(point);
        }
        unusedPoints.Clear();

    }

    public void UpdateCollisionMesh()
    {
        if (hasSetCollider)
        {
            return;
        }
        float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

        if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].SqrVisibleDistanceThreshold)
        {
            if (!lodMeshes[colliderLODIndex].HasRequestedMesh)
            {
                lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
            }
        }

        if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
        {
            if (lodMeshes[colliderLODIndex].HasMesh)
            {
                meshCollider.sharedMesh = lodMeshes[colliderLODIndex].Mesh;
                hasSetCollider = true;
                PopulateFoliage();
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

    void OnMeshDataReceived(object meshData)
    {
        Mesh = ((MeshData)meshData).CreateMesh();
        HasMesh = true;

        UpdateCallback();
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        HasRequestedMesh = true;
        ThreadedDataRequester.RequestData(
            () => MeshGenerator.GenerateTerrainMesh(heightMap.Values, meshSettings, lod),
            OnMeshDataReceived);
    }


}
