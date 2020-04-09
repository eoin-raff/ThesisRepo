using MED10.Architecture.Variables;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public int sceneIndex;
    public StringVariable displayText;
    public FloatVariable progressVariable;
    public BoolVariable loadingVariable;

    private float progress;
    public void Load()
    {
        if (!loadingVariable.Value)
        {
            loadingVariable.Value = true;
            StartCoroutine(LoadSceneInBackground());
        }
    }
    IEnumerator LoadSceneInBackground()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        while (!asyncLoad.isDone)
        {
            progress = Mathf.Clamp01(asyncLoad.progress);
            progressVariable.Value = progress;
            displayText.Value = "Loading: " + (int)progress * 100 + "%";
            yield return null;
        }
    }
}
