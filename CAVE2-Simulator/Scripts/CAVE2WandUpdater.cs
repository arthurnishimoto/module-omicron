using UnityEngine;
using System.Collections;
using omicronConnector;

public class CAVE2WandUpdater : MonoBehaviour
{
    public int wandID = 1;

    public Material litMaterial;
    public Material unlitMaterial;

    public GameObject buttonCross;
    public GameObject buttonCircle;
    public GameObject buttonUp;
    public GameObject buttonDown;
    public GameObject buttonLeft;
    public GameObject buttonRight;

    public GameObject buttonL1;
    public GameObject buttonL2;
    public GameObject buttonL3;

    public Vector2 leftAnalogStick;
    public Vector2 analogTriggers;

    // Gamepad
    public GameObject buttonTriangle;
    public GameObject buttonSquare;

    public GameObject buttonR1;
    public GameObject buttonR2;
    public GameObject buttonR3;

    public Vector2 rightAnalogStick;

    public GameObject buttonSelect;
    public GameObject buttonStart;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        leftAnalogStick = new Vector2(CAVE2.GetAxis(wandID, CAVE2.Axis.LeftAnalogStickLR), CAVE2.GetAxis(wandID, CAVE2.Axis.LeftAnalogStickUD));
        rightAnalogStick = new Vector2(CAVE2.GetAxis(wandID, CAVE2.Axis.RightAnalogStickLR), CAVE2.GetAxis(wandID, CAVE2.Axis.RightAnalogStickUD));
        analogTriggers = new Vector2(CAVE2.GetAxis(wandID, CAVE2.Axis.AnalogTriggerL), CAVE2.GetAxis(wandID, CAVE2.Axis.AnalogTriggerR));

        if (buttonL3)
            buttonL3.transform.localEulerAngles = new Vector3(-leftAnalogStick.y, 0, -leftAnalogStick.x) * 30;
        if (buttonR3)
            buttonR3.transform.localEulerAngles = new Vector3(rightAnalogStick.y, 0, -rightAnalogStick.x) * 30;

        if (buttonL2)
            buttonL2.transform.localEulerAngles = new Vector3(0, 90, analogTriggers.x * 20);
        if (buttonR2)
            buttonR2.transform.localEulerAngles = new Vector3(0, 90, analogTriggers.y * 20);

        // Tests if hold state is working properly (public state varibles should change)
        // Tests if up/down is working (visual buttons should change)
        SetLit(buttonCross, CAVE2.GetButtonState(wandID, CAVE2.Button.Button3));
        SetLit(buttonCircle, CAVE2.GetButtonState(wandID, CAVE2.Button.Button2));
        SetLit(buttonTriangle, CAVE2.GetButtonState(wandID, CAVE2.Button.Button1));
        SetLit(buttonSquare, CAVE2.GetButtonState(wandID, CAVE2.Button.Button4));

        SetLit(buttonUp, CAVE2.GetButtonState(wandID, CAVE2.Button.ButtonUp));
        SetLit(buttonDown, CAVE2.GetButtonState(wandID, CAVE2.Button.ButtonDown));
        SetLit(buttonLeft, CAVE2.GetButtonState(wandID, CAVE2.Button.ButtonLeft));
        SetLit(buttonRight, CAVE2.GetButtonState(wandID, CAVE2.Button.ButtonRight));

        SetLit(buttonL1, CAVE2.GetButtonState(wandID, CAVE2.Button.Button5));
        SetLit(buttonL2, CAVE2.GetButtonState(wandID, CAVE2.Button.Button6));
        SetLit(buttonL3, CAVE2.GetButtonState(wandID, CAVE2.Button.Button7));

        SetLit(buttonR1, CAVE2.GetButtonState(wandID, CAVE2.Button.Button8));
        SetLit(buttonR2, CAVE2.GetButtonState(wandID, CAVE2.Button.SpecialButton3));
        SetLit(buttonR3, CAVE2.GetButtonState(wandID, CAVE2.Button.Button9));

        SetLit(buttonSelect, CAVE2.GetButtonState(wandID, CAVE2.Button.SpecialButton1));
        SetLit(buttonStart, CAVE2.GetButtonState(wandID, CAVE2.Button.SpecialButton2));

    }

    void SetLit(GameObject g, OmicronControllerManager.ButtonState state)
    {
        if (g)
        {
            if (state == OmicronControllerManager.ButtonState.Held)
                g.GetComponent<Renderer>().material = litMaterial;
            else if (state == OmicronControllerManager.ButtonState.Idle)
                g.GetComponent<Renderer>().material = unlitMaterial;
        }
    }
}
