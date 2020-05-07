using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConditionAssigner : MonoBehaviour
{
    public int condition;
    public GameObject showCon;
    public GameObject backgroundImg, text1, text2;

    public void AssignCondition()
    {
        float rnd = Random.value;

        if (rnd > 0.5f)
        {
            condition = 1;
            showCon.GetComponent<Text>().text = ("Condition: 1 : " + rnd);
        }
        else
        {
            condition = 0;
            showCon.GetComponent<Text>().text = ("Condition: 0 : " + rnd);
        }
    }

    public void Fader()
    {
        backgroundImg.GetComponent<Image>().CrossFadeAlpha(0.0f, 2.0f, false);
        text1.GetComponent<Text>().CrossFadeAlpha(0.0f, 1.0f, false);
        text2.GetComponent<Text>().CrossFadeAlpha(0.0f, 1.0f, false);
    }
}
