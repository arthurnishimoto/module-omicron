/**************************************************************************************************
* THE OMICRON PROJECT
*-------------------------------------------------------------------------------------------------
* Copyright 2010-2016             Electronic Visualization Laboratory, University of Illinois at Chicago
* Authors:                                                                                
* Arthur Nishimoto                anishimoto42@gmail.com
*-------------------------------------------------------------------------------------------------
* Copyright (c) 2010-2016, Electronic Visualization Laboratory, University of Illinois at Chicago
* All rights reserved.
* Redistribution and use in source and binary forms, with or without modification, are permitted
* provided that the following conditions are met:
*
* Redistributions of source code must retain the above copyright notice, this list of conditions
* and the following disclaimer. Redistributions in binary form must reproduce the above copyright
* notice, this list of conditions and the following disclaimer in the documentation and/or other
* materials provided with the distribution.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
* IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
* FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
* CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
* DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF
* USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
* WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
* ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*************************************************************************************************/
using UnityEngine;
using System.Collections;
using omicron;
using omicronConnector;

public class MocapState
{
	public int sourceID;
	public Vector3 position;
	public Quaternion rotation;
	
	public MocapState(int ID)
	{
		sourceID = ID;
		position = new Vector3(0,0,0);
		rotation = new Quaternion();
	}
	
	public void Update( Vector3 position, Quaternion orientation )
	{
		this.position = position;
		this.rotation = orientation;
	}
	
	public Vector3 GetPosition()
	{
		return position;
	}
	
	public Quaternion GetRotation()
	{
		return rotation;
	}
}

public class WandEvent
{
	public int wandID;
	public CAVE2Manager.Button button;

	public WandEvent(int ID, CAVE2Manager.Button button)
	{
		wandID = ID;
		this.button = button;
	}
}

public class CAVE2Manager : OmicronEventClient {
    static MocapState nullMocapState;
    static WandState nullWandState;

    public bool simulatorMode = false;
    public bool kinectSimulatorMode = false;

    public Hashtable wandStates;
    public Hashtable mocapStates;

    public static string ERROR_MANAGERNOTFOUND = "CAVE2-Manager GameObject expected, but not found in the current scene. Creating Default.";
    bool ERROR_NOCAMERACONTROLLERFOUND_TRIGGERED;

	public enum Axis { None, LeftAnalogStickLR, LeftAnalogStickUD, RightAnalogStickLR, RightAnalogStickUD, AnalogTriggerL, AnalogTriggerR,
		LeftAnalogStickLR_Inverted, LeftAnalogStickUD_Inverted, RightAnalogStickLR_Inverted, RightAnalogStickUD_Inverted, AnalogTriggerL_Inverted, AnalogTriggerR_Inverted
	};
	public enum Button { Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9, SpecialButton1, SpecialButton2, SpecialButton3, ButtonUp, ButtonDown, ButtonLeft, ButtonRight, None };
	
	// Note these represent Omicron sourceIDs
	public int Head1MocapID = 0; 
	public int Wand1MocapID = 1; // 1 = Batman/Kirk (TrackD = 3)
    public int Wand2MocapID = 2; // 2 = Robin/Spock (TrackD = 4)
    public int Wand3MocapID = 3; // 3 = Xbox (TrackD = 2)
    public int Wand4MocapID = 5; // 5 = Batgirl/Scotty (TrackD = 6)

    public float axisSensitivity = 1f;
	public float axisDeadzone = 0.2f;

    // Distance in meters the wand marker center is offset from the controller center
    public Vector3 wand1TrackingOffset = new Vector3(-0.007781088f, -0.04959464f, -0.07368752f);

	public bool keyboardEventEmulation = false;
	public bool wandMousePointerEmulation = false;
	public bool mocapEmulation = false;
	public bool lockWandToHeadTransform = false;
	public bool mouseHeadLook = false;
	public static bool mouseUsingGUI = false;

	public Vector3 headEmulatedPosition = new Vector3(0, 1.5f, 0);
	public Vector3 headEmulatedRotation = new Vector3(0, 0, 0);

	public Vector3 wandEmulatedPosition = new Vector3(0.175f, 1.2f, 0.6f);
	public Vector3 wandEmulatedRotation = new Vector3(0, 0, 0);

	public enum TrackerEmulated { CAVE, Head, Wand };
	public enum TrackerEmulationMode { Pointer, Translate, Rotate, TranslateForward, TranslateVertical, RotatePitchYaw, RotateRoll };
	string[] trackerEmuStrings = {"CAVE", "Head", "Wand1"};
	string[] trackerEmuModeStrings = {"Translate", "Rotate" };

    public TrackerEmulationMode defaultWandEmulationMode = TrackerEmulationMode.TranslateVertical;
    public TrackerEmulationMode toggleWandEmulationMode = TrackerEmulationMode.Pointer;
    public TrackerEmulationMode wandEmulationMode = TrackerEmulationMode.Pointer;
    public KeyCode toggleWandModeKey = KeyCode.Tab;
    public bool wandModeToggled = false;

    Vector3 mouseLastPos;
    public Vector3 mouseDeltaPos;

	TrackerEmulated WASDkeys = TrackerEmulated.CAVE;
	TrackerEmulationMode WASDkeyMode = TrackerEmulationMode.Translate;

	public TrackerEmulated IJKLkeys = TrackerEmulated.Head;
	public TrackerEmulationMode IJKLkeyMode = TrackerEmulationMode.Translate;
	
	public float emulatedTranslateSpeed = 0.05f;
	public float emulatedRotationSpeed = 0.05f;

	public int framerateCap = 60;
	public static string machineName;

	Vector3 lastMousePosition = Vector3.zero;

    private ArrayList playerControllers;
    public GameObject cameraController;

    // CAVE2 Simulator to Wand button bindings
    public string wandSimulatorAnalogUD = "Vertical";
    public string wandSimulatorAnalogLR = "Horizontal";
    public string wandSimulatorButton3 = "Fire1"; // PS3 Navigation Cross
    public string wandSimulatorButton2 = "Fire2"; // PS3 Navigation Circle
    public KeyCode wandSimulatorDPadUp = KeyCode.UpArrow;
    public KeyCode wandSimulatorDPadDown = KeyCode.DownArrow;
    public KeyCode wandSimulatorDPadLeft = KeyCode.LeftArrow;
    public KeyCode wandSimulatorDPadRight = KeyCode.RightArrow;
    public KeyCode wandSimulatorButton5 = KeyCode.Space; // PS3 Navigation L1
    public string wandSimulatorButton6 = "Fire3"; // PS3 Navigation L3
    public KeyCode wandSimulatorButton7 = KeyCode.LeftShift; // PS3 Navigation L2

    static CAVE2Manager cave2ManagerInstance;

	// Use this for initialization
	new void Start () {
		base.Start();

        Random.seed = 1138;

        mocapStates = new Hashtable();
        wandStates = new Hashtable();

        nullMocapState = new MocapState(-1);
        nullWandState = new WandState(-1,-1);

        // Default head state
        MocapState head1 = new MocapState(Head1MocapID);
        head1.Update(new Vector3(0, 1.6f, 0), Quaternion.identity);
        mocapStates.Add(Head1MocapID, head1);

        // Default wand state
        wandStates.Add(Wand1MocapID, new WandState(1, Wand1MocapID));

		machineName = System.Environment.MachineName;

		if ((OnCAVE2Master() && Application.platform != RuntimePlatform.WindowsEditor) || OnCAVE2Display())
        {
			#if USING_GETREAL3D
			if( Camera.main.GetComponent<getRealCameraUpdater>() )
			{
            	Camera.main.GetComponent<getRealCameraUpdater>().applyHeadPosition = true;
            	Camera.main.GetComponent<getRealCameraUpdater>().applyHeadRotation = true;
            	Camera.main.GetComponent<getRealCameraUpdater>().applyCameraProjection = true;
			}
			else
			{
				Camera.main.gameObject.AddComponent<getRealCameraUpdater>();
			}
			#endif

			keyboardEventEmulation = false;
			wandMousePointerEmulation = false;
			mocapEmulation = false;
			lockWandToHeadTransform = false;

			simulatorMode = false;
        }
		else if( Application.platform == RuntimePlatform.WindowsEditor )
		{
            #if USING_GETREAL3D
            if (!simulatorMode && Camera.main.GetComponent<getRealCameraUpdater>())
            {
                Camera.main.GetComponent<getRealCameraUpdater>().applyHeadPosition = true;
                Camera.main.GetComponent<getRealCameraUpdater>().applyHeadRotation = true;
                Camera.main.GetComponent<getRealCameraUpdater>().applyCameraProjection = true;
            }
            else if(!simulatorMode)
            {
                Camera.main.gameObject.AddComponent<getRealCameraUpdater>();
            }
            #endif
        }
    }

    void Awake()
    {
        cave2ManagerInstance = this;
    }

    public static CAVE2Manager GetCAVE2Manager()
    {
        if (cave2ManagerInstance != null)
        {
            return cave2ManagerInstance;
        }
        else
        {
            OmicronManager omgManager = OmicronManager.GetOmicronManager();
            CAVE2Manager cave2Manager = omgManager.gameObject.AddComponent<CAVE2Manager>();
            cave2Manager.simulatorMode = true;
            return cave2Manager;
        }
    }

	public static bool UsingOmicronServer()
	{
        if (GetCAVE2Manager())
		{
            return OmicronManager.GetOmicronManager().connectToServer;
		}
        else
        {
            return false;
        }
	}

	public static bool UsingGetReal3D()
	{
#if USING_GETREAL3D
		return true;
#else
		return false;
#endif
	}

    public void AddPlayerController(GameObject c)
    {
        if (playerControllers != null)
            playerControllers.Add(c);
        else
        {
            // First run case since client may attempt to connect before
            // OmicronManager Start() is called
            playerControllers = new ArrayList();
            playerControllers.Add(c);
        }
    }

    public GameObject GetPlayerControllerByIndex(int value)
    {
		if (playerControllers != null && playerControllers.Count > value)
		{
        	return playerControllers[value] as GameObject;
		}
		else if (playerControllers == null)
		{
			playerControllers = new ArrayList();
		}
		return null;
    }

    public static GameObject GetPlayer()
    {
        return GetCAVE2Manager().GetPlayerControllerByIndex(0);
    }

    public void AddCameraController(GameObject c)
    {
        cameraController = c;
    }

    public GameObject GetCameraController()
    {
        return cameraController;
    }

    public static GameObject GetMainCameraController()
    {
        return GetCAVE2Manager().GetCameraController();
    }

	public static bool IsMaster()
	{
		#if USING_GETREAL3D
		return getReal3D.Cluster.isMaster;
		#else
        if (System.Environment.MachineName.Contains("LYRA") && !System.Environment.MachineName.Equals("LYRA-WIN"))
			return false;
		else // Assumes on LYRA-WIN or development machine
			return true;
		#endif
	}

    public static bool IsSimulatorMode()
    {
        if (GetCAVE2Manager())
        {
            CAVE2Manager manager = GameObject.Find("CAVE2-Manager").GetComponent<CAVE2Manager>();
            return (manager.simulatorMode || manager.kinectSimulatorMode);
        }
        else
        {
            return false;
        }
    }

	public static bool OnCAVE2Master()
	{
		machineName = System.Environment.MachineName;
		if( machineName.Equals("LYRA-WIN") )
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public static bool OnCAVE2Display()
	{
		machineName = System.Environment.MachineName;
		if( machineName.Contains("LYRA") && !IsMaster() )
		{
			return true;
		}
		else
		{
			return false;
		}
	}

    // Primary function for getting mocap states
    public static MocapState GetMocapState(int ID)
    {
        CAVE2Manager cave2Manager = CAVE2Manager.GetCAVE2Manager();
        if (cave2Manager.mocapStates.ContainsKey(ID))
        {
            return (MocapState)cave2Manager.mocapStates[ID];
        }
        else
        {
            Debug.LogWarning("CAVE2Manager: GetMocapState ID: " + ID + " does not exist.");
            return nullMocapState;
        }
    }

    // Primary function for getting wand states
    public WandState GetWand(int ID)
    {
        CAVE2Manager cave2Manager = CAVE2Manager.GetCAVE2Manager();

        // Wand states are internally stored by MocapID
        switch (ID)
        {
            case (1): ID = Wand1MocapID; break;
            case (2): ID = Wand2MocapID; break;
            case (3): ID = Wand3MocapID; break;
            case (4): ID = Wand4MocapID; break;
            default: ID = Wand1MocapID; break;
        }

        if (cave2Manager.wandStates.ContainsKey(ID))
        {
            return (WandState)cave2Manager.wandStates[ID];
        }
        else
        {
            Debug.LogWarning("CAVE2Manager: GetWand ID: " + ID + " does not exist.");
            return nullWandState;
        }
    }

    // Convience functions for getting head/wand data
    public static MocapState GetHead(int ID)
    {
        CAVE2Manager c2m = CAVE2Manager.GetCAVE2Manager().GetComponent<CAVE2Manager>();

        if (ID == 1)
        {
            ID = c2m.Head1MocapID;

        }

        return GetMocapState(ID);
    }

    public static Vector3 GetWandPosition(int wandID)
	{
        return CAVE2Manager.GetCAVE2Manager().GetWand(wandID).GetPosition();
    }

	public static Quaternion GetWandRotation(int wandID)
	{
        return CAVE2Manager.GetCAVE2Manager().GetWand(wandID).GetRotation();
    }

	public static float GetAxis(int wandID, CAVE2Manager.Axis axis)
	{
        WandState wandState = CAVE2Manager.GetCAVE2Manager().GetWand(wandID);
        return wandState.GetAxis(axis);
	}

	public static bool GetButton(int wandID, CAVE2Manager.Button button)
	{
        WandState wandState = CAVE2Manager.GetCAVE2Manager().GetWand(wandID);
        return wandState.GetButton(button);
	}

	public static bool GetButtonDown(int wandID, CAVE2Manager.Button button)
	{
        WandState wandState = CAVE2Manager.GetCAVE2Manager().GetWand(wandID);
        return wandState.GetButtonDown(button);
	}

	public static bool GetButtonUp(int wandID, CAVE2Manager.Button button)
	{
        WandState wandState = CAVE2Manager.GetCAVE2Manager().GetWand(wandID);
        return wandState.GetButtonUp(button);
	}

	public static WandState.ButtonState GetButtonState(int wandID, CAVE2Manager.Button button)
	{
        return CAVE2Manager.GetCAVE2Manager().GetWand(wandID).GetButtonState((int)button);
	}

    public static Vector3 GetWandTrackingOffset(int wandID)
    {
        if (wandID == 1)
        {
            return GetCAVE2Manager().wand1TrackingOffset;
        }
        else if (wandID == 2)
        {
            
        }

        return Vector3.zero;
    }

    public float lookHorizontal;
    public float lookVertical;

	// Update is called once per frame
	void Update () {
        Application.targetFrameRate = framerateCap;
        if (simulatorMode)
        {
#if USING_GETREAL3D
			if( Camera.main != null && Camera.main.GetComponent<getRealCameraUpdater>() )
			{
            	Camera.main.GetComponent<getRealCameraUpdater>().applyHeadPosition = false;
            	Camera.main.GetComponent<getRealCameraUpdater>().applyHeadRotation = false;
           		Camera.main.GetComponent<getRealCameraUpdater>().applyCameraProjection = false;
			}
#endif

            keyboardEventEmulation = true;
            wandMousePointerEmulation = true;
            mocapEmulation = true;

            if (Input.GetKeyDown(toggleWandModeKey))
            {
                wandModeToggled = !wandModeToggled;
            }

            if(wandModeToggled)
            {
                wandEmulationMode = toggleWandEmulationMode;
            }
            else
            {
                wandEmulationMode = defaultWandEmulationMode;
            }
            mouseDeltaPos = Input.mousePosition - mouseLastPos;
            mouseLastPos = Input.mousePosition;
        }

        foreach (DictionaryEntry pair in wandStates)
        {
            WandState curWandState = (WandState)wandStates[pair.Key];
            curWandState.UpdateState((int)pair.Key, 0);
        }

        float vertical = Input.GetAxis(wandSimulatorAnalogUD) * axisSensitivity;
        float horizontal = Input.GetAxis(wandSimulatorAnalogLR) * axisSensitivity;
		float forward = 0 * axisSensitivity;

        // Horizontal2, Vertical 2 are not a standard Input axis
        // catch exception to prevent console spamming if not set
        try
        {
            lookHorizontal = Input.GetAxis("Horizontal2") * axisSensitivity;
            lookVertical = Input.GetAxis("Vertical2") * axisSensitivity;
        }
        catch
        {
        }

		uint flags = 0;
		
		#if USING_GETREAL3D
		// If using Omicron, make sure button events don't conflict
        if (!simulatorMode)
		{
			vertical = getReal3D.Input.GetAxis("Forward") * axisSensitivity;
			horizontal = getReal3D.Input.GetAxis("Yaw") * axisSensitivity;
			lookHorizontal = getReal3D.Input.GetAxis("Strafe") * axisSensitivity;
			lookVertical = getReal3D.Input.GetAxis("Pitch") * axisSensitivity;

			float DPadUD = getReal3D.Input.GetAxis("DPadUD");
			float DPadLR = getReal3D.Input.GetAxis("DPadLR");
			if( DPadUD > 0 )
				flags += (int)EventBase.Flags.ButtonUp;
			else if( DPadUD < 0 )
				flags += (int)EventBase.Flags.ButtonDown;
			if( DPadLR > 0 )
				flags += (int)EventBase.Flags.ButtonLeft;
			else if( DPadLR < 0 )
				flags += (int)EventBase.Flags.ButtonRight;

			// Wand Button 1 (Triangle/Y)
			if( getReal3D.Input.GetButton("Jump") )
				flags += (int)EventBase.Flags.Button1;
			// F -> Wand Button 2 (Circle/B)
			if( getReal3D.Input.GetButton("ChangeWand") )
				flags += (int)EventBase.Flags.Button2;
			// R -> Wand Button 3 (Cross/A)
			if( getReal3D.Input.GetButton("WandButton") )
				flags += (int)EventBase.Flags.Button3;
			// Wand Button 4 (Square/X)
			if( getReal3D.Input.GetButton("NavSpeed") )
				flags += (int)EventBase.Flags.Button4;
			// Wand Button 8 (R1/RB)
			if( getReal3D.Input.GetButton("Reset") )
				flags += (int)EventBase.Flags.Button8;
			// Wand Button 5 (L1/LB)
			if( getReal3D.Input.GetButton("WandLook") )
				flags += (int)EventBase.Flags.Button5;
			// Wand Button 6 (L3)
			if( getReal3D.Input.GetButton("L3") )
				flags += (int)EventBase.Flags.Button6;
			// Wand Button 7 (L2)
			if( getReal3D.Input.GetButton("WandDrive") )
				flags += (int)EventBase.Flags.Button7;
			// Wand Button 8 (R2)
			if( getReal3D.Input.GetButton("RT") )
				flags += (int)EventBase.Flags.Button8;
			// Wand Button 9 (R3)
			if( getReal3D.Input.GetButton("R3") )
				flags += (int)EventBase.Flags.Button9;
			// Wand Button SP1 (Back)
			if( getReal3D.Input.GetButton("Back") )
				flags += (int)EventBase.Flags.SpecialButton1;
			// Wand Button SP2 (Start)
			if( getReal3D.Input.GetButton("Start") )
				flags += (int)EventBase.Flags.SpecialButton2;
		}
		#endif

		Vector2 wandAnalog = new Vector2(horizontal,vertical);
		Vector2 wandAnalog2 = new Vector2(lookHorizontal,lookVertical);
		Vector2 wandAnalog3 = new Vector2(forward,0);

		if( mouseHeadLook)
		{
			Vector3 mouseDiff = Input.mousePosition - lastMousePosition;
			if( !mouseUsingGUI )
				headEmulatedRotation += new Vector3(-mouseDiff.y, mouseDiff.x, 0) * emulatedRotationSpeed;
			lastMousePosition = Input.mousePosition;
			mouseUsingGUI = false;
		}
		if( keyboardEventEmulation )
		{
			if( WASDkeys == TrackerEmulated.CAVE )
			{
				if( WASDkeyMode == TrackerEmulationMode.Translate )
				{
					wandAnalog = new Vector2(horizontal,vertical);
					wandAnalog2 = new Vector2(lookHorizontal,lookVertical);
					wandAnalog3 = new Vector2(forward,0);
				}
				else if( WASDkeyMode == TrackerEmulationMode.Rotate )
				{
					wandAnalog = new Vector2(0,vertical);
					wandAnalog2 = new Vector2(horizontal,lookVertical);
					wandAnalog3 = new Vector2(forward,0);
				}
			}
			else if( WASDkeys == TrackerEmulated.Head )
			{
				if( WASDkeyMode == TrackerEmulationMode.Translate )
				{
					headEmulatedPosition += new Vector3( horizontal, 0, vertical ) * Time.deltaTime;
				}
				else if( WASDkeyMode == TrackerEmulationMode.Rotate )
					headEmulatedRotation += new Vector3( vertical, horizontal, 0 );
			}

			headEmulatedRotation += new Vector3( lookVertical, 0, 0 ) * emulatedRotationSpeed;

			// Arrow keys -> DPad
			if( Input.GetKey( wandSimulatorDPadUp ) )
				flags += (int)EventBase.Flags.ButtonUp;
            if (Input.GetKey(wandSimulatorDPadDown))
				flags += (int)EventBase.Flags.ButtonDown;
            if (Input.GetKey(wandSimulatorDPadLeft))
				flags += (int)EventBase.Flags.ButtonLeft;
            if (Input.GetKey(wandSimulatorDPadRight))
				flags += (int)EventBase.Flags.ButtonRight;
			
			// F -> Wand Button 2 (Circle)
            if (Input.GetKey(KeyCode.F) || Input.GetButton(wandSimulatorButton2))
				flags += (int)EventBase.Flags.Button2;
			// R -> Wand Button 3 (Cross)
            if (Input.GetKey(KeyCode.R) || Input.GetButton(wandSimulatorButton3))
				flags += (int)EventBase.Flags.Button3;

            // Wand (L2 Trigger)
            if (Input.GetKey(wandSimulatorButton7))
                flags += (int)EventBase.Flags.Button7;

            // Wand (L1 Trigger)
            if (Input.GetKey(wandSimulatorButton5))
                flags += (int)EventBase.Flags.Button5;

            // Wand (L3 Trigger)
            if (Input.GetButton(wandSimulatorButton6))
                flags += (int)EventBase.Flags.Button6;

			float headForward = 0;
			float headStrafe = 0;
			float headVertical = 0;
			
			float speed = emulatedTranslateSpeed;
			if( IJKLkeyMode == TrackerEmulationMode.Translate )
				speed = emulatedTranslateSpeed;
			else if( IJKLkeyMode == TrackerEmulationMode.Rotate )
				speed = emulatedRotationSpeed;

            if (Input.GetKey(KeyCode.I))
                headForward += speed * Time.deltaTime;
            else if (Input.GetKey(KeyCode.K))
                headForward -= speed * Time.deltaTime;
			if( Input.GetKey(KeyCode.J) )
                headStrafe -= speed * Time.deltaTime;
			else if( Input.GetKey(KeyCode.L) )
                headStrafe += speed * Time.deltaTime;
			if( Input.GetKey(KeyCode.U) )
                headVertical += speed * Time.deltaTime;
			else if( Input.GetKey(KeyCode.O) )
                headVertical -= speed * Time.deltaTime;

			if( IJKLkeys == TrackerEmulated.Head )
			{
				if( IJKLkeyMode == TrackerEmulationMode.Translate )
					headEmulatedPosition += new Vector3( headStrafe, headVertical, headForward );
				else if( IJKLkeyMode == TrackerEmulationMode.Rotate )
					headEmulatedRotation += new Vector3( headForward, headStrafe, headVertical );
			}
			else if( IJKLkeys == TrackerEmulated.Wand )
			{
				if( IJKLkeyMode == TrackerEmulationMode.Translate )
					wandEmulatedPosition += new Vector3( headStrafe, headVertical, headForward );
				else if( IJKLkeyMode == TrackerEmulationMode.Rotate )
					wandEmulatedRotation += new Vector3( headForward, headStrafe, headVertical );
			}


		}

		if( !CAVE2Manager.UsingOmicronServer() || CAVE2Manager.UsingGetReal3D() || (keyboardEventEmulation && Input.anyKey) )
		{
			GetWand(1).UpdateController( flags, wandAnalog, wandAnalog2, wandAnalog3 );
		}

		if( mocapEmulation )
		{
            Vector3 lookAround = new Vector3(-GetWand(1).GetAxis(Axis.RightAnalogStickUD), GetWand(1).GetAxis(Axis.RightAnalogStickLR), 0);
			headEmulatedRotation += lookAround * emulatedRotationSpeed;

			// Update emulated positions/rotations
            GetHead(1).Update(headEmulatedPosition, Quaternion.Euler(headEmulatedRotation));

            if (lockWandToHeadTransform)
                GetWand(1).UpdateMocap(headEmulatedPosition, Quaternion.Euler(headEmulatedRotation));
            else
            {
                GetWand(1).UpdateMocap(wandEmulatedPosition, Quaternion.Euler(wandEmulatedRotation));
            }
            if (cameraController != null)
            {
                Camera.main.transform.localPosition = headEmulatedPosition;
                Camera.main.transform.localEulerAngles = headEmulatedRotation;
                //cameraController.transform.localPosition = headEmulatedPosition;
                //cameraController.transform.localEulerAngles = headEmulatedRotation;
            }
			else if(!ERROR_NOCAMERACONTROLLERFOUND_TRIGGERED)
			{
				Debug.LogWarning("CAVE2Manager: No CameraController found. May not display properly in CAVE2! Make sure the parent GameObject of the Main Camera contains an OmicronCameraController script.");
                ERROR_NOCAMERACONTROLLERFOUND_TRIGGERED = true;

            }
		}
		else
		{
            #if USING_GETREAL3D
            GetHead(1).Update( getReal3D.Input.head.position, getReal3D.Input.head.rotation );
            GetWand(1).UpdateMocap( getReal3D.Input.wand.position, getReal3D.Input.wand.rotation );
            #endif
        }

        if( kinectSimulatorMode )
        {
            if (cameraController != null)
            {
                cameraController.transform.localPosition = GetHead(1).position;
                cameraController.transform.localEulerAngles = GetHead(1).rotation.eulerAngles;
            }
            else if(!ERROR_NOCAMERACONTROLLERFOUND_TRIGGERED)
            {
                Debug.LogWarning("CAVE2Manager: No CameraController found. May not display properly in CAVE2!");
                ERROR_NOCAMERACONTROLLERFOUND_TRIGGERED = true;
            }
        }
	}

	void OnEvent( EventData e )
	{
		//Debug.Log("CAVE2Manager: '"+name+"' received " + e.serviceType);
		if( e.serviceType == EventBase.ServiceType.ServiceTypeMocap )
		{
			// -zPos -xRot -yRot for Omicron->Unity coordinate conversion)
			Vector3 unityPos = new Vector3(e.posx, e.posy, -e.posz);
			Quaternion unityRot = new Quaternion(-e.orx, -e.ory, e.orz, e.orw);

			#if USING_GETREAL3D
			getReal3D.RpcManager.call ("UpdateMocapRPC", e.sourceId, unityPos, unityRot );
            #else
            UpdateMocap(e.sourceId, unityPos, unityRot);
            #endif

        }
        else if( e.serviceType == EventBase.ServiceType.ServiceTypeWand )
		{
			// -zPos -xRot -yRot for Omicron->Unity coordinate conversion)
			//Vector3 unityPos = new Vector3(e.posx, e.posy, -e.posz);
			//Quaternion unityRot = new Quaternion(-e.orx, -e.ory, e.orz, e.orw);
			
			// Flip Up/Down analog stick values
			Vector2 leftAnalogStick = new Vector2( e.getExtraDataFloat(0), -e.getExtraDataFloat(1) ) * axisSensitivity;
			Vector2 rightAnalogStick = new Vector2( e.getExtraDataFloat(2), e.getExtraDataFloat(3) ) * axisSensitivity;
			Vector2 analogTrigger = new Vector2( e.getExtraDataFloat(4), e.getExtraDataFloat(5) );
			
			if( Mathf.Abs(leftAnalogStick.x) < axisDeadzone )
				leftAnalogStick.x = 0;
			if( Mathf.Abs(leftAnalogStick.y) < axisDeadzone )
				leftAnalogStick.y = 0;
			if( Mathf.Abs(rightAnalogStick.x) < axisDeadzone )
				rightAnalogStick.x = 0;
			if( Mathf.Abs(rightAnalogStick.y) < axisDeadzone )
				rightAnalogStick.y = 0;

#if USING_GETREAL3D
			getReal3D.RpcManager.call ("UpdateControllerRPC", e.sourceId, e.flags, leftAnalogStick, rightAnalogStick, analogTrigger);
#else
            UpdateController(e.sourceId, e.flags, leftAnalogStick, rightAnalogStick, analogTrigger);
#endif
        }
    }

    void UpdateController(uint wandID, uint flags, Vector2 leftAnalogStick, Vector2 rightAnalogStick, Vector2 analogTrigger)
    {
        if (wandStates.ContainsKey((int)wandID))
        {
            ((WandState)wandStates[(int)wandID]).UpdateController(flags, leftAnalogStick, rightAnalogStick, analogTrigger);
        }
        else
        {
            int mocapID = (int)wandID;
            if ((int)wandID == 1)
            {
                mocapID = Wand1MocapID;
            }
            else if ((int)wandID == 2)
            {
                mocapID = Wand2MocapID;
            }
            else if ((int)wandID == 3)
            {
                mocapID = Wand3MocapID;
            }
            else if ((int)wandID == 4)
            {
                mocapID = Wand4MocapID;
            }
            WandState newWandState = new WandState((int)wandID, mocapID);
            newWandState.UpdateController(flags, leftAnalogStick, rightAnalogStick, analogTrigger);
            wandStates.Add((int)wandID, newWandState);
        }
    }

    void UpdateMocap(uint sourceId, Vector3 unityPos, Quaternion unityRot)
    {
        if (mocapStates.ContainsKey((int)sourceId))
        {
            ((MocapState)mocapStates[(int)sourceId]).Update(unityPos, unityRot);
        }
        else
        {
            MocapState newMocapState = new MocapState((int)sourceId);
            newMocapState.Update(unityPos, unityRot);
            mocapStates.Add((int)sourceId, newMocapState);
        }
        if (wandStates.ContainsKey((int)sourceId))
        {
            ((WandState)wandStates[(int)sourceId]).UpdateMocap(unityPos, unityRot);
        }
        else
        {
            int mocapID = (int)sourceId;
            if ((int)sourceId == 1)
            {
                mocapID = Wand1MocapID;
            }
            else if ((int)sourceId == 2)
            {
                mocapID = Wand2MocapID;
            }
            else if ((int)sourceId == 3)
            {
                mocapID = Wand3MocapID;
            }
            else if ((int)sourceId == 4)
            {
                mocapID = Wand4MocapID;
            }
            WandState newWandState = new WandState((int)sourceId, mocapID);
            newWandState.UpdateMocap(unityPos, unityRot);
            wandStates.Add((int)sourceId, newWandState);
        }
    }

#if USING_GETREAL3D
    [getReal3D.RPC]
	void UpdateControllerRPC( uint wandID, uint flags, Vector2 leftAnalogStick, Vector2 rightAnalogStick, Vector2 analogTrigger )
	{
        UpdateController(wandID, flags, leftAnalogStick, rightAnalogStick, analogTrigger);
    }

	[getReal3D.RPC]
	void UpdateMocapRPC( uint sourceId, Vector3 unityPos, Quaternion unityRot )
	{
        UpdateMocap(sourceId, unityPos, unityRot);

    }
	#endif

	public static void BroadcastMessage(string targetObjectName, string methodName, object param)
	{
#if USING_GETREAL3D
		if( getReal3D.Cluster.isMaster )
			getReal3D.RpcManager.call("SendCAVE2RPC", targetObjectName, methodName, param);
#else
        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            //Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
            targetObject.BroadcastMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
        }
#endif
	}

	public static void BroadcastMessage(string targetObjectName, string methodName)
	{
		BroadcastMessage(targetObjectName, methodName, 0);
	}

	public static void Destroy(string targetObjectName)
	{
		#if USING_GETREAL3D
		if( getReal3D.Cluster.isMaster )
			getReal3D.RpcManager.call("CAVE2DestroyRPC", targetObjectName);
		#else
        GameObject targetObject = GameObject.Find(targetObjectName);
        if (targetObject != null)
        {
            Destroy(targetObject);
        }
		#endif
	}

#if USING_GETREAL3D
	[getReal3D.RPC]
	public void SendCAVE2RPC(string targetObjectName, string methodName, object param)
	{
		//Debug.Log ("SendCAVE2RPC: call '" +methodName +"' on "+targetObjectName);

		GameObject targetObject = GameObject.Find(targetObjectName);
		if( targetObject != null )
		{
			//Debug.Log ("Broadcast '" +methodName +"' on "+targetObject.name);
			targetObject.BroadcastMessage(methodName, param, SendMessageOptions.DontRequireReceiver);
		}
	}

	[getReal3D.RPC]
	public void CAVE2DestroyRPC(string targetObjectName)
	{
		//Debug.Log ("SendCAVE2RPC: call 'Destroy' on "+targetObjectName);
		
		GameObject targetObject = GameObject.Find(targetObjectName);
		if( targetObject != null )
		{
			Destroy(targetObject);
		}
	}
#endif


	Vector2 GUIOffset;
	
	public void SetGUIOffSet( Vector2 offset )
	{
		GUIOffset = offset;
    }
	
    public void OnWindow(int windowID)
	{
		float rowHeight = 25;

		keyboardEventEmulation = GUI.Toggle (new Rect (GUIOffset.x + 20, GUIOffset.y + rowHeight * 0, 250, 20), keyboardEventEmulation, "Keyboard Event Emulation");
		wandMousePointerEmulation = GUI.Toggle (new Rect (GUIOffset.x + 20, GUIOffset.y + rowHeight * 1, 250, 20), wandMousePointerEmulation, "Wand-Mouse Pointer");
		mocapEmulation = GUI.Toggle (new Rect (GUIOffset.x + 20, GUIOffset.y + rowHeight * 2, 250, 20), mocapEmulation, "Mocap Emulation");
		lockWandToHeadTransform = GUI.Toggle (new Rect (GUIOffset.x + 20, GUIOffset.y + rowHeight * 3, 250, 20), lockWandToHeadTransform, "Lock Wand to Head Transform");

		GUI.Label(new Rect(GUIOffset.x + 20, GUIOffset.y + rowHeight * 4, 200, 20), "WASD Keys: ");
		WASDkeys = (TrackerEmulated)GUI.SelectionGrid(new Rect(GUIOffset.x + 100, GUIOffset.y + rowHeight * 4, 200, 20), (int)WASDkeys, trackerEmuStrings, 3);
		WASDkeyMode = (TrackerEmulationMode)GUI.SelectionGrid(new Rect(GUIOffset.x + 100, GUIOffset.y + rowHeight * 5, 200, 20), (int)WASDkeyMode, trackerEmuModeStrings, 3);

		GUI.Label(new Rect(GUIOffset.x + 20, GUIOffset.y + rowHeight * 6, 200, 20), "IJKL Keys: ");
		IJKLkeys = (TrackerEmulated)GUI.SelectionGrid(new Rect(GUIOffset.x + 100, GUIOffset.y + rowHeight * 6, 200, 20), (int)IJKLkeys, trackerEmuStrings, 3);
		IJKLkeyMode = (TrackerEmulationMode)GUI.SelectionGrid(new Rect(GUIOffset.x + 100, GUIOffset.y + rowHeight * 7, 200, 20), (int)IJKLkeyMode, trackerEmuModeStrings, 3);

    }
}
