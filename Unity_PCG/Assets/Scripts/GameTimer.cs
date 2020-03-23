using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    public float timeToPlay;

    public Canvas endScreen;

    public string urlToOpen;

    private bool hasPaused = false;

    private void Update()
    {

        timeToPlay -= Time.deltaTime;

        if (timeToPlay <= 0.0f && hasPaused == false)
        {
            endScreen.gameObject.SetActive(true);
            hasPaused = true;
        }
    }

    public void OpenUrl() 
    {
        Application.OpenURL(urlToOpen);
    }

    public void ResumeGame() 
    {
        endScreen.gameObject.SetActive(false);
    }
}
