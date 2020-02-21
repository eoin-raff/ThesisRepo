using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public DrawMode DrawMode;

    public NoiseData NoiseData;
    public TerrainData TerrainData;
    public TextureData TextureData;

    public Material TerrainMaterial;

    [Range(0, 6)]
    public int EditorPreviewLOD;


    public bool AutoUpdate;

    float[,] falloffMap;

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
    public int MapChunkSize
    {
        /*
         * Size of 240 allows for LOD values of 2, 4, 6, 8, 10 & 12
         * Size of 96 allows for all of these except 10
         * 
         * 240 would go above permitted vertices when using flat shading, so we need to use 96 in that case
         * we return less than those values so that we can use border calculations
         */
        get
        {
            if (TerrainData.UseFlatShading)
            {
                return 95;
            }
            else
            {
                return 239;
            }
        }
    }

    Queue<MapThreadInfo<MapData>> MapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> MeshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(centre, callback);
        };
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);
        lock (MapDataThreadInfoQueue)
        {
            MapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(
            mapData.heightMap,
            TerrainData.MeshHeightMultiplier,
            TerrainData.MeshHeightCurve, 
            lod,
            TerrainData.UseFlatShading);
        lock (MeshDataThreadInfoQueue)
        {
            MeshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        if (MapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < MapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = MapDataThreadInfoQueue.Dequeue();
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

    MapData GenerateMapData(Vector2 centre)
    {
        float[,] noiseMap = Noise.GenerateMap(
            MapChunkSize + 2, //adds border
            MapChunkSize + 2, //adds border
            NoiseData.Seed,
            NoiseData.NoiseScale,
            NoiseData.Octaves,
            NoiseData.Persistance,
            NoiseData.Lacunarity, 
            centre + NoiseData.Offset,
            NoiseData.normalizeMode);

        if (TerrainData.UseFalloff)
        {
            if (falloffMap == null)
            {
                falloffMap = FalloffGenerator.GenerateFalloffMap(MapChunkSize + 2);
            }

            for (int y = 0; y < MapChunkSize + 2; y++)
            {
                for (int x = 0; x < MapChunkSize + 2; x++)
                {
                    if (TerrainData.UseFalloff)
                    {
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                    }
                }
            }

        }


        return new MapData(noiseMap);
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (DrawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightmap(mapData.heightMap));
        }
        else if (DrawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, TerrainData.MeshHeightMultiplier, TerrainData.MeshHeightCurve, EditorPreviewLOD, TerrainData.UseFlatShading));
        }
        else if (DrawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightmap(FalloffGenerator.GenerateFalloffMap(MapChunkSize)));
        }
    }

    private void OnValidate()
    {
        if (TerrainData != null)
        {
            TerrainData.OnValuesUpdated -= OnValuesUpdated; //if not subscribed, then this does nothing. If you are subscribed, it stops multiple calls being made
            TerrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if (NoiseData != null)
        {
            NoiseData.OnValuesUpdated -= OnValuesUpdated;
            NoiseData.OnValuesUpdated += OnValuesUpdated;
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

public struct MapData
{
    public readonly float[,] heightMap;

    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }
}

public enum DrawMode
{
    NoiseMap,
    Mesh,
    FalloffMap
};