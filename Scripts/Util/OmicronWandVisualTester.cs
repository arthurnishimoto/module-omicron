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

	//public bool useRPC = false;
	
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
	public Vector2 rightAnalogStick;

	// Use this for initialization
	new void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		leftAnalogStick = new Vector2(CAVE2Manager.GetAxis(wandID, CAVE2Manager.Axis.LeftAnalogStickLR), CAVE2Manager.GetAxis(wandID, CAVE2Manager.Axis.LeftAnalogStickUD) );
		rightAnalogStick = new Vector2(CAVE2Manager.GetAxis(wandID, CAVE2Manager.Axis.RightAnalogStickLR), CAVE2Manager.GetAxis(wandID, CAVE2Manager.Axis.RightAnalogStickUD) );

		// Tests if hold state is working properly (public state varibles should change)
		cross = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button3);
		circle = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button2);

		up = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.ButtonUp);
		down = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.ButtonDown);
		left = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.ButtonLeft);
		right = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.ButtonRight);

		L1 = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button5);
		L2 = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button7);
		L3 = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button6);

		// Tests if up/down is working (visual buttons should change)
		if( cross == WandState.ButtonState.Down )
			SetButtonState((int)CAVE2Manager.Button.Button3, true);
		else if( cross == WandState.ButtonState.Up )
			SetButtonState((int)CAVE2Manager.Button.Button3, false);
		if( circle == WandState.ButtonState.Down )
			SetButtonState((int)CAVE2Manager.Button.Button2, true);
		else if( circle == WandState.ButtonState.Up )
			SetButtonState((int)CAVE2Manager.Button.Button2, false);

		if( up == WandState.ButtonState.Down )
			SetButtonState((int)CAVE2Manager.Button.ButtonUp, true);
		else if( up == WandState.ButtonState.Up )
			SetButtonState((int)CAVE2Manager.Button.ButtonUp, false);
		if( down == WandState.ButtonState.Down )
			SetButtonState((int)CAVE2Manager.Button.ButtonDown, true);
		else if( down == WandState.ButtonState.Up )
			SetButtonState((int)CAVE2Manager.Button.ButtonDown, false);
		if( left == WandState.ButtonState.Down )
			SetButtonState((int)CAVE2Manager.Button.ButtonLeft, true);
		else if( left == WandState.ButtonState.Up )
			SetButtonState((int)CAVE2Manager.Button.ButtonLeft, false);
		if( right == WandState.ButtonState.Down )
			SetButtonState((int)CAVE2Manager.Button.ButtonRight, true);
		else if( right == WandState.ButtonState.Up )
			SetButtonState((int)CAVE2Manager.Button.ButtonRight, false);

		if( L1 == WandState.ButtonState.Down )
			SetButtonState((int)CAVE2Manager.Button.Button5, true);
		else if( L1 == WandState.ButtonState.Up )
			SetButtonState((int)CAVE2Manager.Button.Button5, false);
		if( L2 == WandState.ButtonState.Down )
			SetButtonState((int)CAVE2Manager.Button.Button7, true);
		else if( L2 == WandState.ButtonState.Up )
			SetButtonState((int)CAVE2Manager.Button.Button7, false);
		if( L3 == WandState.ButtonState.Down )
			SetButtonState((int)CAVE2Manager.Button.Button6, true);
		else if( L3 == WandState.ButtonState.Up )
			SetButtonState((int)CAVE2Manager.Button.Button6, false);
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
		}
	}

	void SetLit(GameObject g, bool lit)
	{
		if( lit )
			g.GetComponent<Renderer>().material = litMaterial;
		else
			g.GetComponent<Renderer>().material = unlitMaterial;
	}
}
