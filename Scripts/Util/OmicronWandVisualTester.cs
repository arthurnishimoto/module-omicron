using UnityEngine;
using System.Collections;

public class OmicronWandVisualTester : OmicronWandUpdater {
	
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
	
	public WandState.ButtonState cross;
	public WandState.ButtonState circle;
	public WandState.ButtonState up;
	public WandState.ButtonState down;
	public WandState.ButtonState left;
	public WandState.ButtonState right;

	public WandState.ButtonState L1;
	public WandState.ButtonState L2;
	public WandState.ButtonState L3;

	public Vector2 leftAnalogStick;
    public Vector2 analogTriggers;

    // Gamepad
    public GameObject buttonTriangle;
    public GameObject buttonSquare;

    public GameObject buttonR1;
    public GameObject buttonR2;
    public GameObject buttonR3;

    public WandState.ButtonState triangle;
    public WandState.ButtonState square;

    public WandState.ButtonState R1;
    public WandState.ButtonState R2;
    public WandState.ButtonState R3;

	public Vector2 rightAnalogStick;

	// Use this for initialization
	new void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		leftAnalogStick = new Vector2(CAVE2Manager.GetAxis(wandID, CAVE2Manager.Axis.LeftAnalogStickLR), CAVE2Manager.GetAxis(wandID, CAVE2Manager.Axis.LeftAnalogStickUD) );
		rightAnalogStick = new Vector2(CAVE2Manager.GetAxis(wandID, CAVE2Manager.Axis.RightAnalogStickLR), CAVE2Manager.GetAxis(wandID, CAVE2Manager.Axis.RightAnalogStickUD) );
        analogTriggers = new Vector2(CAVE2Manager.GetAxis(wandID, CAVE2Manager.Axis.AnalogTriggerL), CAVE2Manager.GetAxis(wandID, CAVE2Manager.Axis.AnalogTriggerR));

		if( buttonL3 )
			buttonL3.transform.localEulerAngles = new Vector3( leftAnalogStick.y, 0, -leftAnalogStick.x ) * 30;
		if( buttonR3 )
			buttonR3.transform.localEulerAngles = new Vector3( rightAnalogStick.y, 0, -rightAnalogStick.x ) * 30;

		if( buttonL2 )
			buttonL2.transform.localEulerAngles = new Vector3( 0, 90, analogTriggers.x * 20 );
		if( buttonR2 )
			buttonR2.transform.localEulerAngles = new Vector3( 0, 90, analogTriggers.y * 20 );

		// Tests if hold state is working properly (public state varibles should change)
		cross = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button3);
		circle = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button2);
        triangle = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button1);
        square = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button4);

		up = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.ButtonUp);
		down = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.ButtonDown);
		left = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.ButtonLeft);
		right = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.ButtonRight);

		L1 = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button5);
		L2 = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button7);
		L3 = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button6);

        R1 = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button8);
        //R2 = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button9);
        R3 = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button9);

		// Tests if up/down is working (visual buttons should change)
        CheckButtonState((int)CAVE2Manager.Button.Button3, cross);
        CheckButtonState((int)CAVE2Manager.Button.Button2, circle);
        CheckButtonState((int)CAVE2Manager.Button.Button1, triangle);
        CheckButtonState((int)CAVE2Manager.Button.Button4, square);

        CheckButtonState((int)CAVE2Manager.Button.ButtonUp, up);
        CheckButtonState((int)CAVE2Manager.Button.ButtonDown, down);
        CheckButtonState((int)CAVE2Manager.Button.ButtonLeft, left);
        CheckButtonState((int)CAVE2Manager.Button.ButtonRight, right);

        CheckButtonState((int)CAVE2Manager.Button.Button5, L1);
        CheckButtonState((int)CAVE2Manager.Button.Button7, L2);
        CheckButtonState((int)CAVE2Manager.Button.Button6, L3);

        CheckButtonState((int)CAVE2Manager.Button.Button8, R1);
        //CheckButtonState((int)CAVE2Manager.Button.Button9, R2);
        CheckButtonState((int)CAVE2Manager.Button.Button9, R3);
	}

    void CheckButtonState(int buttonID, WandState.ButtonState currentState)
    {
        if (currentState == WandState.ButtonState.Down)
        {
            SetButtonState(buttonID, true);
        }
        else if (currentState == WandState.ButtonState.Up)
        {
            SetButtonState(buttonID, false);
        }
    }

	void SetButtonState( int buttonID, bool lit )
	{
		switch(buttonID)
		{
			case((int)CAVE2Manager.Button.Button2):
				SetLit(buttonCircle, lit);
			    break;
			case((int)CAVE2Manager.Button.Button3):
				SetLit(buttonCross, lit);
				break;
			case((int)CAVE2Manager.Button.ButtonUp):
				SetLit(buttonUp, lit);
				break;
			case((int)CAVE2Manager.Button.ButtonDown):
				SetLit(buttonDown, lit);
				break;
			case((int)CAVE2Manager.Button.ButtonLeft):
				SetLit(buttonLeft, lit);
				break;
			case((int)CAVE2Manager.Button.ButtonRight):
				SetLit(buttonRight, lit);
				break;
			case((int)CAVE2Manager.Button.Button5):
				SetLit(buttonL1, lit);
				break;
			case((int)CAVE2Manager.Button.Button7):
				SetLit(buttonL2, lit);
				break;
			case((int)CAVE2Manager.Button.Button6):
				SetLit(buttonL3, lit);
				break;
            case ((int)CAVE2Manager.Button.Button1):
                SetLit(buttonTriangle, lit);
                break;
            case ((int)CAVE2Manager.Button.Button4):
                SetLit(buttonSquare, lit);
                break;
            case ((int)CAVE2Manager.Button.Button8):
                SetLit(buttonR1, lit);
                break;
            case ((int)CAVE2Manager.Button.Button9):
                SetLit(buttonR3, lit);
                break;
		}
	}

	void SetLit(GameObject g, bool lit)
	{
        if (g)
        {
            if (lit)
                g.GetComponent<Renderer>().material = litMaterial;
            else
                g.GetComponent<Renderer>().material = unlitMaterial;
        }
	}
}
