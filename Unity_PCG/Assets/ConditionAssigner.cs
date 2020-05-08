using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConditionAssigner : MonoBehaviour
{
    public int condition;
    public GameObject backgroundImg, logoImg, text;

    public void AssignCondition()
    {
        float rnd = Random.value;

        if (rnd > 0.5f)
        {
            condition = 1;
        }
        else
        {
            condition = 0;
        }
    }

    public void Fader()
    {
        backgroundImg.GetComponent<Image>().CrossFadeAlpha(0.0f, 2.0f, false);
        text.GetComponent<Text>().CrossFadeAlpha(0.0f, 1.0f, false);
        logoImg.GetComponent<Image>().CrossFadeAlpha(0.0f, 1.0f, false);
    }
}
