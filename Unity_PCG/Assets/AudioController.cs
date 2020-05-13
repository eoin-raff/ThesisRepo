using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioSource audioSource;

    public void FadeOutAudio()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        Debug.Log("Fade out Audio");
        while (audioSource.volume > 0.01f)
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, 0, Time.deltaTime);
            yield return null;
        }
        audioSource.volume = 0;
        yield break;
    }
}
