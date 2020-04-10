using UnityEngine;
using UnityEngine.Events;

namespace MED10.Architecture.Events
{
    public class GameEventListener : MonoBehaviour
    {
        public GameEvent Event;
        public UnityEvent Response;

        private void OnEnable()
        {
            Event.RegisterListener(this);
        }
        private void OnDisable()
        {
            Event.UnregisterListener(this);
        }
        public void OnEventRaised()
        {
            Response.Invoke();
        }
    }
}