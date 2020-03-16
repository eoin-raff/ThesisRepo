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

    // foldouts
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlin = false;


    void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        perlinScaleX = serializedObject.FindProperty("perlinScaleX");
        perlinScaleY = serializedObject.FindProperty("perlinScaleY");
        perlinOffsetX = serializedObject.FindProperty("perlinOffsetX");
        perlinOffsetY = serializedObject.FindProperty("perlinOffsetY");
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

            if (GUILayout.Button("Perlin Noise"))
            {
                terrain.Perlin();
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
