using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MED10.PCG
{
    [CustomEditor(typeof(RuntimeGeneration))]
    public class RuntimeGenerationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            RuntimeGeneration generator = (RuntimeGeneration)target;
            if (GUILayout.Button("Generate"))
            {
                generator.Generate();
            }
            base.OnInspectorGUI();
        }
    }
}