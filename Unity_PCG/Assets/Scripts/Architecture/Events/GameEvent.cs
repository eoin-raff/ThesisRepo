using System.Collections.Generic;
using UnityEngine;

namespace MED10.Architecture.Events
{
    [CreateAssetMenu]
    public class GameEvent : ScriptableObject
    {

        private List<GameEventListener> listeners = new List<GameEventListener>();

        public void Raise()
        {
            Debug.Log("Raised Event: " + name);
            // Loop backwards through the listeners in case the respone involves removing it from the list
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();
            }
        }
        public void RegisterListener(GameEventListener listener)
        {
            listeners.Add(listener);
        }
        public void UnregisterListener(GameEventListener listener)
        {
            listeners.Remove(listener);
        }
    } 
}
