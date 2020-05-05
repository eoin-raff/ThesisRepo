using MED10.Utilities;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Collections;

namespace MED10.PCG
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(TerrainManager))]

    public class TerrainGenerator : MonoBehaviour
    {
        public enum SeedType { Fixed, Random };
        public SeedType seedType = SeedType.Fixed;
        public int fixedSeed = 0;
        private int seed;


        public Vector2 randomHeightRange = new Vector2(0, 0.1f);
        public Texture2D heightMapImage;
        public Vector3 heightMapScale = new Vector3(2, 0.5f, 2);
        
        [SerializeField]
        private bool resetTerrain = true;

        #region Perlin Noise
        public float perlinScaleX = 0.001f;
        public float perlinScaleY = 0.001f;
        public int perlinOffsetX = 0;
        public int perlinOffsetY = 0;
        public int perlinOctaves = 3;
        public float perlinPersistance = 0.5f;
        public float perlinHeightScale = 0.5f;

        #endregion
        #region Multiple Perlin Noise
        public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
    {
        new PerlinParameters()
    };

        #endregion

        #region Voronoi
        public int voronoiPeakCount;
        public float voronoiFallOff;
        public float voronoiDropOff;
        public float voronoiMinHeight;
        public float voronoiMaxHeight;
        public enum VoronoiType { Linear, Power, Combined }
        public VoronoiType voronoiType = VoronoiType.Linear;

        #endregion
        #region Midpoint Displacement
        public float MPminHeight;
        public float MPmaxHeight;
        public float MProughness;
        public float MPheightDampener;
        #endregion

        public int smoothAmount = 1;

        public Terrain terrain;
        public TerrainData terrainData;
        private TerrainManager terrainManager;

        public int Seed { get => seed; set => seed = value; }


        /*
         * Get Height Map will return the current height map if reset terrain is false,
         * otherwise it will generate a new height map.
         * The function is overloaded to either use the resetTerrain global varaible, or to be specified when calling the function
         */
        public float[,] GetHeightMap()
        {
            return GetHeightMap(resetTerrain);
        }
        public float[,] GetHeightMap(bool reset)
        {
            if (!reset)
            {
                return terrainManager.TerrainData.GetHeights(0, 0, terrainManager.HeightmapResolution, terrainManager.HeightmapResolution);
            }
            else
            {
                return new float[terrainManager.HeightmapResolution, terrainManager.HeightmapResolution];
            }
        }
        public IEnumerator FlattenAreaAroundPoint(int x, int y, float strength, Vector2 area)
        {
            //Debug.Log("Flattening Area");
            float[,] heightMap = GetHeightMap(false);

            for (int j = Mathf.Max(0, (int)(y - (area.y / 2))); j < Mathf.Min(heightMap.GetLength(1), (int)(y + (area.y / 2))); j++)
            {
                for (int i = Mathf.Max(0, (int)(x - (area.x / 2))); i < Mathf.Min(heightMap.GetLength(0), (int)(x + (area.x / 2))); i++)
                {
                    float centerHeight = heightMap[x, y];
                    heightMap[i, j] = Mathf.Lerp(heightMap[i, j], centerHeight, strength);
                    yield return null;
                }
            }
            //Debug.Log("Setting new heights");
            terrainManager.SetHeightmap(heightMap);
            //SmoothAreaAroundPoint(x, y, 1, area); //TODO
            terrainManager.GetPainter().SplatMaps();
            yield break;
        }
        public void MidpointDisplacement()
        {

            /*
             * This function performs Midpoint Displacement using the Diamond Square Algorithm.
             * 
             * At any point, this algorithm works with a square of squareSize * squareSize
             * The bottom left vertex is x, y
             * the top right is cornerX, cornerY
             * this means top left is (x, cornerY) and bottom right is (cornerX, y)
             * the center of the square is (midX, midY)
             */

            float[,] heightMap = GetHeightMap();
            int width = terrainManager.HeightmapResolution - 1;  //return to 512 to be power of 2
            int squareSize = width;                         //dimensions of working area
            float heightMin = MPminHeight;
            float heightMax = MPmaxHeight;

            float heightDampener = (float)Mathf.Pow(MPheightDampener, -1 * MProughness);

            int cornerX, cornerY;                           //top right vertex of working area
            int midX, midY;                                 //center of working area
            int pmidXR, pmidXL, pmidYU, pmidYD;             //points opposite edge midpoints for square step

            ////Set Terrain Corners to random heights
            //heightMap[0, 0] = UnityEngine.Random.Range(0.0f, 0.2f);
            //heightMap[0, terrainManager.terrainManager.HeightmapResolution - 2] = UnityEngine.Random.Range(0.0f, 0.2f);
            //heightMap[terrainManager.terrainManager.HeightmapResolution - 2, 0] = UnityEngine.Random.Range(0.0f, 0.2f);
            //heightMap[terrainManager.terrainManager.HeightmapResolution - 2, terrainManager.terrainManager.HeightmapResolution - 2] = UnityEngine.Random.Range(0.0f, 0.2f);

            while (squareSize > 0)
            {
                //Diamond Step. Set the mid point of each square to the average height of the surrouning corners
                for (int x = 0; x < width; x += squareSize)
                {
                    for (int y = 0; y < width; y += squareSize)
                    {
                        cornerX = (x + squareSize);
                        cornerY = (y + squareSize);

                        midX = (int)(x + squareSize / 2.0f);
                        midY = (int)(y + squareSize / 2.0f);

                        heightMap[midX, midY] = (float)((heightMap[x, y]
                                                       + heightMap[x, cornerY]
                                                       + heightMap[cornerX, y]
                                                       + heightMap[cornerX, cornerY]) / 4.0f)
                                                        + UnityEngine.Random.Range(heightMin, heightMax);
                    }
                }

                //Square step. Set the height of edge midpoints based on surrounding vertices
                for (int x = 0; x < width; x += squareSize)
                {
                    for (int y = 0; y < width; y += squareSize)
                    {
                        cornerX = (x + squareSize);
                        cornerY = (y + squareSize);

                        midX = (int)(x + squareSize / 2.0f);
                        midY = (int)(y + squareSize / 2.0f);

                        pmidXR = (int)(midX + squareSize);
                        pmidYU = (int)(midY + squareSize);
                        pmidXL = (int)(midX - squareSize);
                        pmidYD = (int)(midY - squareSize);


                        if (pmidXL <= 0 || pmidYD <= 0 || pmidXR >= width - 1 || pmidYU >= width - 1)
                        {
                            continue; //ignore edges where external points would cause out of bounds error
                        }

                        //bottom edge midpoint height
                        heightMap[midX, y] = (float)((heightMap[midX, midY]
                                                       + heightMap[x, y]
                                                       + heightMap[midX, pmidYD]
                                                       + heightMap[cornerX, y]) / 4.0f)
                                                        + UnityEngine.Random.Range(heightMin, heightMax);

                        //top edge midpoint height
                        heightMap[midX, cornerY] = (float)((heightMap[x, cornerY]
                                                       + heightMap[midX, midY]
                                                       + heightMap[cornerX, cornerY]
                                                       + heightMap[midX, pmidYU]) / 4.0f)
                                                        + UnityEngine.Random.Range(heightMin, heightMax);

                        //left edge midpoint height
                        heightMap[x, midY] = (float)((heightMap[midX, midY]
                                                       + heightMap[x, y]
                                                       + heightMap[pmidXL, midY]
                                                       + heightMap[x, cornerY]) / 4.0f)
                                                        + UnityEngine.Random.Range(heightMin, heightMax);

                        //right edge midpoint height
                        heightMap[cornerX, midY] = (float)((heightMap[midX, midY]
                                                       + heightMap[cornerX, cornerY]
                                                       + heightMap[pmidXR, midY]
                                                       + heightMap[cornerX, y]) / 4.0f)
                                                        + UnityEngine.Random.Range(heightMin, heightMax);
                    }
                }

                //reduce square
                squareSize = (int)(squareSize / 2.0f);
                heightMin *= heightDampener;
                heightMax *= heightDampener;
            }
            terrainManager.SetHeightmap(heightMap);

        }
        public void Voronoi()
        {
            float[,] heightMap = GetHeightMap();
            Random.InitState(seed);
            //Debug.Log(terrainManager);
            for (int p = 0; p < voronoiPeakCount; p++)
            {
                // Random Peak
                Vector3 peak = new Vector3(
                     Random.Range(0, terrainManager.HeightmapResolution),
                     Random.Range(voronoiMinHeight, voronoiMaxHeight),
                     Random.Range(0, terrainManager.HeightmapResolution));

                if (heightMap[(int)peak.x, (int)peak.z] < peak.y)
                {
                    heightMap[(int)peak.x, (int)peak.z] = peak.y;
                }
                else
                {
                    continue;
                }
                Vector2 peakLocation = new Vector2(peak.x, peak.z);
                float maxDistance = Vector2.Distance(Vector2.zero, new Vector2(terrainManager.HeightmapResolution, terrainManager.HeightmapResolution));

                for (int y = 0; y < terrainManager.HeightmapResolution; y++)
                {
                    for (int x = 0; x < terrainManager.HeightmapResolution; x++)
                    {
                        if (!(x == peakLocation.x && y == peakLocation.y))
                        {
                            float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance; //linear interpolate distance
                            float h;
                            switch (voronoiType)
                            {
                                case VoronoiType.Linear:
                                    h = peak.y - distanceToPeak * voronoiFallOff;
                                    break;
                                case VoronoiType.Power:
                                    h = peak.y - Mathf.Pow(distanceToPeak, voronoiDropOff) * voronoiFallOff;
                                    break;
                                case VoronoiType.Combined:
                                    h = peak.y - (distanceToPeak * voronoiFallOff)
                                               - Mathf.Pow(distanceToPeak, voronoiDropOff);
                                    break;
                                default:
                                    // Default is Linear, but this code should not be reached.
                                    h = peak.y - distanceToPeak * voronoiFallOff;
                                    break;
                            }
                            if (heightMap[x, y] < h)
                            {
                                heightMap[x, y] = h;
                            }
                        }
                    }
                }
            }


            terrainManager.SetHeightmap(heightMap);
        }
        public void Smooth()
        {
            // Don't use GetHeights() in case ResetTerrain is true;
            float[,] heightMap = GetHeightMap(false);

            float smoothProgress = 0;
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress);
#endif
            for (int i = 0; i < smoothAmount; i++)
            {
                for (int y = 0; y < terrainManager.HeightmapResolution; y++)
                {
                    for (int x = 0; x < terrainManager.HeightmapResolution; x++)
                    {
                        float avgHeight = heightMap[x, y];
                        List<Vector2> neighbors = Utils.GetNeighbors(new Vector2(x, y), terrainManager.HeightmapResolution, terrainManager.HeightmapResolution);
                        foreach (Vector2 n in neighbors)
                        {
                            avgHeight += heightMap[(int)n.x, (int)n.y];
                        }

                        heightMap[x, y] = avgHeight / ((float)neighbors.Count + 1);
                    }
                }
                smoothProgress++;
#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress / smoothAmount);
#endif
            }
            terrainManager.SetHeightmap(heightMap);
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif
        }
        public void Perlin()
        {
            float[,] heightMap = GetHeightMap();
            for (int y = 0; y < terrainManager.HeightmapResolution; y++)
            {
                for (int x = 0; x < terrainManager.HeightmapResolution; x++)
                {
                    heightMap[x, y] += Utils.fBM(
                            (seed + x + perlinOffsetX) * perlinScaleX,
                            (seed + y + perlinOffsetY) * perlinScaleY,
                            perlinOctaves,
                            perlinPersistance)
                        * perlinHeightScale;
                }
            }
            terrainManager.SetHeightmap(heightMap);
        }
        public void MultiplePerlinTerrain()
        {
            float[,] heightMap = GetHeightMap();
            float highestPoint = 0;
            float lowestPoint = float.MaxValue;
            for (int y = 0; y < terrainManager.HeightmapResolution; y++)
            {
                for (int x = 0; x < terrainManager.HeightmapResolution; x++)
                {
                    float totalMax = 0;

                    foreach (PerlinParameters parameters in perlinParameters)
                    {
                        heightMap[x, y] += Utils.fBM(
                            (seed + x + parameters.xOffset) * parameters.xScale,
                            (seed + y + parameters.yOffset) * parameters.yScale,
                            parameters.octaves,
                            parameters.persistance,
                            ref lowestPoint,
                            ref highestPoint) * parameters.heightScale;
                    }
                }
            }
            for (int y = 0; y < terrainManager.HeightmapResolution; y++)
            {
                for (int x = 0; x < terrainManager.HeightmapResolution; x++)
                {
                    //heightMap[x, y] = Mathf.InverseLerp(lowestPoint, highestPoint, heightMap[x, y]);
                    heightMap[x, y] = Utils.Map(heightMap[x, y], lowestPoint, highestPoint, 0, 1);
                }
            }
            terrainManager.SetHeightmap(heightMap);
        }
        public void RandomTerrain()
        {
            float[,] heightMap = GetHeightMap();

            for (int x = 0; x < terrainManager.HeightmapResolution; x++)
            {
                for (int y = 0; y < terrainManager.HeightmapResolution; y++)
                {
                    heightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
                }
            }
            terrainManager.SetHeightmap(heightMap);

        }
        public void LoadTexture()
        {
            float[,] heightMap = GetHeightMap();

            for (int x = 0; x < terrainManager.HeightmapResolution; x++)
            {
                for (int y = 0; y < terrainManager.HeightmapResolution; y++)
                {
                    heightMap[x, y] += heightMapImage.GetPixel((int)(x * heightMapScale.x),
                                                              (int)(y * heightMapScale.z)).grayscale
                                                              * heightMapScale.y;
                }
            }
            terrainManager.SetHeightmap(heightMap);
        }
        public void AddFalloffMap()
        {
            float[,] heightMap = GetHeightMap(false);
            float[,] falloffMap = FalloffGenerator.GenerateFalloffMap(terrainManager.HeightmapResolution);
            for (int y = 0; y < terrainManager.HeightmapResolution; y++)
            {
                for (int x = 0; x < terrainManager.HeightmapResolution; x++)
                {

                    //heightMap[x, y] -= falloffMap[x, y];
                    heightMap[x, y] *= 1 - falloffMap[x, y];
                }
            }
            terrainManager.SetHeightmap(heightMap);

        }
        public void ResetTerrain()
        {
            float[,] heightMap = new float[terrainManager.HeightmapResolution, terrainManager.HeightmapResolution];

            for (int x = 0; x < terrainManager.HeightmapResolution; x++)
            {
                for (int y = 0; y < terrainManager.HeightmapResolution; y++)
                {
                    heightMap[x, y] = 0;
                }
            }
            terrainManager.SetHeightmap(heightMap);
        }

        void OnEnable()
        {
            //Debug.Log("Initialising Terrain Generator");
            terrainManager = gameObject.GetOrAddComponent<TerrainManager>();
            terrainManager.SetTerrainGenerator(this);
            terrain = terrainManager.Terrain;
            terrainData = terrainManager.TerrainData;
        }
        void Awake()
        {
            SetRandomSeed();
        }

        public void SetRandomSeed()
        {
            switch (seedType)
            {
                case SeedType.Fixed:
                    seed = fixedSeed;
                    break;
                case SeedType.Random:
                    seed = DateTime.Now.Millisecond;
                    break;
                default:
                    seed = 0;
                    break;
            }
            UnityEngine.Random.InitState(seed);
        }
        /// <summary>
        /// This funciton should smooth a small area of the map with a simple mean filter.
        /// </summary>
        /// <param name="pointX">X coordinate of the centre point</param>
        /// <param name="pointY">Y coordinate of the centre point</param>
        /// <param name="smoothAmount">Number of passes of the mean filter</param>
        /// <param name="area">Size of the area to be filtered</param>
        [Obsolete("Do not use this function until pit creating bug has been fixed!")]
        private void SmoothAreaAroundPoint(int pointX, int pointY, int smoothAmount, Vector2 area)
        {
            // Don't use GetHeights() in case ResetTerrain is true;
            float[,] heightMap = GetHeightMap(false);

            float smoothProgress = 0;
#if UNITY_EDITOR
            EditorUtility.DisplayProgressBar("Smoothing Area", "Progress", smoothProgress);
#endif
            for (int i = 0; i < smoothAmount; i++)
            {
                for (int y = Mathf.Max(0, pointY - (int)(area.y / 2)); y < Mathf.Min(terrainManager.HeightmapResolution, pointY + (int)(area.y / 2)); y++)
                {
                    for (int x = Mathf.Max(0, pointX - (int)(area.x / 2)); x < Mathf.Min(terrainManager.HeightmapResolution, pointX + (int)(area.x / 2)); x++)
                    {
                        float avgHeight = heightMap[x, y];
                        List<Vector2> neighbors = Utils.GetNeighbors(new Vector2(x, y), (int)area.x, (int)area.y);
                        foreach (Vector2 n in neighbors)
                        {
                            avgHeight += heightMap[(int)n.x, (int)n.y];
                        }

                        heightMap[x, y] = avgHeight / ((float)neighbors.Count + 1);
                    }
                }
                smoothProgress++;
#if UNITY_EDITOR
                EditorUtility.DisplayProgressBar("Smoothing Area", "Progress", smoothProgress / smoothAmount);
#endif
            }
            terrainManager.SetHeightmap(heightMap);
#if UNITY_EDITOR
            EditorUtility.ClearProgressBar();
#endif        
        }
    }

}