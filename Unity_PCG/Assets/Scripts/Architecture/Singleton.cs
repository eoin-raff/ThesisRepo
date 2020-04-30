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
        private static Type instance;

        /// <summary>
        /// Lazy instantiation of singleton object
        /// </summary>
        /// <returns></returns>
        public static Type Instance()
        {
            if (instance == null)
            {
                GameObject singletonObject = new GameObject();
                Type type = singletonObject.AddComponent<Type>();
                singletonObject.name = type.name;
                singletonObject.transform.position = Vector3.zero;
                singletonObject.transform.rotation = Quaternion.identity;
                return type;
            }
            else
            {
                return instance;
            }
        }
        protected void Awake()
        {
            if (instance == null)
            {
                instance = gameObject.GetComponent<Type>();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    } 
}