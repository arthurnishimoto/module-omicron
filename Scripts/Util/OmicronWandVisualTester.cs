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

	//public bool useRPC = false;
	
	public WandState.ButtonState cross;
	public WandState.ButtonState circle;
	public WandState.ButtonState up;
	public WandState.ButtonState down;
	public WandState.ButtonState left;
	public WandState.ButtonState right;

	// Use this for initialization
	new void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		// Tests if hold state is working properly (public state varibles should change)
		cross = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button3);
		circle = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.Button2);

		up = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.ButtonUp);
		down = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.ButtonDown);
		left = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.ButtonLeft);
		right = CAVE2Manager.GetButtonState(wandID, CAVE2Manager.Button.ButtonRight);

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
	}

	void SetButtonState( int buttonID, bool lit )
	{
		switch(buttonID)
		{
			case((int)CAVE2Manager.Button.Button2):
				if( lit )
					buttonCircle.GetComponent<Renderer>().material = litMaterial;
				else
					buttonCircle.GetComponent<Renderer>().material = unlitMaterial;
				break;
			case((int)CAVE2Manager.Button.Button3):
				if( lit )
					buttonCross.GetComponent<Renderer>().material = litMaterial;
				else
					buttonCross.GetComponent<Renderer>().material = unlitMaterial;
				break;
			case((int)CAVE2Manager.Button.ButtonUp):
				if( lit )
					buttonUp.GetComponent<Renderer>().material = litMaterial;
				else
					buttonUp.GetComponent<Renderer>().material = unlitMaterial;
				break;
			case((int)CAVE2Manager.Button.ButtonDown):
				if( lit )
					buttonDown.GetComponent<Renderer>().material = litMaterial;
				else
					buttonDown.GetComponent<Renderer>().material = unlitMaterial;
				break;
			case((int)CAVE2Manager.Button.ButtonLeft):
				if( lit )
					buttonLeft.GetComponent<Renderer>().material = litMaterial;
				else
					buttonLeft.GetComponent<Renderer>().material = unlitMaterial;
				break;
			case((int)CAVE2Manager.Button.ButtonRight):
				if( lit )
					buttonRight.GetComponent<Renderer>().material = litMaterial;
				else
					buttonRight.GetComponent<Renderer>().material = unlitMaterial;
				break;
		}
	}
}
