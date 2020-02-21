using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class getReal3DSensorLister : MonoBehaviour
{
#if USING_GETREAL3D
    Text uiText;

    List<float> valuators;
    List<int> buttons;

    // Start is called before the first frame update
    void Start()
    {
        uiText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {

        string[] sensorList = getReal3D.Input.sensorsName();

        // Sensors / Trackers
        uiText.text = "Sensor List:" + "\n";
        for (int i = 0; i < sensorList.Length; i++)
        {
            getReal3D.Sensor sensor = getReal3D.Input.GetSensor(sensorList[i]);
            uiText.text += "[" + i + "] = " + sensorList[i] + " Pos: " + sensor.position + " Rot: " + sensor.rotation + "" + "\n";
        }

        // Valuators / Axes
        string[] valuatorList = getReal3D.Input.valuatorsName();
        valuators = getReal3D.Input.valuators;

        uiText.text += "\n";
        uiText.text += "Valuator Names:" + "\n";
        for (int i = 0; i < valuatorList.Length; i++)
        {
          uiText.text += "[" + i + "] = " + valuatorList[i] + "\n";
        }

        uiText.text += "\n";
        uiText.text += "Valuators:" + "\n";
        for (int i = 0; i < valuators.Count; i++)
        {
            uiText.text += "[" + i + "] = " + valuators[i] + "\n";
        }

        // Buttons
        string[] buttonList = getReal3D.Input.buttonsName();
        buttons = getReal3D.Input.buttons;

        uiText.text += "\n";
        uiText.text += "Button Names:" + "\n";
        for (int i = 0; i < buttonList.Length; i++)
        {
            uiText.text += "[" + i + "] = " + buttonList[i] + "\n";
        }

        uiText.text += "\n";
        uiText.text += "Buttons:" + "\n";
        for (int i = 0; i < buttons.Count; i++)
        {
            uiText.text += "[" + i + "] = " + buttons[i] + "\n";
        }

    }
    #endif
}
