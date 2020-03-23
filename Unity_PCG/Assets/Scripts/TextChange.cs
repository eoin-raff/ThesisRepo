using UnityEngine;
using UnityEngine.UI;

public class TextChange : MonoBehaviour
{
    public Text text1;
    public Text text2;
    public Button startButton;
    public Button nextButton;

    public void TextChanger() 
    {
        text1.text = text2.text;

        nextButton.interactable = false;


        
    }
}
