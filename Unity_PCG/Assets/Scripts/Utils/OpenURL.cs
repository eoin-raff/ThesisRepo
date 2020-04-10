using UnityEngine;

namespace MED10.Utilities
{
    [ExecuteAlways]
    public class OpenURL : MonoBehaviour
    {
        [SerializeField]
        private string URL = "https://forms.gle/9f8u7y8TAxViBpTP9";
        [SerializeField]
#pragma warning disable IDE0051 // Remove unused private members
        private readonly string URLDescription = "Default URL is a test questionnaire";
#pragma warning restore IDE0051 // Remove unused private members

        public void Open()
        {
            Application.OpenURL(URL);
        }

    }
}