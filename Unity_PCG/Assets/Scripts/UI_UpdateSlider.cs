using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MED10.Architecture.Variables;

public class UI_UpdateSlider : MonoBehaviour
{
    public FloatVariable progress;
    public Slider slider;

    private void Update()
    {
        slider.value = Mathf.Clamp01(progress.Value);
    }
}
