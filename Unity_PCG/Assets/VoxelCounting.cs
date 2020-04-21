using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MED10.Utilities;
using MED10.Architecture.Variables;
using MED10.Architecture.Events;

public class VoxelCounting : MonoBehaviour
{
    public Terrain terrain;
    [Range(2, 10)]
    public int scaleFactor;               //The factor that the voxels are scaled down by
    //private float voxelScale;                //The scale of each voxel compared to bounding area
    private Vector3 voxelBoundingVolume;    //The bouning volume of the
    private Vector3 voxelVolume;            //The scale of an individual voxel

    private Voxel[,,] voxels;
    private TerrainCollider terrainCollider;

    public FloatVariable coroutineCompletePercentage;
    public StringVariable uiDisplayString;

    private bool coroutineRunning = false;

    public GameEvent coroutineStartEvent;
    public GameEvent coroutineEndEvent;

    void Start()
    {
        terrainCollider = terrain.GetComponent<TerrainCollider>();
        Debug.Assert(terrainCollider != null, "No terrain collider found", this);

        float boundingAreaHeight = GetBoundingAreaScaledHeight();

        InitializeVoxels(boundingAreaHeight, scaleFactor);

        if (!coroutineRunning)
        {
            coroutineRunning = true;
            coroutineStartEvent.Raise();
            StartCoroutine(CheckVoxels());
        }
        else
        {
            Debug.LogError("Coroutine already running", this);
        }
    }

    /// <summary>
    /// Initialize the voxels
    /// </summary>
    /// <param name="boundingAreaHeight">The height of the area bounding area. This will be used to determine the size of the voxels.</param>
    /// <param name="voxelScaleFactor">The factor by which the voxels are scaled down (e.g. 2 will scale them by 0.5, 4 by 0.25, 10 by 0.1 etc.)</param>
    private void InitializeVoxels(float boundingAreaHeight, int voxelScaleFactor)
    {
        float voxelScale = (float)1 / voxelScaleFactor;

        voxelBoundingVolume = new Vector3(
            terrain.terrainData.size.x,
            boundingAreaHeight + (boundingAreaHeight * voxelScale), // make sure that there is a layer of voxels above the highest point since we boxcast downwards
            terrain.terrainData.size.z);

        //voxelVolume = voxelBoundingVolume * voxelScale; //relative to bounding area
        voxelVolume = Vector3.one * boundingAreaHeight * voxelScale;     //cubes

        Vector3 numberOfVoxelsPerAxis = new Vector3(
            (int)(voxelBoundingVolume.x / voxelVolume.x),
            (int)(voxelBoundingVolume.y / voxelVolume.y),
            (int)(voxelBoundingVolume.z / voxelVolume.z));

        voxels = new Voxel[(int)numberOfVoxelsPerAxis.x, (int)numberOfVoxelsPerAxis.y, (int)numberOfVoxelsPerAxis.z];

        for (int x = 0; x < numberOfVoxelsPerAxis.x; x++)
        {
            for (int y = 0; y < numberOfVoxelsPerAxis.y; y++)
            {
                for (int z = 0; z < numberOfVoxelsPerAxis.z; z++)
                {
                    GameObject go = new GameObject
                    {
                        name = string.Format("voxel({0}, {1}, {2})", x, y, z)
                    };
                    go.transform.parent = transform;
                    Voxel v = go.AddComponent<Voxel>();
                    v.InitVoxel(new Vector3(
                            (x * voxelVolume.x) + (0.5f * voxelVolume.x),
                            (y * voxelVolume.y) + (0.5f * voxelVolume.y),
                            (z * voxelVolume.z) + (0.5f * voxelVolume.z)
                        ), voxelVolume);

                    voxels[x, y, z] = v;
                }
            }
        }
    }

    /// <summary>
    /// This fucntion will set up the bounding area for the voxels. Since the terrain is normall in the bottom 0.2 of the area, it scales the height to fit the terrain mesh.
    /// </summary>
    /// <returns>The new height of the bounding area</returns>
    private float GetBoundingAreaScaledHeight()
    {
        float height = terrain.terrainData.size.y;
        float[,] heightmap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
        float highestpoint = 0;
        for (int y = 0; y < terrain.terrainData.heightmapResolution; y++)
        {
            for (int x = 0; x < terrain.terrainData.heightmapResolution; x++)
            {
                if (heightmap[x, y] > highestpoint)
                {
                    highestpoint = heightmap[x, y];
                }
            }
        }
        height *= highestpoint;
        return height;
    }

    private IEnumerator CheckVoxels()
    {
        int numberOfVoxels = voxels.Length; ;
        int numberOfTerrainVoxels = 0;

        int iterator = 0;
        
        foreach (Voxel voxel in voxels)
        {
            float percentage =  (float)++iterator / numberOfVoxels;
            coroutineCompletePercentage.Value = percentage;
            uiDisplayString.Value = string.Format("Counting Voxels, {0} of {1}, {2}% complete", iterator, numberOfVoxels, (int)(percentage*100));
            if (voxel.ContainsTerrain())
            {
                numberOfTerrainVoxels++;
                voxel.isTerrain = true;
            }
            //else
            //{
            //    DestroyImmediate(voxel);
            //}
            yield return null;
        }
        coroutineEndEvent.Raise();
        coroutineRunning = false;
        float logNR = Mathf.Log10(numberOfTerrainVoxels);
        float log1R = Mathf.Log10(1 / (float)scaleFactor);
        Debug.Log("FD: " + logNR / log1R);
        Debug.Log(string.Format("{0} of {1} voxels contain terrain", numberOfTerrainVoxels, numberOfVoxels));
        //log(Nr)/log(1/r)
    }

    /// <summary>
    /// Draw the voxels in the editor if the parent is selected
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (voxels==null)
        {
            return;
        }
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.blue;

        foreach (Voxel voxel in voxels)
        {
            if (voxel.isTerrain)
            {
                Gizmos.DrawWireCube(voxel.position, voxel.size);
            }
        }

    }

}

class Voxel : MonoBehaviour
{
    public Vector3 position;
    public Vector3 size;
    public bool isTerrain;                              //Will be used to find fractal dimension
    private BoxCollider boxCollider;

    private void OnEnable()
    {
        boxCollider = gameObject.GetOrAddComponent<BoxCollider>();
        //boxCollider.isTrigger = true;
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public void InitVoxel()                                      //Default Position is (0, 0, 0) and Scale is (1, 1, 1)
    {
        InitVoxel(Vector3.zero, Vector3.one);
    }
    public void InitVoxel(Vector3 position)                      //Default Scale is (1, 1, 1)
    {
        InitVoxel(position, Vector3.one);
    }
    public void InitVoxel(Vector3 position, Vector3 size)        
    {
        this.position = position;
        this.size = size;
        transform.position = this.position;
        transform.localScale = this.size;
    }

    public bool ContainsTerrain()
    {
        if (Physics.BoxCast(position, size/2, Vector3.down, out RaycastHit info, Quaternion.identity, size.y))
        {
            if(info.collider.name == "Terrain")
            {
                return true;
            }
        }
        gameObject.SetActive(false);
        return false;
    }


    public bool ContainsPoint(Vector3 point)
    {
        bool containsX = (point.x > position.x - (0.5 * size.x)) && (point.x < position.x + (0.5 * size.x));
        bool containsY = (point.y > position.y - (0.5 * size.y)) && (point.y < position.y + (0.5 * size.y));
        bool containsZ = (point.z > position.z - (0.5 * size.z)) && (point.z < position.z + (0.5 * size.z));

        return containsX && containsY && containsZ;
    }


}