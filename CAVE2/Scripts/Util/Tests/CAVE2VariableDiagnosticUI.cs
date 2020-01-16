using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CAVE2VariableDiagnosticUI : MonoBehaviour
{

    Text uiText;

    // Start is called before the first frame update
    void Start()
    {
        uiText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        uiText.text = "IsMaster() = " + CAVE2.IsMaster() + "\n";
        uiText.text += "OnCAVE2Display() = " + CAVE2.OnCAVE2Display() + "\n";
        uiText.text += "VR Enabled = " + UnityEngine.XR.XRSettings.enabled + "\n";
        uiText.text += "VR Model = '" + UnityEngine.XR.XRDevice.model + "'\n";
    }
}
