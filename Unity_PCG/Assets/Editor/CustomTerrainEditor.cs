using UnityEditor;
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
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    SerializedProperty perlinScaleX;
    SerializedProperty perlinScaleY;
    SerializedProperty perlinOffsetX;
    SerializedProperty perlinOffsetY;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;

    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters; 
    SerializedProperty resetTerrain;

    SerializedProperty voronoiPeakCount;
    SerializedProperty voronoiFallOff;
    SerializedProperty voronoiDropOff;
    SerializedProperty voronoiMinHeight;
    SerializedProperty voronoiMaxHeight;


    // foldouts
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlin = false;
    bool showMultiplePerlin = false;
    bool showVoroni = false;

    void OnEnable()
    {
        resetTerrain = serializedObject.FindProperty("resetTerrain");

        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");

        perlinScaleX = serializedObject.FindProperty("perlinScaleX");
        perlinScaleY = serializedObject.FindProperty("perlinScaleY");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");

        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty("perlinParameters");

        voronoiPeakCount = serializedObject.FindProperty("voronoiPeakCount");
        voronoiFallOff = serializedObject.FindProperty("voronoiFallOff");
        voronoiDropOff = serializedObject.FindProperty("voronoiDropOff");
        voronoiMinHeight = serializedObject.FindProperty("voronoiMinHeight");
        voronoiMaxHeight = serializedObject.FindProperty("voronoiMaxHeight");
    }

    public override void OnInspectorGUI() 
    {
        // Update values in GUI from Custom Terrain
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;
        EditorGUILayout.PropertyField(resetTerrain);

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

        showLoadHeights = EditorGUILayout.Foldout(showLoadHeights, "Load Heights");
        if (showLoadHeights)
        {
            HLine();
            GUILayout.Label("Set Heights From Image", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapScale);
            EditorGUILayout.PropertyField(heightMapImage);
            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
        }

        showPerlin = EditorGUILayout.Foldout(showPerlin, "Perlin Noise");
        if (showPerlin)
        {
            GUILayout.Label("Set Heights From Perlin Noise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(perlinScaleX, 0, 0.05f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinScaleY, 0, 0.05f, new GUIContent("Y Scale"));            
            EditorGUILayout.IntSlider(perlinOffsetX, 0, 1000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinOffsetY, 0, 1000, new GUIContent("Y Offset"));
            EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, 0, 1, new GUIContent("Persistance"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("Height Scale"));


            if (GUILayout.Button("Perlin Noise"))
            {
                terrain.Perlin();
            }
        }

        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perlin");
        if (showMultiplePerlin)
        {
            HLine();
            GUILayout.Label("Multiple Perlin Noise", EditorStyles.boldLabel);
            perlinParameterTable = GUITableLayout.DrawTable(perlinParameterTable,
                perlinParameters);
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewPerlin();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Multiple Perlin"))
            {
                terrain.MultiplePerlinTerrain();
            }
        }

        showVoroni = EditorGUILayout.Foldout(showVoroni, "Voronoi");
        if (showVoroni)
        {
            EditorGUILayout.IntSlider(voronoiPeakCount, 0, 50, new GUIContent("Peak Count"));
            EditorGUILayout.Slider(voronoiFallOff, 0, 5, new GUIContent("Fall Off"));
            EditorGUILayout.Slider(voronoiDropOff, 0, 5, new GUIContent("Drop Off"));
            float minValue = voronoiMinHeight.floatValue;
            float maxValue = voronoiMaxHeight.floatValue;
            EditorGUILayout.LabelField("Peak Height Range");
            EditorGUILayout.LabelField("Min: " + minValue);
            EditorGUILayout.LabelField("Max: " + maxValue);
            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, 0, 1);
            voronoiMaxHeight.floatValue = maxValue;
            voronoiMinHeight.floatValue = minValue;

            if (GUILayout.Button("Voronoi"))
            {
                terrain.Voronoi();
            }
        }

        HLine();
        if (GUILayout.Button("Reset"))
        {
            terrain.ResetTerrain();
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
