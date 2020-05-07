using MED10.Architecture.Events;
using MED10.Architecture.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace MED10.PCG
{
    public class RuntimeGeneration : MonoBehaviour
    {
        public GameEvent heightmapDone;

        public IntVariable seed;
        public TerrainGenerator terrainGenerator;
        public UnityEvent GenerationEvents;
        public int ErosionGenerations = 1;
        public UnityEvent ErosionEvents;
        public UnityEvent PaintingEvents;

        // Start is called before the first frame update
        void OnEnable()
        {
            Debug.Assert(GenerationEvents != null, "No Generation Events found", this);
            Debug.Assert(ErosionEvents != null, "No Erosion Events found", this);
            Debug.Assert(PaintingEvents != null, "No Painting Events found", this);
        }

        private void Start()
        {
            Generate();
        }

        public void Generate()
        {
            //terrain.SetRandomSeed();
            seed.Value = terrainGenerator.Seed;
            terrainGenerator.ResetTerrain();
            GenerateHeightmap();
            PerformErosion();

            SmoothTerrain();

            PaintTerrainDetails();
            heightmapDone.Raise();
        }

        public void SmoothTerrain()
        {
            terrainGenerator.Smooth();
            //smoothingDone.Raise();
        }

        public void PaintTerrainDetails()
        {
            if (PaintingEvents != null)
            {
                PaintingEvents.Invoke();
            }
        }

        public void PerformErosion()
        {
            if (ErosionEvents != null)
            {
                for (int i = 0; i < ErosionGenerations; i++)
                {
                    ErosionEvents.Invoke();
                }
            }
        }

        public void GenerateHeightmap()
        {
            if (GenerationEvents != null)
            {
                GenerationEvents.Invoke();
            }
        }
    }

}