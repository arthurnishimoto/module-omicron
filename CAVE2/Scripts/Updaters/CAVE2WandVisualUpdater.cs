using UnityEngine;
using System.Collections;
using omicronConnector;

public class CAVE2WandVisualUpdater : MonoBehaviour
{
    public int wandID = 1;

    public Material litMaterial;
    public Material unlitMaterial;

    public Transform buttonCross;
    public Transform buttonCircle;
    public Transform buttonUp;
    public Transform buttonDown;
    public Transform buttonLeft;
    public Transform buttonRight;

    public Transform buttonL1;
    public Transform buttonL2;
    public Transform buttonL3;

    public Vector2 leftAnalogStick;
    public Vector2 analogTriggers;

    // Gamepad
    public Transform buttonTriangle;
    public Transform buttonSquare;

    public Transform buttonR1;
    public Transform buttonR2;
    public Transform buttonR3;

    public Vector2 rightAnalogStick;

    public Transform buttonSelect;
    public Transform buttonStart;

    // Use this for initialization
    void Start()
    {
        // Wand (Navigation Controller)
        buttonCross = transform.FindChild("CrossButton");
        buttonCircle = transform.FindChild("CircleButton");
        buttonDown = transform.FindChild("ButtonDown");
        buttonLeft = transform.FindChild("ButtonLeft");
        buttonRight = transform.FindChild("ButtonRight");
        buttonUp = transform.FindChild("ButtonUp");
        buttonL1 = transform.FindChild("L1");
        buttonL2 = transform.FindChild("L2");
        buttonL3 = transform.FindChild("AnalogStick-L3");

        // Full Controller
        buttonTriangle = transform.FindChild("TriangleButton");
        buttonSquare = transform.FindChild("SquareButton");

        buttonR1 = transform.FindChild("R1");
        buttonR2 = transform.FindChild("R2");
        buttonR3 = transform.FindChild("AnalogStick-R3");

        buttonSelect = transform.FindChild("Select");
        buttonStart = transform.FindChild("Start");
    }

    // Update is called once per frame
    void Update()
    {
        leftAnalogStick = new Vector2(CAVE2.GetAxis(wandID, CAVE2.Axis.LeftAnalogStickLR), CAVE2.GetAxis(wandID, CAVE2.Axis.LeftAnalogStickUD));
        rightAnalogStick = new Vector2(CAVE2.GetAxis(wandID, CAVE2.Axis.RightAnalogStickLR), CAVE2.GetAxis(wandID, CAVE2.Axis.RightAnalogStickUD));
        analogTriggers = new Vector2(CAVE2.GetAxis(wandID, CAVE2.Axis.AnalogTriggerL), CAVE2.GetAxis(wandID, CAVE2.Axis.AnalogTriggerR));

        if (buttonL3)
            buttonL3.localEulerAngles = new Vector3(leftAnalogStick.y, 0, -leftAnalogStick.x) * 30;
        if (buttonR3)
            buttonR3.localEulerAngles = new Vector3(rightAnalogStick.y, 0, -rightAnalogStick.x) * 30;

        if (buttonL2)
            buttonL2.localEulerAngles = new Vector3(0, 90, analogTriggers.x * 20);
        if (buttonR2)
            buttonR2.localEulerAngles = new Vector3(0, 90, analogTriggers.y * 20);

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
        SetLit(buttonL2, CAVE2.GetButtonState(wandID, CAVE2.Button.Button7));
        SetLit(buttonL3, CAVE2.GetButtonState(wandID, CAVE2.Button.Button6));

        SetLit(buttonR1, CAVE2.GetButtonState(wandID, CAVE2.Button.Button8));
        SetLit(buttonR2, CAVE2.GetButtonState(wandID, CAVE2.Button.SpecialButton3));
        SetLit(buttonR3, CAVE2.GetButtonState(wandID, CAVE2.Button.Button9));

        SetLit(buttonSelect, CAVE2.GetButtonState(wandID, CAVE2.Button.SpecialButton1));
        SetLit(buttonStart, CAVE2.GetButtonState(wandID, CAVE2.Button.SpecialButton2));

    }

    void SetLit(Transform g, OmicronController.ButtonState state)
    {
        if (g)
        {
            if (state == OmicronController.ButtonState.Held)
                g.GetComponent<Renderer>().material = litMaterial;
            else if (state == OmicronController.ButtonState.Idle)
                g.GetComponent<Renderer>().material = unlitMaterial;
        }
    }
}
