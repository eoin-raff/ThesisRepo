using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public DrawMode DrawMode;

    public MeshSettings MeshSettings;
    public HeightMapSettings HeightMapSettings;
    public TextureData TextureData;

    public Material TerrainMaterial;



    [Range(0, MeshSettings.NumSupportedLODs-1)]
    public int EditorPreviewLOD;

    

    public bool AutoUpdate;

    float[,] falloffMap;


    Queue<MapThreadInfo<HeightMap>> HeightMapThreadInfoQueue = new Queue<MapThreadInfo<HeightMap>>();
    Queue<MapThreadInfo<MeshData>> MeshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private void Start()
    {
        //not correct if using 
        TextureData.ApplyToMaterial(TerrainMaterial);
        TextureData.UpdateMeshHeights(TerrainMaterial, HeightMapSettings.MinHeight, HeightMapSettings.MaxHeight);

    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }
    void OnTextureValuesUpdated()
    {
        TextureData.ApplyToMaterial(TerrainMaterial);
    }

    public void RequestHeightMap(Vector2 centre, Action<HeightMap> callback)
    {
        ThreadStart threadStart = delegate
        {
            HeightMapThread(centre, callback);
        };
        new Thread(threadStart).Start();
    }

    void HeightMapThread(Vector2 centre, Action<HeightMap> callback)
    {
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(MeshSettings.NumVertsPerLine, MeshSettings.NumVertsPerLine, HeightMapSettings, centre);
        lock (HeightMapThreadInfoQueue)
        {
            HeightMapThreadInfoQueue.Enqueue(new MapThreadInfo<HeightMap>(callback, heightMap));
        }
    }

    public void RequestMeshData(HeightMap heightMap, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(heightMap, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(HeightMap heightMap, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(
            heightMap.Values,
            MeshSettings,
            lod);
        lock (MeshDataThreadInfoQueue)
        {
            MeshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        if (HeightMapThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < HeightMapThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<HeightMap> threadInfo = HeightMapThreadInfoQueue.Dequeue();
                threadInfo.Callback(threadInfo.parameter);
            }
        }
        if (MeshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < MeshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = MeshDataThreadInfoQueue.Dequeue();
                threadInfo.Callback(threadInfo.parameter);
            }
        }
    }


    public void DrawMapInEditor()
    {
        TextureData.UpdateMeshHeights(TerrainMaterial, HeightMapSettings.MinHeight, HeightMapSettings.MaxHeight);

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(MeshSettings.NumVertsPerLine, MeshSettings.NumVertsPerLine, HeightMapSettings, Vector2.zero);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (DrawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightmap(heightMap.Values));
        }
        else if (DrawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.Values, MeshSettings, EditorPreviewLOD));
        }
        else if (DrawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightmap(FalloffGenerator.GenerateFalloffMap(MeshSettings.NumVertsPerLine)));
        }
    }

    private void OnValidate()
    {
        if (MeshSettings != null)
        {
            MeshSettings.OnValuesUpdated -= OnValuesUpdated; //if not subscribed, then this does nothing. If you are subscribed, it stops multiple calls being made
            MeshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (HeightMapSettings != null)
        {
            HeightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            HeightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (TextureData != null)
        {
            TextureData.OnValuesUpdated -= OnTextureValuesUpdated;
            TextureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> Callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            Callback = callback;
            this.parameter = parameter;
        }
    }
}


public enum DrawMode
{
    NoiseMap,
    Mesh,
    FalloffMap
};