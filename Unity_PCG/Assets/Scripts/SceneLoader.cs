using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneToLoad;

    public void StartGame()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
