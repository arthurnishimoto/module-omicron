using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelectorUI : MonoBehaviour
{
    [SerializeField]
    string inputHexString = "";

    [SerializeField]
    bool validHexString = false;

    [SerializeField]
    Color outputColor = Color.white;

    [SerializeField]
    Image outputColorPreviewImage = null;

    [SerializeField]
    Text colorHexText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void UpdateColorString(string inputHexString)
    {
        this.inputHexString = inputHexString;

        Color tempColor;
        validHexString = ColorUtility.TryParseHtmlString(inputHexString, out tempColor);

        if (validHexString && outputColorPreviewImage)
        {
            outputColor = tempColor;
            outputColorPreviewImage.color = outputColor;
        }
    }

    public Color GetOutputColor()
    {
        return outputColor;
    }

    public void SetColor(Color color)
    {
        outputColor = color;
        outputColorPreviewImage.color = color;

        inputHexString = ColorUtility.ToHtmlStringRGB(color);
        colorHexText.text = "#" + inputHexString;
    }
}
