using MED10.Architecture.Variables;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public int sceneIndex;
    public StringVariable displayText;

    public void Load()
    {
        StartCoroutine(LoadSceneInBackground());
    }
    IEnumerator LoadSceneInBackground()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
