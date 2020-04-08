using UnityEngine;
using UnityEditor;

namespace MED10.PCG
{
    [CustomEditor(typeof(Erosion))]
    class ErosionEditor : Editor
    {

        private SerializedProperty terrain;
        private SerializedProperty erosionType;

        private SerializedProperty rainDroplets;
        private SerializedProperty rainErosionStrength;

        private SerializedProperty riverSprings;
        private SerializedProperty riverSolubility;
        private SerializedProperty riverErosionStrength;
        private SerializedProperty riverSourcePoints;

        private SerializedProperty thermalErosionSensitivity;
        private SerializedProperty thermalErosionAmount;

        private SerializedProperty tidalErosionStrength;

        private SerializedProperty windAngle;
        private SerializedProperty windErosionAmount;
        private SerializedProperty windErosionStrength;


        private void OnEnable()
        {
            terrain = serializedObject.FindProperty("terrain");
            erosionType = serializedObject.FindProperty("erosionType");

            rainDroplets = serializedObject.FindProperty("rainDroplets");
            rainErosionStrength = serializedObject.FindProperty("rainErosionStrength");

            riverSprings = serializedObject.FindProperty("riverSprings");
            riverSolubility = serializedObject.FindProperty("riverSolubility");
            riverErosionStrength = serializedObject.FindProperty("riverErosionStrength");
            riverSourcePoints = serializedObject.FindProperty("riverSourcePoints");

            thermalErosionSensitivity = serializedObject.FindProperty("thermalErosionSensitivity");
            thermalErosionAmount = serializedObject.FindProperty("thermalErosionAmount");

            tidalErosionStrength = serializedObject.FindProperty("tidalErosionStrength");
            
            windAngle = serializedObject.FindProperty("windAngle");
            windErosionAmount = serializedObject.FindProperty("windErosionAmount");
            windErosionStrength = serializedObject.FindProperty("windErosionStrength"); 
        }

        public override void OnInspectorGUI()
        {
            
            //base.OnInspectorGUI();
            Erosion erosion = (Erosion)target;
            
            EditorGUILayout.PropertyField(terrain);
            EditorGUILayout.PropertyField(erosionType);
            Erosion.ErosionType type = (Erosion.ErosionType)erosionType.enumValueIndex;
            switch (type)
            {
                case Erosion.ErosionType.Rain:
                    EditorGUILayout.IntSlider(rainDroplets, 0, 125000);
                    EditorGUILayout.Slider(rainErosionStrength, 0.0001f, 0.1f);
                    break;
                case Erosion.ErosionType.River:
                    EditorGUILayout.PropertyField(riverSourcePoints);
                    EditorGUILayout.PropertyField(riverSprings);
                    EditorGUILayout.Slider(riverErosionStrength, 0.00001f, 0.05f);
                    EditorGUILayout.Slider(riverSolubility, 0.00001f, 0.1f);
                    break;
                case Erosion.ErosionType.Thermal:
                    EditorGUILayout.Slider(thermalErosionAmount, 0, 1);
                    EditorGUILayout.Slider(thermalErosionSensitivity, 0.000001f, 0.001f) ;
                    break;
                case Erosion.ErosionType.Tidal:
                    EditorGUILayout.Slider(tidalErosionStrength, 0, 1);
                    break;
                case Erosion.ErosionType.Wind:
                    EditorGUILayout.Slider(windAngle, 0, 360);
                    EditorGUILayout.Slider(windErosionAmount, 0.001f, 0.01f);
                    EditorGUILayout.Slider(windErosionStrength, 0.01f, 10f);
                    break;
                default:
                    break;
            }

            //EditorGUILayout.Slider(erosionStrength, 0.00001f, 1, new GUIContent("Erosion Strength"));
            //EditorGUILayout.Slider(erosionAmount, 0.00001f, 1, new GUIContent("Erosion Amount"));
            //EditorGUILayout.IntSlider(droplets, 1, 1000, new GUIContent("Droplets"));
            //EditorGUILayout.Slider(solubility, 0.00001f, 1, new GUIContent("Solubility"));
            //EditorGUILayout.IntSlider(springsPerRiver, 0, 20, new GUIContent("Springs Per River"));
            //EditorGUILayout.IntSlider(erosionSmoothAmount, 0, 10, new GUIContent("Smooth Amount"));
            
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Erode"))
            {
                erosion.Erode();
            }

        }
    }

}
