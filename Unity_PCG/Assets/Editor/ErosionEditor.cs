using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Erosion))]
class ErosionEditor : Editor
{

    private SerializedProperty customTerrain;
    private SerializedProperty erosionType;
    private SerializedProperty erosionStrength;
    private SerializedProperty erosionAmount;
    private SerializedProperty springsPerRiver;
    private SerializedProperty solubility;
    private SerializedProperty droplets;
    private SerializedProperty erosionSmoothAmount;

    private void OnEnable()
    {

        customTerrain = serializedObject.FindProperty("customTerrain");
        erosionType = serializedObject.FindProperty("erosionType");
        erosionStrength = serializedObject.FindProperty("erosionStrength");
        erosionAmount = serializedObject.FindProperty("erosionAmount");
        springsPerRiver = serializedObject.FindProperty("springsPerRiver");
        solubility = serializedObject.FindProperty("solubility");
        droplets = serializedObject.FindProperty("droplets");
        erosionSmoothAmount = serializedObject.FindProperty("erosionSmoothAmount");

    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Erosion erosion = (Erosion)target;
        //EditorGUILayout.PropertyField(customTerrain);
        //EditorGUILayout.PropertyField(erosionType);
        //EditorGUILayout.Slider(erosionStrength, 0.00001f, 1, new GUIContent("Erosion Strength"));
        //EditorGUILayout.Slider(erosionAmount, 0.00001f, 1, new GUIContent("Erosion Amount"));
        //EditorGUILayout.IntSlider(droplets, 1, 1000, new GUIContent("Droplets"));
        //EditorGUILayout.Slider(solubility, 0.00001f, 1, new GUIContent("Solubility"));
        //EditorGUILayout.IntSlider(springsPerRiver, 0, 20, new GUIContent("Springs Per River"));
        //EditorGUILayout.IntSlider(erosionSmoothAmount, 0, 10, new GUIContent("Smooth Amount"));

        if (GUILayout.Button("Erode"))
        {
            erosion.Erode();
        }

    }
}

