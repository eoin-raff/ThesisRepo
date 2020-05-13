using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    public GameObject backgroundImg, logoImg, text1, text2, text3;

    public void FadeOut()
    {
        backgroundImg.GetComponent<Image>().CrossFadeAlpha(0.0f, 2.0f, false);
        text1.GetComponent<Text>().CrossFadeAlpha(0.0f, 1.0f, false);
        text2.GetComponent<Text>().CrossFadeAlpha(0.0f, 1.0f, false);
        logoImg.GetComponent<Image>().CrossFadeAlpha(0.0f, 1.0f, false);
    }

    public void FadeIn()
    {
        backgroundImg.GetComponent<Image>().CrossFadeAlpha(1.0f, 1.0f, false);
        text1.GetComponent<Text>().CrossFadeAlpha(1.0f, 1.0f, false);
        logoImg.GetComponent<Image>().CrossFadeAlpha(1.0f, 1.0f, false);
        text2.GetComponent<Text>().CrossFadeAlpha(1.0f, 1.0f, false);
        text3.GetComponent<Text>().CrossFadeColor(Color.white, 1.0f, false, true);

    }
}
