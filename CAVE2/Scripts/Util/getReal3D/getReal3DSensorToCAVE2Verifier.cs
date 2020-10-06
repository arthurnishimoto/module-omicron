using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class getReal3DSensorToCAVE2Verifier : MonoBehaviour {
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
        uiText.text = " Head Pos: " + getReal3D.Input.head.position + " Rot: " + getReal3D.Input.head.rotation + "" + "\n";
        uiText.text = " Wand Pos: " + getReal3D.Input.wand.position + " Rot: " + getReal3D.Input.wand.rotation + "" + "\n";

        uiText.text += "\n";

        uiText.text += "Wand 1:" + "\n";

        uiText.text += " Left Analog LR: " + CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.LeftAnalogStickLR) + " = " + getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.LeftAnalogStickLR)) + "\n";
        uiText.text += " Left Analog UD: " + CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.LeftAnalogStickUD) + " = " + getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.LeftAnalogStickUD)) + "\n";

        uiText.text += " Right Analog LR: " + CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.RightAnalogStickLR) + " = " + getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.RightAnalogStickLR)) + "\n";
        uiText.text += " Right Analog UD: " + CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.RightAnalogStickUD) + " = " + getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.RightAnalogStickUD)) + "\n";

        uiText.text += " UPad UD: " + "DPadUD" + " = " + getReal3D.Input.GetAxis("DPadUD") + "\n";
        uiText.text += " UPad UD: " + "DPadLR" + " = " + getReal3D.Input.GetAxis("DPadLR") + "\n";

        uiText.text += " Button 1: " + CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button1) 
            + " = " + getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button1)) + "\n";

        uiText.text += " Button 2: " + CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button2)
            + " = " + getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button2)) + "\n";

        uiText.text += " Button 3: " + CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button3)
    + " = " + getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button3)) + "\n";

        uiText.text += " Button 4: " + CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button4)
    + " = " + getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button4)) + "\n";

        uiText.text += " Button 5: " + CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button5)
    + " = " + getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button5)) + "\n";

        uiText.text += " Button 6: " + CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button6)
    + " = " + getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button6)) + "\n";

        uiText.text += " Button 7: " + CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button7)
    + " = " + getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button7)) + "\n";

        uiText.text += " Button 8: " + CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button8)
    + " = " + getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button8)) + "\n";

        uiText.text += " Button 9: " + CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button9)
    + " = " + getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button9)) + "\n";

        uiText.text += " Button SP1: " + CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton1)
+ " = " + getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton1)) + "\n";

        uiText.text += " Button SP2: " + CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton2)
    + " = " + getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton2)) + "\n";

        uiText.text += " Button SP3: " + CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton3)
    + " = " + getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton3)) + "\n";
    }
#endif
}
