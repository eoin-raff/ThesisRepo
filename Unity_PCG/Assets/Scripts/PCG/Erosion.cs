using UnityEngine;
using System.Collections.Generic;

public class Erosion : MonoBehaviour
{
    public CustomTerrain terrain;
    public enum ErosionType { Rain, Thermal, Tidal, River, Wind };
    public ErosionType erosionType = ErosionType.Rain;
    public float erosionStrength = 0.1f;
    public float erosionAmount = 0.5f;
    public int springsPerRiver = 5;
    public float solubility = 0.01f;
    public int droplets = 10;
    public int erosionSmoothAmount = 5;
    public int resolution;

    private void Awake()
    {
        resolution = terrain.terrainData.heightmapResolution;
    }
    public void Erode()
    {
        switch (erosionType)
        {
            case ErosionType.Rain:
                Rain();
                break;
            case ErosionType.Thermal:
                Thermal();
                break;
            case ErosionType.Tidal:
                Tidal();
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

        //smoothAmount = erosionSmoothAmount;
        //for (int i = 0; i < erosionSmoothAmount; i++)
        //{
        //Smooth();
        //}
    }

    private void Wind()
    {
        /*
         * This algorithm simulates particles being lifted and despostied, or dragged across a surface by wind
         */
        float[,] heightMap = terrain.GetHeightMap(false);

        float windDir = 30;                                                                              //angle of the wind between 0 and 360

        float sin = -Mathf.Sin(Mathf.Deg2Rad * windDir);
        float cos = Mathf.Cos(Mathf.Deg2Rad * windDir);

        //Loop a much larger area than heightmap to accommodate for rotation in final step.
        for (int y = -(resolution - 1) * 2; y < resolution * 2; y += 10) // Skip ahead a larger amount on the y axis
        {
            for (int x = -(resolution - 1) * 2; x < resolution * 2; x++)
            {
                float noise = (float)Mathf.PerlinNoise(x * 0.06f, y * 0.06f) * 20 * erosionStrength;    //Get a Perlin Noise value for waves
                int nx = x;
                int digY = y + (int)noise; //Dig a trench at the current Y values offset by noise
                int ny = digY + 5;         //Find a new Y which is a step away from the digY

                //rotate both dig (x, digY) and pile (nx, ny) by wind direction
                Vector2 digCoords = new Vector2(x * cos - digY * sin, digY * cos + x * sin);
                Vector2 pileCoords = new Vector2(nx * cos - ny * sin, ny * cos + nx * sin);

                bool digOutOfBounds = (digCoords.x < 0 || digCoords.x > (resolution - 1) || digCoords.y < 0 || digCoords.y > (resolution - 1));
                bool pileOutOfBounds = (pileCoords.x < 0 || pileCoords.x > (resolution - 1) || pileCoords.y < 0 || pileCoords.y > (resolution - 1));
                if (!(pileOutOfBounds || digOutOfBounds))  //Check that nx and ny are valid points within the heightMap
                {
                    heightMap[(int)digCoords.x, (int)digCoords.y] -= erosionAmount;                  //Dig a Trench at the digY
                    heightMap[(int)pileCoords.x, (int)pileCoords.y] += erosionAmount;                     //Deposit sediment at ny
                }
            }
        }
        terrain.terrainData.SetHeights(0, 0, heightMap);
    }

    private void River()
    {
        float[,] heightMap = terrain.GetHeightMap(false);                                           // Get the current HeightMap without resetting terrain

        float[,] erosionMap = new float[                                                    // Create a new map to keep track of the rivers
            resolution,                                                //  with the same size as heightMap
            resolution];

        for (int i = 0; i < droplets; i++)                                                  // Droplets controls the number of rivers
        {
            Vector2 dropletPosition = new Vector2(                                          // Create droplets in random positions on the heightMap
                UnityEngine.Random.Range(0, resolution),
                UnityEngine.Random.Range(0, resolution));

            erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] = erosionStrength;   // Initialize erosionMap with values of erosionStrength at each droplet's position

            for (int j = 0; j < springsPerRiver; j++)                                       // Springs per river determines how many directions the river will flow in.
            {
                erosionMap = RunRiver(dropletPosition,                                      // Call RunRiver to calculate the river's path down the terrain
                    heightMap,
                    erosionMap,
                    resolution, resolution);
            }
        }

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                if (erosionMap[x, y] > 0)
                {
                    heightMap[x, y] -= erosionMap[x, y];                                    // Apply the erosion map to the heightmap
                }
            }
        }
        terrain.terrainData.SetHeights(0, 0, heightMap);                                            // Apply heightMap to Terrain
    }

    private float[,] RunRiver(Vector2 dropletPosition, float[,] heightMap, float[,] erosionMap, int width, int height)
    {
        while (erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] > 0)
        {
            List<Vector2> neighbors = Utils.GetNeighbors(dropletPosition, width, height);                             //Get the neighboring positions
            neighbors.Shuffle();                                                                                //Shuffle them so we don't always get the same value
            bool foundLower = false;
            foreach (Vector2 n in neighbors)
            {
                if (heightMap[(int)n.x, (int)n.y] < heightMap[(int)dropletPosition.x, (int)dropletPosition.y])  // If the neighbor is lower than the droplet
                {
                    erosionMap[(int)n.x, (int)n.y] = erosionMap[(int)dropletPosition.x,
                                                                (int)dropletPosition.y]
                                                              - solubility;                                     // Set the erosionMap at the neighbors position equal to the erosionMap at droplet position - solubility
                    dropletPosition = n;                                                                        // Check from the neighbors position next time. I.e. move downhill, gathering sediment
                    foundLower = true;
                    break;                                                                                      // Stop searching the neighbors once you have found one suitable
                }
            }
            if (!foundLower)                                                                                    // If you have checked all neighbors and not found one lower
            {
                erosionMap[(int)dropletPosition.x, (int)dropletPosition.y] -= solubility;                       // Then reduce your current value (i.e. drop off sediment)
            }
        }

        return erosionMap;
    }

    private void Thermal()
    {
        /*
         *  Simulate erosion from landslides by checking the steepness of a slope
         *  If the steepness is above a certain threshold, then move some of the current height to the lower neighbor
         *  This causes cliffs with lower slopes beneath them
         */
        Debug.Log("Thermal Erosion");
        float[,] heightMap = terrain.GetHeightMap(false);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector2 location = new Vector2(x, y);
                List<Vector2> neighbors = Utils.GetNeighbors(location, resolution, resolution);
                foreach (Vector2 n in neighbors)
                {
                    if (heightMap[x, y] > heightMap[(int)n.x, (int)n.y] + erosionStrength)
                    {
                        //  Debug.Log("Eroding");
                        float currentHeight = heightMap[x, y];
                        heightMap[x, y] -= currentHeight * erosionAmount;
                        heightMap[(int)n.x, (int)n.y] += currentHeight * erosionAmount;
                    }
                }
            }
        }
        terrain.terrainData.SetHeights(0, 0, heightMap);
    }

    private void Tidal()
    {
        /*
         *  Simulate erosion from waves beating at landscape
         *  like a blend of thermal and shorline
         */
        Debug.Log("Tidal Erosion");
        float[,] heightMap = terrain.GetHeightMap(false);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector2 location = new Vector2(x, y);
                List<Vector2> neighbors = Utils.GetNeighbors(location, resolution, resolution);
                foreach (Vector2 n in neighbors)
                {
                    if (heightMap[x, y] < terrain.waterHeight && heightMap[(int)n.x, (int)n.y] > terrain.waterHeight)
                    {
                        heightMap[x, y] = terrain.waterHeight;
                        heightMap[(int)n.x, (int)n.y] = terrain.waterHeight;
                    }
                }
            }
        }
        terrain.terrainData.SetHeights(0, 0, heightMap);
    }

    private void Rain()
    {
        /*
         *  Simulate erosion by rain by creating divots all across the terrain
         */


        float[,] heightMap = terrain.GetHeightMap(false);

        for (int i = 0; i < droplets; i++)
        {
            heightMap[UnityEngine.Random.Range(0, resolution),
                      UnityEngine.Random.Range(0, resolution)]
                    -= erosionStrength;
        }
        terrain.terrainData.SetHeights(0, 0, heightMap);
    }

}
