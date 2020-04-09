using MED10.Architecture.Events;
using MED10.Architecture.Variables;
using TMPro;
using UnityEngine;

namespace MED10.PCG
{
    public class EnvironmentSeedSelector : MonoBehaviour
    {
        [SerializeField]
        private IntReference fixedSeed;

        public IntVariable EnvironmentSeed;
        public TMP_InputField input;

        public GameEvent seedSetEvent;

        private StringVariable displayText;

        private void OnEnable()
        {
            Debug.Assert(fixedSeed != null, "No fixed seed reference found.", this);
            Debug.Assert(EnvironmentSeed != null, "No environment seed variable found.", this);
            Debug.Assert(displayText != null, "No text display variable found.", this);
            Debug.Assert(input != null, "No input field found", this);
            Debug.Assert(seedSetEvent != null, "No game event assigned", this);
            displayText.Value = "";
        }
        public void SetFixed()
        {
            SetSeed(fixedSeed.Value);
        }
        public void SetRandom()
        {
            SetSeed(System.DateTime.Now.Millisecond);
        }
        public void SetFromInput()
        {
            if (input)
            {
                if (int.TryParse(input.text, out int seed))
                {
                    SetSeed(seed);
                }
                else
                {
                    displayText.Value = "Input not valid. Please enter a whole number";
                }
            }
        }
        private void SetSeed(int seed)
        {
            EnvironmentSeed.Value = seed;
            if (seedSetEvent)
            {
                seedSetEvent.Raise();
            }
            displayText.Value = "Environment Seed sucessfully set to " + seed;
        }
    } 
}
