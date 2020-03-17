using UnityEditor;
using UnityEngine;
using System.IO;

public class TextureCreatorWindow : EditorWindow
{
    string filename = "myProceduralTexture";
    float perlinXScale;
    float perlinYScale;
    bool uniformScale = true;
    int perlinOctaves;
    float perlinPersistance;
    float perlinHeightScale;
    int perlinOffsetX;
    int perlinOffsetY;
    bool alphaToggle = false;
    bool seamlessToggle = false;
    bool mapToggle = false;

    Texture2D pTexture;

    [MenuItem("Window/TextureCreatorWindow")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TextureCreatorWindow));
    }

    private void OnEnable()
    {
        pTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
    }
    private void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        filename = EditorGUILayout.TextField("Texture Name", filename);

        int wSize = (int)(EditorGUIUtility.currentViewWidth - 100);
        uniformScale = EditorGUILayout.Toggle("Uniform Scale?", uniformScale);
        if (uniformScale)
        {
            perlinXScale = EditorGUILayout.Slider("Scale", perlinXScale, 0.001f, 0.1f);
            perlinYScale = perlinXScale;
        }
        else
        {
            perlinXScale = EditorGUILayout.Slider("X Scale", perlinXScale, 0.001f, 0.1f);
            perlinYScale = EditorGUILayout.Slider("Y Scale", perlinYScale, 0.001f, 0.1f);
        }
        perlinOctaves = EditorGUILayout.IntSlider("Octaves", perlinOctaves, 1, 10);
        perlinPersistance = EditorGUILayout.Slider("Persistance", perlinPersistance, 1, 10);
        perlinHeightScale = EditorGUILayout.Slider("Height Scale", perlinHeightScale, 0, 1);
        perlinOffsetX = EditorGUILayout.IntSlider("Offset X", perlinOffsetX, 0, 10000);
        perlinOffsetY = EditorGUILayout.IntSlider("Offset Y", perlinOffsetY, 0, 10000);

        alphaToggle = EditorGUILayout.Toggle("Alpha?", alphaToggle);
        seamlessToggle = EditorGUILayout.Toggle("Seamless?", seamlessToggle);
        mapToggle = EditorGUILayout.Toggle("Remap Values?", mapToggle);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Generate", GUILayout.Width(wSize)))
        {
            int w = 513;
            int h = 513;
            float pValue;
            Color pixCol = Color.white;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    pValue = Utils.fBM((x + perlinOffsetX) * perlinXScale,
                                        (y + perlinOffsetY) * perlinYScale,
                                        perlinOctaves,
                                        perlinPersistance) * perlinHeightScale;
                    float colValue = pValue;
                    pixCol = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1);
                    pTexture.SetPixel(x, y, pixCol);
                }
            }
            pTexture.Apply(false, false);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(pTexture, GUILayout.Width(wSize), GUILayout.Height(wSize));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save", GUILayout.Width(wSize)))
        {

        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}
