using MED10.Architecture.Events;
using MED10.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace MED10.PCG
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(TerrainManager))]
    public class TerrainPainter : MonoBehaviour
    {

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

        #region Clouds
        public int numberOfClouds = 1;
        public int particlesPerCloud = 50;
        public float cloudParticleSize = 5;
        public Vector3 cloudMinSize = Vector3.one;
        public Vector3 cloudMaxSize = Vector3.one;
        public Material cloudMaterial;
        public Material cloudShadowMaterial;
        public Color color = Color.white;
        public Color lining = Color.grey;
        public float minSpeed = 0.2f;
        public float maxSpeed = 0.5f;
        public float distanceTravelled = 500f;

        #endregion
        public GameEvent paintingDone;
        public bool detailsDone, treesDone, splatmapsDone;


        private void Start()
        {
            TerrainManager.Instance.SetPainter(this);
        }

        private void RaiseEventIfComplete()
        {
            if (detailsDone && treesDone && splatmapsDone)
            {
                paintingDone.Raise();
                detailsDone = false;
                treesDone = false;
                splatmapsDone = false;
            }
        }
        public void SplatMaps()
        {
#if UNITY_EDITOR
            Profiler.BeginSample("EDITOR: Assign Splat Maps");
            TerrainLayer[] newSplatPrototypes;
            newSplatPrototypes = new TerrainLayer[splatHeights.Count];
            int spIndex = 0;
            foreach (Splatmap splatHeight in splatHeights)
            {
                newSplatPrototypes[spIndex] = new TerrainLayer
                {
                    diffuseTexture = splatHeight.texture,
                    normalMapTexture = splatHeight.normalMap,
                    tileOffset = splatHeight.tileOffset,
                    tileSize = splatHeight.tileSize
                };
                newSplatPrototypes[spIndex].diffuseTexture.Apply(true);
                newSplatPrototypes[spIndex].normalMapTexture.Apply(true);
                string path = "Assets/TerrainLayer_" + splatHeight.texture.name + "_" + spIndex + ".terrainlayer";
                AssetDatabase.CreateAsset(newSplatPrototypes[spIndex], path);
                spIndex++;
                Selection.activeObject = this.gameObject;
            }
            TerrainManager.Instance.TerrainData.terrainLayers = newSplatPrototypes;
            Profiler.EndSample();
#endif
            Profiler.BeginSample("Apply Splatmaps");
            StartCoroutine(ApplySplatmaps());
            Profiler.EndSample();
        }

        private IEnumerator ApplySplatmaps()
        {
            splatmapsDone = false;
            TerrainData terrainData = TerrainManager.Instance.TerrainData;
            int resolution = TerrainManager.Instance.HeightmapResolution;
            float[,] heightMap = terrainData.GetHeights(0, 0, resolution,
                                                              resolution);
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
                yield return null;
            }
            //Debug.Log("Reapplying Ground Textures");
            terrainData.SetAlphamaps(0, 0, splatmapData);
            splatmapsDone = true;
            RaiseEventIfComplete();
            yield break;
        }

        public void PlantVegetation()
        {
            treesDone = false;
            //Take the list of tree prefabs from VegetationData object, and convert it into an array of type TreePrototype.
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
            //Assign the tree prototypes to TerrainData
            TerrainData terrainData = TerrainManager.Instance.TerrainData;
            terrainData.treePrototypes = treePrototypes;

            //Create a list of TreeInstance which will contain every tree which is instantiated on the terrain
            List<TreeInstance> allVegetation = new List<TreeInstance>();

            //The position data for trees needs to come from the Terrain, and NOT from the heightmap.
            //Iterate over the terrain (x, z) in steps determined by the tree spacing (global variable)
            for (int z = 0; z < terrainData.size.z; z += treeSpacing)
            {
                for (int x = 0; x < terrainData.size.x; x += treeSpacing)
                {

                    // Run the following code separately for each prototype tree
                    for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)
                    {
                        // eliminate trees based on density using simple RMG
                        if (UnityEngine.Random.Range(0.0f, 1.0f) > vegetationData[tp].density)
                        {
                            break;
                        }

                        //Find the heightmap position and check if this area is suitable for the prototype based on it's height and slope bands
                        int resolution = TerrainManager.Instance.HeightmapResolution;
                        int hmX = (int)Utils.Map(x, 0, terrainData.size.x, 0, (float)resolution);
                        int hmZ = (int)Utils.Map(z, 0, terrainData.size.z, 0, (float)resolution);
                        float thisHeight = terrainData.GetHeight(hmX, hmZ) / terrainData.size.y;
                        float steepness = terrainData.GetSteepness(
                            x / (float)terrainData.size.x,
                            z / (float)terrainData.size.z);
                        if ((thisHeight >= vegetationData[tp].minHeight && thisHeight <= vegetationData[tp].maxHeight)
                            && (steepness >= vegetationData[tp].minSlope && steepness <= vegetationData[tp].maxSlope))
                        {
                            //If the tree can be placed, then set its position
                            Vector3 position;

                            ////Default: perfect grid
                            //position = new Vector3(x / TerrainManager.Instance.TerrainData.size.x,
                            //        thisHeight,
                            //        z / TerrainManager.Instance.TerrainData.size.z),

                            //Slight random offset
                            position = new Vector3((x + UnityEngine.Random.Range(-10.0f, 10.0f)) / terrainData.size.x,
                                                    thisHeight,
                                                    (z + UnityEngine.Random.Range(-10.0f, 10.0f)) / terrainData.size.z);

                            ////Todo: Poisson Disk
                            //position = new Vector3(x / TerrainManager.Instance.TerrainData.size.x,
                            //        thisHeight,
                            //        z / TerrainManager.Instance.TerrainData.size.z),

                            Vector3 treeWorldPos = new Vector3(
                                position.x * terrainData.size.x,
                                position.y * terrainData.size.y,
                                position.z * terrainData.size.z)
                                + this.transform.position;

                            //Perform a raycast on the terrain layer to ensure trees will be grounded properly
                            RaycastHit hit;
                            int layerMask = 1 << TerrainManager.Instance.TerrainLayer;
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
                            //If we have exceeded maxTrees, then stop creating more
                            if (allVegetation.Count >= maxTrees) goto TREESDONE;
                        }

                    }
                }
            }
        TREESDONE:
            terrainData.treeInstances = allVegetation.ToArray();
            treesDone = true;
            RaiseEventIfComplete();
        }

        public void PaintDetails()
        {
            detailsDone = false;
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
                    noiseSpread = detail.noiseSpread,
                    bendFactor = detail.bendFactor

                };
                if (detailPrototypes[detailIndex].prototype)
                {
                    detailPrototypes[detailIndex].usePrototypeMesh = true;
                    detailPrototypes[detailIndex].bendFactor = 0; //Don't want meshes to move with the wind
                    if (detail.renderMode == DetailRenderMode.GrassBillboard)
                    {
                        detail.renderMode = DetailRenderMode.Grass; //Meshes can't use billboard
                    }
                }
                else
                {
                    detailPrototypes[detailIndex].usePrototypeMesh = false;
                    //detailPrototypes[detailIndex].renderMode = DetailRenderMode.GrassBillboard;
                }
                detailPrototypes[detailIndex].renderMode = detail.renderMode;

                detailIndex++;
            }
            TerrainData terrainData = TerrainManager.Instance.TerrainData;
            terrainData.detailPrototypes = detailPrototypes;

            float[,] heightMap = gameObject.GetOrAddComponent<TerrainManager>().GetHeightmap(false);
            int resolution = TerrainManager.Instance.HeightmapResolution;
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
                        int hmX = (int)Utils.Map(x, 0, terrainData.detailResolution, 0, (float)resolution);
                        int hmY = (int)Utils.Map(y, 0, terrainData.detailResolution, 0, (float)resolution);

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
            detailsDone = true;
            RaiseEventIfComplete();
        }

        public void GenerateClouds()
        {
            // Create a Cloud Manager Game Object in the scene
            GameObject cloudManager = GameObject.Find("CloudManager");
            if (!cloudManager)
            {
                cloudManager = new GameObject
                {
                    name = "CloudManager"
                };
                cloudManager.transform.position = transform.position;
                cloudManager.AddComponent<CloudManager>();
            }

            // Remove clouds if they already exists
            GameObject[] allClouds = GameObject.FindGameObjectsWithTag("Cloud");
            for (int i = 0; i < allClouds.Length; i++)
            {
                DestroyImmediate(allClouds[i]);
            }

            // Create new Clouds
            for (int c = 0; c < numberOfClouds; c++)
            {
                // Create cloud GameObject with Name and Tag
                GameObject cloudGO = new GameObject
                {
                    name = "Cloud" + c,
                    tag = "Cloud"
                };
                //Set transform rotation and position to that of cloud manager
                cloudGO.transform.rotation = cloudManager.transform.rotation;
                cloudGO.transform.position = cloudManager.transform.position;

                // Add CloudController script
                CloudController cc = cloudGO.AddComponent<CloudController>();
                cc.lining = lining;
                cc.color = color;
                cc.numberOfParticles = particlesPerCloud;
                cc.minSpeed = minSpeed;
                cc.maxSpeed = maxSpeed;
                cc.distance = distanceTravelled;

                //Create cloud particle system
                ParticleSystem cloudSystem = cloudGO.AddComponent<ParticleSystem>();

                //Get renderer from cloud GO, or add the component if it doesnt already have it;
                Renderer cloudRenderer = cloudGO.GetOrAddComponent<Renderer>();
                cloudRenderer.material = cloudMaterial;
                cloudRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;            // Turn off recieving and casting of shadows for ParticleSystem. we will use a projector instead
                cloudRenderer.receiveShadows = false;

                // Add clouds to the sky layer so they will not recieve shadows from projector
                cloudGO.layer = LayerMask.NameToLayer("Sky");


                //if (UnityEngine.Random.Range(0, 1) > 0.5)   //Only add shadows to 50% of clouds so that it doesnt become too dark
                {
                    // Build Shadow Projector, and set Sky and Water as layers which it will ignore
                    GameObject cloudProjector = new GameObject();
                    cloudProjector.name = "Shadow";
                    cloudProjector.transform.position = cloudGO.transform.position;
                    cloudProjector.transform.forward = Vector3.down;
                    cloudProjector.transform.parent = cloudGO.transform;

                    Projector cp = cloudProjector.AddComponent<Projector>();
                    cp.material = cloudShadowMaterial;
                    cp.farClipPlane = TerrainManager.Instance.TerrainData.size.y;
                    int skyLayerMask = 1 << LayerMask.NameToLayer("Sky");
                    int waterLayerMask = 1 << LayerMask.NameToLayer("Water");
                    cp.ignoreLayers = skyLayerMask | waterLayerMask; //Bitwise OR i.e. 1000 or 0101 == 1101

                    //Set projector parameters
                    cp.fieldOfView = 20.0f;
                }

                // Set Particle system values
                ParticleSystem.MainModule main = cloudSystem.main;  //Get the MainModule of the cloudSystem
                main.loop = false; //Create the cloud and stop, we will control the rest from CloudManager

                //main.startLifetime = Mathf.Infinity; //The cloud should remain the entire time
                main.startLifetime = float.MaxValue; //The cloud should remain the entire time
                main.startSpeed = 0; //We will control the movement of the cloud
                main.startSize = cloudParticleSize;
                main.startColor = color;

                var emission = cloudSystem.emission;
                emission.rateOverTime = 0;                                      //Create whoe cloud at once
                emission.SetBursts(new ParticleSystem.Burst[]                   //A single burst at time 0.0f that will create particlesPerCloud billboards
                {
                new ParticleSystem.Burst(0.0f, (short)particlesPerCloud)
                });
                var shape = cloudSystem.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.scale = Utils.RandomVector(cloudMinSize, cloudMaxSize);

                //Set CloudManager as parent, and fix cloud scale
                cloudGO.transform.parent = cloudManager.transform;
                cloudGO.transform.localScale = Vector3.one;
            }
        }

        public void AddShore()
        {
            TerrainData terrainData = TerrainManager.Instance.TerrainData;
            float[,] heightMap = gameObject.GetOrAddComponent<TerrainManager>().GetHeightmap(false);
            int quadCount = 0;

            int resolution = TerrainManager.Instance.HeightmapResolution;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector2 location = new Vector2(x, y);
                    List<Vector2> neighbors = Utils.GetNeighbors(location, resolution, resolution);
                    foreach (Vector2 n in neighbors)
                    {
                        //Find positions on the height map below the waterline, with a neighbor above the waterline (i.e. coast)
                        if (heightMap[x, y] < waterHeight && heightMap[(int)n.x, (int)n.y] > waterHeight)
                        {
                            // Create a quad
                            quadCount++;
                            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                            go.transform.localScale *= 10.0f;

                            // Position the quad at water height
                            go.transform.position = transform.position
                                + new Vector3(
                                        y / (float)resolution * terrainData.size.z,
                                        waterHeight * terrainData.size.y,
                                        x / (float)resolution * terrainData.size.x);

                            // Rotate quad to face shore
                            go.transform.LookAt(new Vector3(
                                    n.y / resolution * terrainData.size.z,
                                    waterHeight * terrainData.size.y,
                                    n.x / resolution * terrainData.size.x));

                            // Rotate Quad to lie flat
                            go.transform.Rotate(90, 0, 0);

                            go.tag = "Shore";
                        }
                    }
                }
            }
            //Get all shore quads and get all of their mesh filters
            GameObject[] shoreQuads = GameObject.FindGameObjectsWithTag("Shore");
            MeshFilter[] meshFilters = new MeshFilter[shoreQuads.Length];
            for (int m = 0; m < shoreQuads.Length; m++)
            {
                meshFilters[m] = shoreQuads[m].GetComponent<MeshFilter>();
            }

            //Create a CombineInstance of the meshfilters
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                meshFilters[i].gameObject.SetActive(false);
                i++;
            }

            // If the shoreline already exists, remove it, then create a new one
            GameObject currentShoreLine = GameObject.Find("ShoreLine");
            if (currentShoreLine)
            {
                DestroyImmediate(currentShoreLine);
            }
            GameObject shoreLine = new GameObject
            {
                name = "ShoreLine",
                layer = LayerMask.NameToLayer("Water")
            };
            //Add wave animation to the shore, and set transfrom
            shoreLine.AddComponent<WaveAnimation>(); //Old code from Unity Islands Demo, back in Unity 3 or 4
            shoreLine.transform.position = transform.position;
            shoreLine.transform.rotation = transform.rotation;
            //Add meshfilter and empty mesh
            MeshFilter thisMF = shoreLine.AddComponent<MeshFilter>();
            thisMF.mesh = new Mesh();
            //combine the shore quads mesh filters to one, and delete the shore quads
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
            //Debug.Log("Setting water plane to height " + waterHeight * TerrainManager.Instance.TerrainData.size.y);
            // If the water plane already exists, remove it and create a new one.
            GameObject water = GameObject.Find("water");
            if (!water)
            {
                water = Instantiate(waterGO, transform.position, transform.rotation);
                water.name = "water";
            }
            // Center the plane and set the height
            TerrainData terrainData = TerrainManager.Instance.TerrainData;
            water.transform.position = transform.position + new Vector3(
                    terrainData.size.x / 2,
                    waterHeight * terrainData.size.y,
                    terrainData.size.z / 2
                );
            // Scale the plane on x and z to fit the terrain
            water.transform.localScale = new Vector3(terrainData.size.x, 1, terrainData.size.z);
        }

        /// <summary>
        /// This funtion will search an area of the terrain and remove any trees.
        /// </summary>
        /// <param name="centre">Centre of the area in Terrain Coordinates</param>
        /// <param name="area">Size of the area to be searched</param>
        public IEnumerator RemoveTreesInArea(Vector2 centre, Vector2 area)
        {
            //Debug.Log("Clearing Trees");
            TerrainData terrainData = TerrainManager.Instance.TerrainData;
            TreeInstance[] treesInstances = terrainData.treeInstances;
            List<TreeInstance> newTreeInstances = new List<TreeInstance>();

            //Search the area for TreeInstances
            int removedTrees = 0;
            int numTrees = 0;
            foreach (TreeInstance tree in treesInstances)
            {
                numTrees++;
                //tree.position is between 0 and 1, so scale it to fit the terrain size
                Vector2 treePosition = new Vector2(tree.position.x * terrainData.size.x, tree.position.z * terrainData.size.z);

                bool treeIsInArea =
                    treePosition.x > centre.x - (area.x / 2) && treePosition.x < centre.x + (area.x / 2)
                    && treePosition.y > centre.y - (area.y / 2) && treePosition.y < centre.y + (area.y / 2);

                //Remove those TreeInstances (i.e., only keep ones which are not in the area)
                if (!treeIsInArea)
                {
                    newTreeInstances.Add(tree);
                }
                else
                {
                    removedTrees++;
                }
                if (numTrees%100 == 0)
                {
                    yield return null;
                }
            }
            //Re-Apply Trees to Terrain
            terrainData.treeInstances = newTreeInstances.ToArray();
            yield break;
        }

    }

}