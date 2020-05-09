using MED10.PCG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MED10.PCG.TerrainGenerator;

public class ConditionAssigner : MonoBehaviour
{
    public GameObject backgroundImg, logoImg, text;


    public void FadeOut()
    {
        backgroundImg.GetComponent<Image>().CrossFadeAlpha(0.0f, 2.0f, false);
        text.GetComponent<Text>().CrossFadeAlpha(0.0f, 1.0f, false);
        logoImg.GetComponent<Image>().CrossFadeAlpha(0.0f, 1.0f, false);
    }

    public void FadeIn()
    {
        backgroundImg.GetComponent<Image>().CrossFadeAlpha(1.0f, 1.0f, false);
    }
}
