using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public partial class CustomTerrain : MonoBehaviour
{
    public enum TagType { Tag = 0, Layer = 1 }
    [SerializeField]
    int terrainLayer = -1;

    public Vector2 randomHeightRange = new Vector2(0, 0.1f);
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(2, 0.5f, 2);

    public bool resetTerrain = true;

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
    #region SplatMaps
    public List<Splatmap> splatHeights = new List<Splatmap>()
    {
        new Splatmap()
    };

    #endregion
    #region Vegetation
    public List<Vegetation> vegetationData = new List<Vegetation>()
    {
        new Vegetation()
    };
    public int maxTrees = 5000;
    public int treeSpacing = 5;

    #endregion
    #region Details
    public List<Detail> details = new List<Detail>()
    {
        new Detail()
    };
    public int maxDetails = 5000;
    public int detailSpacing = 5;
    #endregion
    #region Water
    public float waterHeight;
    public GameObject waterGO;
    public Material shorelineMaterial;
    #endregion

    #region Erosion
    public enum ErosionType { Rain, Thermal, Tidal, River, Wind };
    public ErosionType erosionType = ErosionType.Rain;
    public float erosionStrength = 0.1f;
    public int springsPerRiver = 5;
    public float solubility = 0.01f;
    public int droplets = 10;
    public int erosionSmoothAmount = 5;

    #endregion



    public Terrain terrain;
    public TerrainData terrainData;

    private float[,] GetHeightMap()
    {
        if (!resetTerrain)
        {
            return terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        }
        else
        {
            return new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        }
    }


    [Obsolete("Use Unity's in built steepness instead")]
    float GetSteepness(float[,] heightMap, int x, int y, int width, int height)
    {
        float h = heightMap[x, y];
        //if on upper edge, find gradient by going backwards
        int nx = x + 1 > width - 1 ? x - 1 : x + 1;
        int ny = y + 1 > height - 1 ? y - 1 : y + 1;

        float dx = heightMap[nx, y] - h;
        float dy = heightMap[x, ny] - h;
        Vector2 gradient = new Vector2(dx, dy);

        return gradient.magnitude;

    }
    public void SplatMaps()
    {
        TerrainLayer[] newSplatPrototypes;
        newSplatPrototypes = new TerrainLayer[splatHeights.Count];
        int spIndex = 0;
        foreach (Splatmap splatHeight in splatHeights)
        {
            newSplatPrototypes[spIndex] = new TerrainLayer
            {
                diffuseTexture = splatHeight.texture,
                tileOffset = splatHeight.tileOffset,
                tileSize = splatHeight.tileSize
            };
            newSplatPrototypes[spIndex].diffuseTexture.Apply(true);
            string path = "Assets/New Terrain Layer " + spIndex + ".terrainlayer";
            AssetDatabase.CreateAsset(newSplatPrototypes[spIndex], path);
            spIndex++;
            Selection.activeObject = this.gameObject;
        }
        terrainData.terrainLayers = newSplatPrototypes;

        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution,
                                                          terrainData.heightmapResolution);
        float[,,] splatmapData = new float[terrainData.alphamapResolution,
                                           terrainData.alphamapResolution,
                                           terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapResolution; y++)
        {
            for (int x = 0; x < terrainData.alphamapResolution; x++)
            {
                float[] splat = new float[terrainData.alphamapLayers];
                for (int i = 0; i < splatHeights.Count; i++)
                {
                    float noise = Mathf.PerlinNoise(x * splatHeights[i].blendNoiseScale.x,
                                                    y * splatHeights[i].blendNoiseScale.y)
                                                    * splatHeights[i].blendNoiseScalar;
                    float offset = splatHeights[i].blendOffset + noise;
                    float thisHeightStart = splatHeights[i].minHeight - offset;
                    float thisHeightStop = splatHeights[i].maxHeight + offset;
                    //float thisSteepness = GetSteepness(heightMap, x, y,
                    //    terrainData.heightmapResolution, terrainData.heightmapResolution);

                    //alpha and height maps are at 90deg to each other, so swap x and y here
                    float thisSteepness = terrainData.GetSteepness(y / (float)terrainData.alphamapResolution,
                                                                   x / (float)terrainData.alphamapResolution);
                    bool isInHeightBand = heightMap[x, y] >= thisHeightStart
                                            && heightMap[x, y] <= thisHeightStop;
                    bool isInSteepnessBand = thisSteepness >= splatHeights[i].minSlope
                                            && thisSteepness <= splatHeights[i].maxSlope;
                    if (isInHeightBand && isInSteepnessBand)
                    {
                        splat[i] = 1;
                    }
                }
                Utils.NormalizeVector(ref splat);
                for (int z = 0; z < splatHeights.Count; z++)
                {
                    splatmapData[x, y, z] = splat[z];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);

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
        int width = terrainData.heightmapResolution - 1;  //return to 512 to be power of 2
        int squareSize = width;                         //dimensions of working area
        float heightMin = MPminHeight;
        float heightMax = MPmaxHeight;

        float heightDampener = (float)Mathf.Pow(MPheightDampener, -1 * MProughness);

        int cornerX, cornerY;                           //top right vertex of working area
        int midX, midY;                                 //center of working area
        int pmidXR, pmidXL, pmidYU, pmidYD;             //points opposite edge midpoints for square step

        ////Set Terrain Corners to random heights
        //heightMap[0, 0] = UnityEngine.Random.Range(0.0f, 0.2f);
        //heightMap[0, terrainData.heightmapResolution - 2] = UnityEngine.Random.Range(0.0f, 0.2f);
        //heightMap[terrainData.heightmapResolution - 2, 0] = UnityEngine.Random.Range(0.0f, 0.2f);
        //heightMap[terrainData.heightmapResolution - 2, terrainData.heightmapResolution - 2] = UnityEngine.Random.Range(0.0f, 0.2f);

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
        terrainData.SetHeights(0, 0, heightMap);

    }

    public void PaintDetails()
    {
        DetailPrototype[] detailPrototypes;
        detailPrototypes = new DetailPrototype[details.Count];
        int detailIndex = 0;
        foreach (Detail detail in details)
        {
            detailPrototypes[detailIndex] = new DetailPrototype
            {
                prototype = detail.prototype,
                prototypeTexture = detail.prototypeTexture,
                healthyColor = detail.healthyColor,
                dryColor = detail.dryColor,
                minHeight = detail.heightRange.x,
                maxHeight = detail.heightRange.y,
                minWidth = detail.widthRange.x,
                maxWidth = detail.widthRange.y,
                noiseSpread = detail.noiseSpread
            };
            if (detailPrototypes[detailIndex].prototype)
            {
                detailPrototypes[detailIndex].usePrototypeMesh = true;
                detailPrototypes[detailIndex].renderMode = DetailRenderMode.VertexLit;
            }
            else
            {
                detailPrototypes[detailIndex].usePrototypeMesh = false;
                detailPrototypes[detailIndex].renderMode = DetailRenderMode.GrassBillboard;
            }
            detailIndex++;
        }
        terrainData.detailPrototypes = detailPrototypes;
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
        {
            int[,] detailMap = new int[terrainData.detailResolution, terrainData.detailResolution]; //perhaps use width & height instead of resolution
            for (int y = 0; y < terrainData.detailResolution; y += detailSpacing)
            {
                for (int x = 0; x < terrainData.detailResolution; x += detailSpacing)
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > details[i].density)
                    {
                        continue;
                    }
                    int hmX = (int)Utils.Map(x, 0, terrainData.detailResolution, 0, (float)terrainData.heightmapResolution);
                    int hmY = (int)Utils.Map(y, 0, terrainData.detailResolution, 0, (float)terrainData.heightmapResolution);

                    float thisNoise = Utils.Map(Mathf.PerlinNoise(
                        x * details[i].feather,
                        y * details[i].feather),
                        0, 1, 0.5f, 1);
                    float heightStart = details[i].minHeight * thisNoise - details[i].overlap * thisNoise;
                    float heightEnd = details[i].maxHeight * thisNoise - details[i].overlap * thisNoise;
                    float thisHeight = heightMap[hmY, hmX]; //XY is flipped for heightmap and detail map
                    float steepness = terrainData.GetSteepness(
                        hmX / (float)terrainData.size.x,
                        hmY / (float)terrainData.size.z);

                    bool inHeightBand = thisHeight >= heightStart && thisHeight <= heightEnd;
                    bool inSlopeBand = steepness >= details[i].minSlope && steepness <= details[i].maxSlope;

                    if (inHeightBand && inSlopeBand)
                    {
                        detailMap[y, x] = 1; // XY is flipped because detail map is rotated 90deg from heightMap 
                    }
                }
            }
            terrainData.SetDetailLayer(0, 0, i, detailMap);
        }
    }

    public void Erode()
    {
        switch (erosionType)
        {
            case ErosionType.Rain:
                Rain();
                break;
            case ErosionType.Thermal:
                Tidal();
                break;
            case ErosionType.Tidal:
                Thermal();
                break;
            case ErosionType.River:
                River();
                break;
            case ErosionType.Wind:
                Wind();
                break;
            default:
                break;
        }
        for (int i = 0; i < erosionSmoothAmount; i++)
        {
            Smooth();
        }
    }

    private void Wind()
    {
        throw new NotImplementedException();
    }

    private void River()
    {
        throw new NotImplementedException();
    }

    private void Thermal()
    {
        throw new NotImplementedException();
    }

    private void Tidal()
    {
        throw new NotImplementedException();
    }

    private void Rain()
    {
        throw new NotImplementedException();
    }

    public void AddShore()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        int quadCount = 0;

        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                Vector2 location = new Vector2(x, y);
                List<Vector2> neighbors = GetNeighbors(location, terrainData.heightmapResolution, terrainData.heightmapResolution);
                foreach (Vector2 n in neighbors)
                {
                    if (heightMap[x, y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)
                    {

                        quadCount++;
                        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        go.transform.localScale *= 10.0f;

                        go.transform.position = this.transform.position
                            + new Vector3(
                                    y / (float)terrainData.heightmapResolution * terrainData.size.z,
                                    waterHeight * terrainData.size.y,
                                    x / (float)terrainData.heightmapResolution * terrainData.size.x);

                        go.transform.LookAt(new Vector3(
                                n.y / (float)terrainData.heightmapResolution * terrainData.size.z,
                                waterHeight * terrainData.size.y,
                                n.x / (float)terrainData.heightmapResolution * terrainData.size.x));

                        go.transform.Rotate(90, 0, 0);

                        go.tag = "Shore";
                    }
                }
            }
        }
        GameObject[] shoreQuads = GameObject.FindGameObjectsWithTag("Shore");
        MeshFilter[] meshFilters = new MeshFilter[shoreQuads.Length];
        for (int m = 0; m < shoreQuads.Length; m++)
        {
            meshFilters[m] = shoreQuads[m].GetComponent<MeshFilter>();
        }
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }

        GameObject currentShoreLine = GameObject.Find("ShoreLine");
        if (currentShoreLine)
        {
            DestroyImmediate(currentShoreLine);
        }
        GameObject shoreLine = new GameObject();
        shoreLine.name = "ShoreLine";
        shoreLine.AddComponent<WaveAnimation>(); //Old code from Unity Islands Demo, back in Unity 3 or 4
        shoreLine.transform.position = this.transform.position;
        shoreLine.transform.rotation = this.transform.rotation;
        MeshFilter thisMF = shoreLine.AddComponent<MeshFilter>();
        thisMF.mesh = new Mesh();
        shoreLine.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
        MeshRenderer r = shoreLine.AddComponent<MeshRenderer>();
        r.sharedMaterial = shorelineMaterial;
        for (int sQ = 0; sQ < shoreQuads.Length; sQ++)
        {
            DestroyImmediate(shoreQuads[sQ]);
        }
    }

    public void AddWater()
    {
        GameObject water = GameObject.Find("water");
        if (!water)
        {
            water = Instantiate(waterGO, transform.position, transform.rotation);
            water.name = "water";
        }
        water.transform.position = transform.position + new Vector3(
                terrainData.size.x / 2,
                waterHeight * terrainData.size.y,
                terrainData.size.z / 2
            );
        water.transform.localScale = new Vector3(terrainData.size.x, 1, terrainData.size.z);
    }

    public void PlantVegetation()
    {
        TreePrototype[] treePrototypes;
        treePrototypes = new TreePrototype[vegetationData.Count];
        int treeIdx = 0;
        foreach (Vegetation t in vegetationData)
        {
            treePrototypes[treeIdx] = new TreePrototype
            {
                prefab = t.mesh
            };
            treeIdx++;
        }
        terrainData.treePrototypes = treePrototypes;

        List<TreeInstance> allVegetation = new List<TreeInstance>();
        //NEED TO GET VALUES FROM TERRAIN MESH, NOT HEIGHTMAP

        for (int z = 0; z < terrainData.size.z; z += treeSpacing)
        {
            for (int x = 0; x < terrainData.size.x; x += treeSpacing)
            {
                for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > vegetationData[tp].density)
                    {
                        break;
                    }

                    int hmX = (int)Utils.Map(x, 0, terrainData.size.x, 0, (float)terrainData.heightmapResolution);
                    int hmZ = (int)Utils.Map(z, 0, terrainData.size.z, 0, (float)terrainData.heightmapResolution);
                    float thisHeight = terrainData.GetHeight(hmX, hmZ) / terrainData.size.y;
                    float steepness = terrainData.GetSteepness(
                        x / (float)terrainData.size.x,
                        z / (float)terrainData.size.z);
                    if ((thisHeight >= vegetationData[tp].minHeight && thisHeight <= vegetationData[tp].maxHeight)
                        && (steepness >= vegetationData[tp].minSlope && steepness <= vegetationData[tp].maxSlope))
                    {
                        Vector3 position;

                        ////Default: perfect grid
                        //position = new Vector3(x / terrainData.size.x,
                        //        thisHeight,
                        //        z / terrainData.size.z),

                        //Slight random offset
                        position = new Vector3((x + UnityEngine.Random.Range(-10.0f, 10.0f)) / terrainData.size.x,
                                                thisHeight,
                                                (z + UnityEngine.Random.Range(-10.0f, 10.0f)) / terrainData.size.z);

                        ////Todo: Poisson Disk
                        //position = new Vector3(x / terrainData.size.x,
                        //        thisHeight,
                        //        z / terrainData.size.z),

                        Vector3 treeWorldPos = new Vector3(
                            position.x * terrainData.size.x,
                            position.y * terrainData.size.y,
                            position.z * terrainData.size.z)
                            + this.transform.position;
                        RaycastHit hit;
                        int layerMask = 1 << terrainLayer;
                        if (Physics.Raycast(treeWorldPos + Vector3.up * 10, -Vector3.up, out hit, 100, layerMask)
                            || Physics.Raycast(treeWorldPos - Vector3.up * 10, Vector3.up, out hit, 100, layerMask))
                        {
                            TreeInstance instance = new TreeInstance
                            {

                                position = position,
                                rotation = UnityEngine.Random.Range(0, 360),
                                prototypeIndex = tp,
                                color = Color.Lerp(
                                    vegetationData[tp].color1,
                                    vegetationData[tp].color2,
                                    UnityEngine.Random.Range(0.0f, 1.0f)),
                                lightmapColor = vegetationData[tp].lightColor,
                                heightScale = UnityEngine.Random.Range(vegetationData[tp].minScale, vegetationData[tp].maxScale),
                                widthScale = UnityEngine.Random.Range(vegetationData[tp].minScale, vegetationData[tp].maxScale)
                            };

                            float treeHeight = (hit.point.y - this.transform.position.y) / terrainData.size.y;
                            instance.position = new Vector3(
                                instance.position.x,
                                treeHeight - vegetationData[tp].pivotOffset,
                                instance.position.z);
                            allVegetation.Add(instance);

                        }
                        if (allVegetation.Count >= maxTrees) goto TREESDONE;
                    }

                }
            }
        }
    TREESDONE:
        terrainData.treeInstances = allVegetation.ToArray();

    }

    public void Voronoi()
    {
        float[,] heightMap = GetHeightMap();
        for (int p = 0; p < voronoiPeakCount; p++)
        {
            // Random Peak
            Vector3 peak = new Vector3(
                 UnityEngine.Random.Range(0, terrainData.heightmapResolution),
                 UnityEngine.Random.Range(voronoiMinHeight, voronoiMaxHeight),
                 UnityEngine.Random.Range(0, terrainData.heightmapResolution));

            if (heightMap[(int)peak.x, (int)peak.z] < peak.y)
            {
                heightMap[(int)peak.x, (int)peak.z] = peak.y;
            }
            else
            {
                continue;
            }
            Vector2 peakLocation = new Vector2(peak.x, peak.z);
            float maxDistance = Vector2.Distance(Vector2.zero, new Vector2(terrainData.heightmapResolution, terrainData.heightmapResolution));

            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
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


        terrainData.SetHeights(0, 0, heightMap);
    }

    public List<Vector2> GetNeighbors(Vector2 pos, int width, int height)
    {
        List<Vector2> neighbors = new List<Vector2>();
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (!(x == 0 && y == 0)) //don't include current position, kernel is only focused on neighbors
                {
                    //Find neighbors, clamped within boundaries of image
                    Vector2 neighborPos = new Vector2(Mathf.Clamp(pos.x + x, 0, width - 1),
                                                        Mathf.Clamp(pos.y + y, 0, height - 1));

                    // If this is not already in the kernel, then add it
                    if (!neighbors.Contains(neighborPos))
                    {
                        neighbors.Add(neighborPos);
                    }
                }
            }
        }
        return neighbors;
    }

    public void Smooth()
    {
        // Don't use GetHeights() in case ResetTerrain is true;
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

        float smoothProgress = 0;
#if UNITY_EDITOR
        EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress);
#endif
        for (int i = 0; i < smoothAmount; i++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    float avgHeight = heightMap[x, y];
                    List<Vector2> neighbors = GetNeighbors(new Vector2(x, y), terrainData.heightmapResolution, terrainData.heightmapResolution);
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
        terrainData.SetHeights(0, 0, heightMap);
#if UNITY_EDITOR
        EditorUtility.ClearProgressBar();
#endif
    }

    public void Perlin()
    {
        float[,] heightMap = GetHeightMap();
        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                heightMap[x, y] += Utils.fBM(
                        (x + perlinOffsetX) * perlinScaleX,
                        (y + perlinOffsetY) * perlinScaleY,
                        perlinOctaves,
                        perlinPersistance)
                    * perlinHeightScale;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }
    public void MultiplePerlinTerrain()
    {
        float[,] heightMap = GetHeightMap();
        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                foreach (PerlinParameters parameters in perlinParameters)
                {
                    heightMap[x, y] += Utils.fBM(
                        (x + parameters.xOffset) * parameters.xScale,
                        (y + parameters.yOffset) * parameters.yScale,
                        parameters.octaves,
                        parameters.persistance) * parameters.heightScale;
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void RandomTerrain()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                heightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        terrainData.SetHeights(0, 0, heightMap);

    }
    public void LoadTexture()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                heightMap[x, y] += heightMapImage.GetPixel((int)(x * heightMapScale.x),
                                                          (int)(y * heightMapScale.z)).grayscale
                                                          * heightMapScale.y;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }
    public void ResetTerrain()
    {
        float[,] heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                heightMap[x, y] = 0;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

#if UNITY_EDITOR
    private int AddTag(SerializedProperty tagsProp, string newTag, TagType tagType)
    {
        bool found = false;

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            // stop if tag already exists
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag))
            {
                found = true;
                return i;
            }
        }
        if (!found)
        {
            switch (tagType)
            {
                case TagType.Tag:
                    // create a new item in the tags array and give it the newTag value
                    tagsProp.InsertArrayElementAtIndex(0);
                    SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
                    newTagProp.stringValue = newTag;
                    return -1; //not needed
                case TagType.Layer:
                    // Create a new layer
                    for (int j = 8; j < tagsProp.arraySize; j++)
                    {
                        //user layers start at 8
                        SerializedProperty newLayer = tagsProp.GetArrayElementAtIndex(j);
                        if (newLayer.stringValue == "")
                        {
                            newLayer.stringValue = newTag;
                            return j;
                        }
                    }
                    return -1; //shouldnt be called
                default:
                    return -1; // shouldn't be called
            }

        }
        return -1;
    }
#endif

    void OnEnable()
    {
        Debug.Log("Initialising Terrain Data");
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }
    void Awake()
    {
#if UNITY_EDITOR
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        AddTag(tagsProp, "Terrain", TagType.Tag);
        AddTag(tagsProp, "Cloud", TagType.Tag);
        AddTag(tagsProp, "Shore", TagType.Tag);

        // update tags DB
        tagManager.ApplyModifiedProperties();

        SerializedProperty layerProp = tagManager.FindProperty("layers");
        terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);
        tagManager.ApplyModifiedProperties();

        // tag this object
        this.gameObject.tag = "Terrain";
        this.gameObject.layer = terrainLayer;
#endif
    }

    public void AddNewData<T>(ref List<T> list) where T : new()
    {
        T data = default; // should be equivalent to new T()
        list.Add(data);
    }
    public void RemoveData<T>(ref List<T> list) where T : TableItem
    {
        List<T> keptData = new List<T>();
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].remove)
            {
                keptData.Add(list[i]);
            }
        }
        if (keptData.Count == 0)
        {
            keptData.Add(list[0]);
        }
        list = keptData;
    }

    [Obsolete("Please use 'AddNewData<T>(ref List<T> list)' instead")]
    public void AddNewSplatHeight()
    {
        splatHeights.Add(new Splatmap());
    }

    [Obsolete("Please use 'RemoveData<T>(ref List<T> list)' instead")]
    public void RemoveSplatHeight()
    {
        List<Splatmap> keptSplatHeights = new List<Splatmap>();
        for (int i = 0; i < splatHeights.Count; i++)
        {
            if (!splatHeights[i].remove)
            {
                keptSplatHeights.Add(splatHeights[i]);
            }
        }
        if (keptSplatHeights.Count == 0)
        {
            keptSplatHeights.Add(splatHeights[0]);
        }
        splatHeights = keptSplatHeights;
    }

    [Obsolete("Please use 'AddNewData<T>(ref List<T> list)' instead")]
    public void AddNewPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }

    [Obsolete("Please use 'RemoveData<T>(ref List<T> list)' instead")]
    public void RemovePerlin()
    {
        // New list for parameters that should be kept
        List<PerlinParameters> keptPerlinParameters = new List<PerlinParameters>();

        // Loop through current parameters, and check if they should be removed or not
        for (int i = 0; i < perlinParameters.Count; i++)
        {
            if (!perlinParameters[i].remove)
            {
                //if they should not be removed, add them to the kepy list
                keptPerlinParameters.Add(perlinParameters[i]);
            }
        }
        if (keptPerlinParameters.Count == 0)
        {
            //if none are to be kept, then add at least 1 item to the kept list so the table will work
            keptPerlinParameters.Add(perlinParameters[0]);
        }
        // set list to kept list
        perlinParameters = keptPerlinParameters;
    }

    [Obsolete("Please use 'AddNewData<T>(ref List<T> list)' instead")]
    public void AddNewVegetation()
    {
        vegetationData.Add(new Vegetation());
    }

    [Obsolete("Please use 'RemoveData<T>(ref List<T> list)' instead")]
    public void RemoveVegetation()
    {
        List<Vegetation> keptData = new List<Vegetation>();
        for (int i = 0; i < vegetationData.Count; i++)
        {
            if (!vegetationData[i].remove)
            {
                keptData.Add(vegetationData[i]);
            }


        }
        if (keptData.Count == 0)
        {
            keptData.Add(vegetationData[0]);
        }
        vegetationData = keptData;
    }

    [Obsolete("Please use 'RemoveData<T>(ref List<T> list)' instead")]
    public void RemoveDetail()
    {
        List<Detail> keptData = new List<Detail>();
        for (int i = 0; i < details.Count; i++)
        {
            if (!details[i].remove)
            {
                keptData.Add(details[i]);
            }


        }
        if (keptData.Count == 0)
        {
            keptData.Add(details[0]);
        }
        details = keptData;
    }

    [Obsolete("Please use 'AddNewData<T>(ref List<T> list)' instead")]
    public void AddNewDetail()
    {
        details.Add(new Detail());
    }
}
