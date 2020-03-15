﻿using UnityEditor;
using UnityEngine;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
/*
 * Use Serialized properties instead of direct access  to public variables.
 * This way changes in the editor will be saved when we change and recompile the code. 
 */

    // properties
    SerializedProperty randomHeightRange;

    // foldouts
    bool showRandom = false;


    void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
    }

    public override void OnInspectorGUI() 
    {
        // Update values in GUI from Custom Terrain
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;


        showRandom = EditorGUILayout.Foldout(showRandom, "Random");
        if (showRandom)
        {
            HLine();
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
        }
        HLine();
        if (GUILayout.Button("Reset"))
        {
            terrain.Reset();
        }

        // Apply changes to Custom Terrain
        serializedObject.ApplyModifiedProperties();
    }

    private static void HLine()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
