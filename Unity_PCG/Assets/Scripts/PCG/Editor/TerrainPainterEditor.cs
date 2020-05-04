using EditorGUITable;
using MED10.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MED10.PCG
{
    [CustomEditor(typeof(TerrainPainter))]
    [CanEditMultipleObjects]
    public class TerrainPainterEditor : Editor, IDrawLines
    {

        private GUITableState splatMapTable;
        private SerializedProperty splatHeights;

        private GUITableState vegetationTable;
        private SerializedProperty vegetationData;
        private SerializedProperty maxTrees;
        private SerializedProperty treeSpacing;

        private GUITableState detailTable;
        private SerializedProperty details;
        private SerializedProperty maxDetails;
        private SerializedProperty detailSpacing;

        private SerializedProperty waterHeight;
        private SerializedProperty waterGO;
        private SerializedProperty shorelineMaterial;


        private SerializedProperty numberOfClouds;
        private SerializedProperty particlesPerCloud;
        private SerializedProperty cloudParticleSize;
        private SerializedProperty cloudMinSize;
        private SerializedProperty cloudMaxSize;
        private SerializedProperty cloudMaterial;
        private SerializedProperty cloudShadowMaterial;
        private SerializedProperty color;
        private SerializedProperty lining;
        private SerializedProperty minSpeed;
        private SerializedProperty maxSpeed;
        private SerializedProperty distanceTravelled;

        private bool showSplatMaps = false;
        private bool showVegetation = false;
        private bool showDetail = false;
        private bool showWater = false;
        private bool showErosion = false;
        private bool showClouds = false;

        private void OnEnable()
        {
            splatMapTable = new GUITableState("splatMapTable");
            splatHeights = serializedObject.FindProperty("splatHeights");

            vegetationTable = new GUITableState("vegetationTable");
            vegetationData = serializedObject.FindProperty("vegetationData");
            maxTrees = serializedObject.FindProperty("maxTrees");
            treeSpacing = serializedObject.FindProperty("treeSpacing");

            detailTable = new GUITableState("details");
            details = serializedObject.FindProperty("details");
            maxDetails = serializedObject.FindProperty("maxDetails");
            detailSpacing = serializedObject.FindProperty("detailSpacing");

            waterHeight = serializedObject.FindProperty("waterHeight");
            waterGO = serializedObject.FindProperty("waterGO");
            shorelineMaterial = serializedObject.FindProperty("shorelineMaterial");

            numberOfClouds = serializedObject.FindProperty("numberOfClouds");
            particlesPerCloud = serializedObject.FindProperty("particlesPerCloud");
            cloudParticleSize = serializedObject.FindProperty("cloudParticleSize");
            cloudMinSize = serializedObject.FindProperty("cloudMinSize");
            cloudMaxSize = serializedObject.FindProperty("cloudMaxSize");
            cloudMaterial = serializedObject.FindProperty("cloudMaterial");
            cloudShadowMaterial = serializedObject.FindProperty("cloudShadowMaterial");
            color = serializedObject.FindProperty("color");
            lining = serializedObject.FindProperty("lining");
            minSpeed = serializedObject.FindProperty("minSpeed");
            maxSpeed = serializedObject.FindProperty("maxSpeed");
            distanceTravelled = serializedObject.FindProperty("distanceTravelled");
        }

        public override void OnInspectorGUI()
        {
            TerrainPainter painter = (TerrainPainter)target;


            showSplatMaps = EditorGUILayout.Foldout(showSplatMaps, "Splat Maps");
            if (showSplatMaps)
            {
                HLine();
                GUILayout.Label("Splat Maps", EditorStyles.boldLabel);
                splatMapTable = GUITableLayout.DrawTable(splatMapTable, splatHeights);
                EditorGUILayout.Space(20);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+"))
                {
                    Utils.AddNewData<Splatmap>(ref painter.splatHeights);
                }
                if (GUILayout.Button("-"))
                {
                    Utils.RemoveData<Splatmap>(ref painter.splatHeights);
                }
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Apply Splatmaps"))
                {
                    painter.SplatMaps();
                }
            }

            showVegetation = EditorGUILayout.Foldout(showVegetation, "Vegetation");
            if (showVegetation)
            {
                HLine();
                GUILayout.Label("Vegetation", EditorStyles.boldLabel);

                EditorGUILayout.IntSlider(maxTrees, 0, 100000, new GUIContent("Max Trees"));
                EditorGUILayout.IntSlider(treeSpacing, 1, 20, new GUIContent("Tree Spacing"));

                vegetationTable = GUITableLayout.DrawTable(vegetationTable, vegetationData);
                EditorGUILayout.Space(20);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+"))
                {
                    Utils.AddNewData(ref painter.vegetationData);
                }
                if (GUILayout.Button("-"))
                {
                    Utils.RemoveData<Vegetation>(ref painter.vegetationData);
                }
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Apply Vegetation"))
                {
                    painter.PlantVegetation();
                }
            }

            showDetail = EditorGUILayout.Foldout(showDetail, "Details");
            if (showDetail)
            {
                HLine();
                GUILayout.Label("Details", EditorStyles.boldLabel);


                EditorGUILayout.IntSlider(maxDetails, 0, 10000, new GUIContent("Max Details"));
                EditorGUILayout.IntSlider(detailSpacing, 1, 20, new GUIContent("Detail Spacing"));
                //
                EditorGUILayout.Slider(waterHeight, 0, 1, new GUIContent("Water Height"));


                detailTable = GUITableLayout.DrawTable(detailTable, details);

                painter.GetComponent<Terrain>().detailObjectDistance = maxDetails.intValue;

                EditorGUILayout.Space(20);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("+"))
                {
                    Utils.AddNewData(ref painter.details);
                }
                if (GUILayout.Button("-"))
                {
                    Utils.RemoveData<Detail>(ref painter.details);
                }
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("Apply Details"))
                {
                    painter.PaintDetails();
                }
            }

            showWater = EditorGUILayout.Foldout(showWater, "Water");
            if (showWater)
            {
                EditorGUILayout.Slider(waterHeight, 0, 1, new GUIContent("Water Height"));
                EditorGUILayout.IntSlider(maxDetails, 0, 10000, new GUIContent("Max Details"));

                EditorGUILayout.PropertyField(waterGO, new GUIContent("Water Game Object"));

                if (GUILayout.Button("Add Water"))
                {
                    painter.AddWater();
                }
                EditorGUILayout.PropertyField(shorelineMaterial);
                if (GUILayout.Button("Add Shore"))
                {
                    painter.AddShore();
                }
            }

            showClouds = EditorGUILayout.Foldout(showClouds, "Clouds");
            if (showClouds)
            {
                EditorGUILayout.PropertyField(numberOfClouds);
                EditorGUILayout.PropertyField(particlesPerCloud);
                EditorGUILayout.PropertyField(cloudParticleSize);
                EditorGUILayout.PropertyField(cloudMinSize);
                EditorGUILayout.PropertyField(cloudMaxSize);
                EditorGUILayout.PropertyField(cloudMaterial);
                EditorGUILayout.PropertyField(cloudShadowMaterial);
                EditorGUILayout.PropertyField(color);
                EditorGUILayout.PropertyField(lining);
                EditorGUILayout.PropertyField(minSpeed);
                EditorGUILayout.PropertyField(maxSpeed);
                EditorGUILayout.PropertyField(distanceTravelled);
                if (GUILayout.Button("Generate Clouds"))
                {
                    painter.GenerateClouds();
                }
            }
        }

        public void HLine()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
    }

}