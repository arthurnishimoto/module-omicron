using UnityEngine;
using System.Collections;
using omicronConnector;

public class OmicronControllerVisualTester : MonoBehaviour
{
    public OmicronController controller;

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
        if(controller == null)
        {
            controller = GetComponent<OmicronController>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        leftAnalogStick = controller.GetAnalogStick(1);
		rightAnalogStick = controller.GetAnalogStick(2);
        analogTriggers = controller.GetAnalogStick(3);

        if ( buttonL3 )
			buttonL3.transform.localEulerAngles = new Vector3( -leftAnalogStick.y, 0, -leftAnalogStick.x ) * 30;
		if( buttonR3 )
			buttonR3.transform.localEulerAngles = new Vector3( rightAnalogStick.y, 0, -rightAnalogStick.x ) * 30;

		if( buttonL2 )
			buttonL2.transform.localEulerAngles = new Vector3( 0, 90, analogTriggers.x * 20 );
		if( buttonR2 )
			buttonR2.transform.localEulerAngles = new Vector3( 0, 90, analogTriggers.y * 20 );

        // Tests if hold state is working properly (public state varibles should change)
        // Tests if up/down is working (visual buttons should change)
        SetLit(buttonCross, controller.GetButtonState(CAVE2.Button.Button3));
        SetLit(buttonCircle, controller.GetButtonState(CAVE2.Button.Button2));
        SetLit(buttonTriangle, controller.GetButtonState(CAVE2.Button.Button1));
        SetLit(buttonSquare, controller.GetButtonState(CAVE2.Button.Button4));

        SetLit(buttonUp, controller.GetButtonState(CAVE2.Button.ButtonUp));
        SetLit(buttonDown, controller.GetButtonState(CAVE2.Button.ButtonDown));
        SetLit(buttonLeft, controller.GetButtonState(CAVE2.Button.ButtonLeft));
        SetLit(buttonRight, controller.GetButtonState(CAVE2.Button.ButtonRight));

        SetLit(buttonL1, controller.GetButtonState(CAVE2.Button.Button5));
        SetLit(buttonL2, controller.GetButtonState(CAVE2.Button.Button7));
        SetLit(buttonL3, controller.GetButtonState(CAVE2.Button.Button6));

        SetLit(buttonR1, controller.GetButtonState(CAVE2.Button.Button8));
        SetLit(buttonR2, controller.GetButtonState(CAVE2.Button.SpecialButton3));
        SetLit(buttonR3, controller.GetButtonState(CAVE2.Button.Button9));

        SetLit(buttonSelect, controller.GetButtonState(CAVE2.Button.SpecialButton1));
        SetLit(buttonStart, controller.GetButtonState(CAVE2.Button.SpecialButton2));

    }

    void SetLit(GameObject g, OmicronController.ButtonState state)
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
