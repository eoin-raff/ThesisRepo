using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapPreview))]
public class MapPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapPreview preview = (MapPreview)target;

        if (DrawDefaultInspector())
        {
            if (preview.AutoUpdate)
            {
                preview.DrawMapInEditor();
                
            }
        }   

        if (GUILayout.Button("Generate"))
        {
            preview.DrawMapInEditor();
        }
    }
}
