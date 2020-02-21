using UnityEngine;

namespace MED10.Utilities
{
    public class HideOnPlay : MonoBehaviour
    {
        void Start()
        {
            gameObject.SetActive(false);
        }
    }

}