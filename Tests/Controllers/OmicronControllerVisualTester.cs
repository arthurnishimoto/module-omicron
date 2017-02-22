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

    }

    // Update is called once per frame
    void Update()
    {
        leftAnalogStick = new Vector2(controller.analogInput1.x, controller.analogInput1.y);
		rightAnalogStick = new Vector2(controller.analogInput2.x, controller.analogInput2.y);
        analogTriggers = new Vector2(controller.analogInput3.x, controller.analogInput3.y);

        if( buttonL3 )
			buttonL3.transform.localEulerAngles = new Vector3( -leftAnalogStick.y, 0, -leftAnalogStick.x ) * 30;
		if( buttonR3 )
			buttonR3.transform.localEulerAngles = new Vector3( rightAnalogStick.y, 0, -rightAnalogStick.x ) * 30;

		if( buttonL2 )
			buttonL2.transform.localEulerAngles = new Vector3( 0, 90, analogTriggers.x * 20 );
		if( buttonR2 )
			buttonR2.transform.localEulerAngles = new Vector3( 0, 90, analogTriggers.y * 20 );

        // Tests if hold state is working properly (public state varibles should change)
        // Tests if up/down is working (visual buttons should change)
        SetLit(buttonCross, controller.Button3);
        SetLit(buttonCircle, controller.Button2);
        SetLit(buttonTriangle, controller.Button1);
        SetLit(buttonSquare, controller.Button4);

        SetLit(buttonUp, controller.ButtonUp);
        SetLit(buttonDown, controller.ButtonDown);
        SetLit(buttonLeft, controller.ButtonLeft);
        SetLit(buttonRight, controller.ButtonRight);

        SetLit(buttonL1, controller.Button5);
        SetLit(buttonL2, controller.Button7);
        SetLit(buttonL3, controller.Button6);

        SetLit(buttonR1, controller.Button8);
        SetLit(buttonR2, controller.SpecialButton3);
        SetLit(buttonR3, controller.Button9);

        SetLit(buttonSelect, controller.SpecialButton1);
        SetLit(buttonStart, controller.SpecialButton2);

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
