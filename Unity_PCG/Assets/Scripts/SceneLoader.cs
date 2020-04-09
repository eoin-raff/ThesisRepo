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
    private void OnEnable()
    {
        loadingVariable.Value = false;
    }
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
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);

        while (!asyncLoad.isDone)
        {
            progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);    // asyncLoad.isDone becomes true at 0.9, we want to remap that to 1 for display purposes
            progressVariable.Value = progress;
            //Debug.Log(progress);
            displayText.Value = "Loading: " + (int)(progress * 100) + "%";
            Debug.Log(displayText.Value);
            yield return null;
        }
    }
}
