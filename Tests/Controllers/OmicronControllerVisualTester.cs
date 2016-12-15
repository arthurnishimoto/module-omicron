using UnityEngine;
using System.Collections;
using omicronConnector;

public class OmicronControllerVisualTester : MonoBehaviour
{
    public OmicronControllerManager controllerManager;

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

    void OnEvent(EventData e)
    {
        //Debug.Log("OmicronEventClient: '"+name+"' received " + e.serviceType);
    }

    // Update is called once per frame
    void Update()
    {
        leftAnalogStick = new Vector2(controllerManager.analogInput1.x, controllerManager.analogInput1.y);
		rightAnalogStick = new Vector2(controllerManager.analogInput2.x, controllerManager.analogInput2.y);
        analogTriggers = new Vector2(controllerManager.analogInput3.x, controllerManager.analogInput3.y);

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
        SetLit(buttonCross, controllerManager.Button3);
        SetLit(buttonCircle, controllerManager.Button2);
        SetLit(buttonTriangle, controllerManager.Button1);
        SetLit(buttonSquare, controllerManager.Button4);

        SetLit(buttonUp, controllerManager.ButtonUp);
        SetLit(buttonDown, controllerManager.ButtonDown);
        SetLit(buttonLeft, controllerManager.ButtonLeft);
        SetLit(buttonRight, controllerManager.ButtonRight);

        SetLit(buttonL1, controllerManager.Button5);
        SetLit(buttonL2, controllerManager.Button7);
        SetLit(buttonL3, controllerManager.Button6);

        SetLit(buttonR1, controllerManager.Button8);
        SetLit(buttonR2, controllerManager.SpecialButton3);
        SetLit(buttonR3, controllerManager.Button9);

        SetLit(buttonSelect, controllerManager.SpecialButton1);
        SetLit(buttonStart, controllerManager.SpecialButton2);

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
