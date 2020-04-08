using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MED10.PCG
{
    [CustomEditor(typeof(OnlineTerrainGenerator))]
    public class OnlineTerrainGeneratorEditor : Editor
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public override void OnInspectorGUI()
        {
            OnlineTerrainGenerator generator = (OnlineTerrainGenerator)target;
            if (GUILayout.Button("Generate"))
            {
                generator.Generate();
            }
            base.OnInspectorGUI();
        }
    }

}