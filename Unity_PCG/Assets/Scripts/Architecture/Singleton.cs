using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MED10.Architecture
{
    /// <summary>
    /// The singeton abstract class is designed to be inherited by any object which wants singleton functionality.
    /// </summary>
    /// <typeparam name="Type">The type of class to function as a singleton. e.g. public class GameManager : Singleton<GameManager></typeparam>
    public abstract class Singleton<Type> : MonoBehaviour where Type : MonoBehaviour
    {
        public static Type Instance { get; private set; }

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = gameObject.GetComponent<Type>();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    } 
}