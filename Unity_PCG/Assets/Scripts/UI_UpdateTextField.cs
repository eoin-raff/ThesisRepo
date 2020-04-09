using MED10.Architecture.Variables;
using TMPro;
using UnityEngine;

public class UI_UpdateTextField : MonoBehaviour
{
    public TextMeshProUGUI textField;
    [SerializeField]
    private StringVariable text;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(textField != null, "No text display component", this);
        Debug.Assert(text != null, "No string reference found", this);
    }

    // Update is called once per frame
    void Update()
    {
        textField.text = text.Value;
    }
}
