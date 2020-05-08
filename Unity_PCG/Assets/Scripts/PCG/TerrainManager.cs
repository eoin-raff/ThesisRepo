using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MED10.Architecture;
using UnityEditor;
using System;

namespace MED10.PCG
{
    //[ExecuteInEditMode]
    [RequireComponent(typeof(Terrain))]
    [RequireComponent(typeof(TerrainGenerator))]
    [RequireComponent(typeof(Erosion))]
    [RequireComponent(typeof(TerrainPainter))]
    public class TerrainManager : Singleton<TerrainManager>
    {
        public int seed;
        public Terrain Terrain { get; private set; }
        public TerrainData TerrainData { get; private set; }

        [SerializeField]
        private TerrainGenerator terrainGenerator;
        [SerializeField]
        private TerrainPainter painter;
        [SerializeField]
        private Erosion erosion;


        #region Aggregate Classes Encapsulation
        public void SetTerrainGenerator(TerrainGenerator terrainGenerator)
        {
            if (this.terrainGenerator == null)
            {
                this.terrainGenerator = terrainGenerator;
            }
            //else if (this.terrainGenerator == terrainGenerator)
            //{
            //    Debug.LogWarning("This generator has already been assigned to the terrain manager", terrainGenerator);
            //}
            //else{
            //    Debug.LogWarning("Terrain Manager already has another terrain generator", this);
            //}
        }
        public TerrainGenerator GetTerrainGenerator()
        {
            if (terrainGenerator == null)
            {
                Debug.LogWarning("Terrain Manager has no terrain generator", this);
                return null;
            }
            return terrainGenerator;
        }
        public void SetErosion(Erosion erosion)
        {
            if (this.erosion == null)
            {
                this.erosion = erosion;
            }
            //else if (this.erosion == erosion)
            //{
            //    Debug.LogWarning("This erosion has already been assigned to the terrain manager", terrainGenerator);
            //}
            //else
            //{
            //    Debug.LogWarning("Terrain Manager already has erosion", this);
            //}
        }
        public void SetPainter(TerrainPainter painter)
        {
            if (this.painter == null)
            {
                this.painter = painter;
            }
//            else if (this.painter == painter)
//            {
////                Debug.LogWarning("This painter has already been assigned to the terrain manager", painter);
//            }

//            else
//            {
//                Debug.LogWarning("Terrain Manager already has a painter", this);
//            }
        }
        public TerrainPainter GetPainter()
        {
            if (painter == null)
            {
                //Debug.LogWarning("Terrain Manager has no painter", this);
                return null;
            }
            return painter;
        }
        #endregion

        [SerializeField]
        private bool resetHeightmap = false;
        public bool ResetHeightmap { get => resetHeightmap; private set => resetHeightmap = value; }

        #region Heightmaps
        public float[,] GetHeightmap()
        {
            return GetHeightmap(resetHeightmap);
        }
        public float[,] GetHeightmap(bool resetHeightmap)
        {
            return GetTerrainGenerator().GetHeightMap(resetHeightmap);
        }
        public void SetHeightmap(float[,] heightmap)
        {
            TerrainData.SetHeights(0, 0, heightmap);
        }
        public int HeightmapResolution { get { return TerrainData.heightmapResolution; } } 
        #endregion

        #region Tags & Layers

        public enum TagType { Tag = 0, Layer = 1 }
        [SerializeField]
        int terrainLayer = -1;
        public int TerrainLayer { get { return terrainLayer; } }

        #endregion

        private void OnEnable()
        {
            InitTerrainManager();
        }

        private void InitTerrainManager()
        {
            Terrain = GetComponent<Terrain>();
            TerrainData = Terrain.activeTerrain.terrainData;
            //SetErosion(GetComponent<Erosion>());
            //SetTerrainGenerator(GetComponent<TerrainGenerator>());
            //SetPainter(GetComponent<TerrainPainter>());
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
        private new void Awake()
        {
            base.Awake();
            InitTerrainManager();
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
            AddTag(layerProp, "Sky", TagType.Layer);
            terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);
            tagManager.ApplyModifiedProperties();

            // tag this object
            this.gameObject.tag = "Terrain";
            this.gameObject.layer = terrainLayer;
#endif
        }
    }

}