using UnityEngine;
using UnityEngine.Events;

namespace MED10.PCG
{
    [ExecuteAlways]
    public class RuntimeGeneration : MonoBehaviour
    {
        public TerrainGenerator terrain;
        public UnityEvent GenerationEvents;
        public int ErosionGenerations = 1;
        public UnityEvent ErosionEvents;
        public UnityEvent PaintingEvents;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Assert(GenerationEvents != null, "No Generation Events found", this);
            Debug.Assert(ErosionEvents != null, "No Erosion Events found", this);
            Debug.Assert(PaintingEvents != null, "No Painting Events found", this);
            Generate();
        }

        public void Generate()
        {
            Debug.Log("Generating from seed " + terrain.Seed);
            if (terrain.seedType == TerrainGenerator.SeedType.Random)
            {
                terrain.Seed = System.DateTime.Now.Millisecond;
            }
            if (GenerationEvents != null)
            {
                GenerationEvents.Invoke();
            }
            for (int i = 0; i < ErosionGenerations; i++)
            {
                if (ErosionEvents != null)
                {
                    ErosionEvents.Invoke();
                }
            }
            terrain.Smooth();
            if (PaintingEvents != null)
            {
                PaintingEvents.Invoke();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        void Erode()
        {

        }
        void PaintTerrain()
        {

        }
    }

}