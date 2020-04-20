using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MED10.Architecture.Variables;
using MED10.Architecture.Events;

public class LoadingBar : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    public Slider slider;

    public StringVariable textContent;
    public FloatVariable percentage;

    public GameEvent startEvent;
    public GameEvent endEvent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textDisplay.text = textContent.Value;
        slider.value = percentage.Value;
    }
}
