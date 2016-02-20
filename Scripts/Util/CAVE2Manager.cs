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

public class HeadTrackerState
{
	public int sourceID;
	public Vector3 position;
	public Quaternion rotation;
	
	public HeadTrackerState(int ID)
	{
		sourceID = ID;
		position = new Vector3(0,1.6f,0);
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
	static HeadTrackerState head1;
	static HeadTrackerState head2;

    public bool simulatorMode = false;

	public static WandState wand1;
	public static WandState wand2;

    public static string ERROR_MANAGERNOTFOUND = "CAVE2-Manager GameObject expected, but not found in the current scene. Creating Default.";

	public enum Axis { None, LeftAnalogStickLR, LeftAnalogStickUD, RightAnalogStickLR, RightAnalogStickUD, AnalogTriggerL, AnalogTriggerR,
		LeftAnalogStickLR_Inverted, LeftAnalogStickUD_Inverted, RightAnalogStickLR_Inverted, RightAnalogStickUD_Inverted, AnalogTriggerL_Inverted, AnalogTriggerR_Inverted
	};
	public enum Button { Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9, SpecialButton1, SpecialButton2, SpecialButton3, ButtonUp, ButtonDown, ButtonLeft, ButtonRight, None };
	
	// Note these represent Omicron sourceIDs
	public int Head1 = 0; 
	public int Wand1 = 1; // Controller ID
	public int Wand1Mocap = 3; // 3 = Xbox
	
	public int Head2 = 4; // 4 = Head_Tracker2
	public int Wand2 = 2;
	public int Wand2Mocap = 5;

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

	// Use this for initialization
	new void Start () {
		base.Start();

		Random.seed = 1138;

		head1 = new HeadTrackerState(Head1);
		head2 = new HeadTrackerState(Head2);
		
		if (wand1 == null)
			wand1 = new WandState(Wand1, Wand1Mocap);
		else
		{
			wand1.sourceID = Wand1;
			wand1.mocapID = Wand1Mocap;
		}
		if( wand2 == null )
			wand2 = new WandState(Wand2, Wand2Mocap);
		else
		{
			wand2.sourceID = Wand2;
			wand2.mocapID = Wand2Mocap;
		}

		Application.targetFrameRate = framerateCap;
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
			if( Camera.main == null )
			{
				Debug.LogError("CAVE2Manager: No Camera tagged 'MainCamera' was found. Will not display properly in CAVE2!");
			}
			#if USING_GETREAL3D
			else if( !simulatorMode && !Camera.main.GetComponent<getRealCameraUpdater>() )
			{
				Camera.main.gameObject.AddComponent<getRealCameraUpdater>();
			}
			#endif
		}
	}

	public static bool UsingOmicronServer()
	{
        if (GetCAVE2Manager())
		{
			return GameObject.Find ("CAVE2-Manager").GetComponent<OmicronManager> ().connectToServer;
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

    public static GameObject GetCAVE2Manager()
    {
        if (GameObject.Find("CAVE2-Manager"))
        {
            return GameObject.Find("CAVE2-Manager");
        }
        else
        {
            GameObject cave2Manager = new GameObject("CAVE2-Manager");
            cave2Manager.AddComponent<CAVE2Manager>();
            cave2Manager.AddComponent<OmicronManager>();
            cave2Manager.AddComponent<DebugGUIManager>();
            cave2Manager.GetComponent<CAVE2Manager>().simulatorMode = true;
            Debug.LogWarning(CAVE2Manager.ERROR_MANAGERNOTFOUND);
            return cave2Manager;
        }
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

    public GameObject GetPlayerController(int value)
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

    public void AddCameraController(GameObject c)
    {
        cameraController = c;
    }

	public static bool IsMaster()
	{
		#if USING_GETREAL3D
		return getReal3D.Cluster.isMaster;
		#else
		if( machineName.Contains("LYRA") && !machineName.Equals("LYRA-WIN") )
			return false;
		else // Assumes on LYRA-WIN or development machine
			return true;
		#endif
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

	public static Vector3 GetHeadPosition(int ID)
	{
		if( ID == 1 )
		{
			return CAVE2Manager.head1.GetPosition();
		}
		else if( ID == 2 )
		{
			return CAVE2Manager.head2.GetPosition();
		}
		
		return Vector3.zero;
	}
	
	public static Quaternion GetHeadRotation(int ID)
	{
		if( ID == 1 )
		{
			return CAVE2Manager.head1.GetRotation();
		}
		else if( ID == 2 )
		{
			return CAVE2Manager.head2.GetRotation();
		}
		
		return Quaternion.identity;
	}

	public static Vector3 GetWandPosition(int wandID)
	{
		if( wandID == 1 )
		{
			return CAVE2Manager.wand1.GetPosition();
		}
		else if( wandID == 2 )
		{
			return CAVE2Manager.wand2.GetPosition();
		}
		
		return Vector3.zero;
	}

	public static Quaternion GetWandRotation(int wandID)
	{
		if( wandID == 1 )
		{
			return CAVE2Manager.wand1.GetRotation();
		}
		else if( wandID == 2 )
		{
			return CAVE2Manager.wand2.GetRotation();
		}
		
		return Quaternion.identity;
	}

	public static float GetAxis(int wandID, CAVE2Manager.Axis axis)
	{
		if( wandID == 1 )
		{
			return CAVE2Manager.wand1.GetAxis(axis);
		}
		else if( wandID == 2 )
		{
			return CAVE2Manager.wand2.GetAxis(axis);
		}
		
		return 0;
	}

	public static bool GetButton(int wandID, CAVE2Manager.Button button)
	{
		if( wandID == 1 )
		{
			return CAVE2Manager.wand1.GetButton(button);
		}
		else if( wandID == 2 )
		{
			return CAVE2Manager.wand2.GetButton(button);
		}
		
		return false;
	}

	public static bool GetButtonDown(int wandID, CAVE2Manager.Button button)
	{
		if( wandID == 1 )
		{
			return CAVE2Manager.wand1.GetButtonDown(button);
		}
		else if( wandID == 2 )
		{
			return CAVE2Manager.wand2.GetButtonDown(button);
		}
		
		return false;
	}

	public static bool GetButtonUp(int wandID, CAVE2Manager.Button button)
	{
		if( wandID == 1 )
		{
			return CAVE2Manager.wand1.GetButtonUp(button);
		}
		else if( wandID == 2 )
		{
			return CAVE2Manager.wand2.GetButtonUp(button);
		}
		
		return false;
	}

	public static WandState.ButtonState GetButtonState(int wandID, CAVE2Manager.Button button)
	{
		if( wandID == 1 )
		{
			return CAVE2Manager.wand1.GetButtonState((int)button);
		}
		else if( wandID == 2 )
		{
			return CAVE2Manager.wand2.GetButtonState((int)button);
		}
		
		return 0;
	}

    public static Vector3 GetWandTrackingOffset(int wandID)
    {
        if (wandID == 1)
        {
            return GetCAVE2Manager().GetComponent<CAVE2Manager>().wand1TrackingOffset;
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

		wand1.UpdateState(Wand1, Wand1Mocap);
		wand2.UpdateState(Wand2, Wand2Mocap);

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
			vertical = -getReal3D.Input.GetAxis("Forward") * axisSensitivity;
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
			if( getReal3D.Input.GetButton("Reset") )
				flags += (int)EventBase.Flags.Button2;
			// R -> Wand Button 3 (Cross/A)
			if( getReal3D.Input.GetButton("ChangeWand") )
				flags += (int)EventBase.Flags.Button3;
			// Wand Button 4 (Square/X)
			if( getReal3D.Input.GetButton("WandButton") )
				flags += (int)EventBase.Flags.Button4;
			// Wand Button 8 (R1/RB)
			if( getReal3D.Input.GetButton("NavSpeed") )
				flags += (int)EventBase.Flags.Button8;
			// Wand Button 5 (L1/LB)
			if( getReal3D.Input.GetButton("WandLook") )
				flags += (int)EventBase.Flags.Button5;
			// Wand Button 6 (L3)
			if( getReal3D.Input.GetButton("L3") )
				flags += (int)EventBase.Flags.Button6;
			// Wand Button 7 (L2)
			if( getReal3D.Input.GetButton("LT") )
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

		if( !CAVE2Manager.UsingOmicronServer() || (CAVE2Manager.UsingOmicronServer() && CAVE2Manager.UsingGetReal3D()) || (keyboardEventEmulation && Input.anyKey) )
		{
			wand1.UpdateController( flags, wandAnalog, wandAnalog2, wandAnalog3 );
		}

		if( mocapEmulation )
		{
			Vector3 lookAround = new Vector3( -wand1.GetAxis(Axis.RightAnalogStickUD), wand1.GetAxis(Axis.RightAnalogStickLR), 0 );
			headEmulatedRotation += lookAround * emulatedRotationSpeed;

			// Update emulated positions/rotations
			head1.Update( headEmulatedPosition , Quaternion.Euler(headEmulatedRotation) );

			if( lockWandToHeadTransform )
				wand1.UpdateMocap( headEmulatedPosition , Quaternion.Euler(headEmulatedRotation) );
			else
				wand1.UpdateMocap( wandEmulatedPosition , Quaternion.Euler(wandEmulatedRotation) );

            if (cameraController != null)
            {
                cameraController.transform.localPosition = headEmulatedPosition;
                cameraController.transform.localEulerAngles = headEmulatedRotation;
            }
			else
			{
				Debug.LogWarning("CAVE2Manager: No CameraController found. May not display properly in CAVE2!");
			}
		}
		else
		{
			#if USING_GETREAL3D
			head1.Update( getReal3D.Input.head.position, getReal3D.Input.head.rotation );
			wand1.UpdateMocap( getReal3D.Input.wand.position, getReal3D.Input.wand.rotation );
			#endif
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
			//getReal3D.RpcManager.call ("UpdateMocapRPC", e.sourceId, unityPos, unityRot );
			#else
			if( e.sourceId == head1.sourceID )
			{
				head1.Update( unityPos, unityRot );
			}
			else if( e.sourceId == head2.sourceID )
			{
				head2.Update( unityPos, unityRot );
			}
			else if( e.sourceId == wand1.mocapID )
			{
				wand1.UpdateMocap( unityPos, unityRot );
			}
			else if( e.sourceId == wand2.mocapID )
			{
				wand2.UpdateMocap( unityPos, unityRot );
			}
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
			//getReal3D.RpcManager.call ("UpdateControllerRPC", e.sourceId, e.flags, leftAnalogStick, rightAnalogStick, analogTrigger );
			#else
			if( e.sourceId == wand1.sourceID )
			{
				wand1.UpdateController( e.flags, leftAnalogStick, rightAnalogStick, analogTrigger );
			}
			else if( e.sourceId == wand2.sourceID )
			{
				wand2.UpdateController( e.flags, leftAnalogStick, rightAnalogStick, analogTrigger );
			}
			#endif
		}
	}

	#if USING_GETREAL3D
	[getReal3D.RPC]
	void UpdateControllerRPC( uint wandID, uint flags, Vector2 leftAnalogStick, Vector2 rightAnalogStick, Vector2 analogTrigger )
	{
		if( wandID == wand1.sourceID )
		{
			wand1.UpdateController( flags, leftAnalogStick, rightAnalogStick, analogTrigger );
		}
		else if( wandID == wand2.sourceID )
		{
			wand2.UpdateController( flags, leftAnalogStick, rightAnalogStick, analogTrigger );
		}
	}

	[getReal3D.RPC]
	void UpdateMocapRPC( uint id, Vector3 unityPos, Quaternion unityRot )
	{
		if( id == head1.sourceID )
		{
			head1.Update( unityPos, unityRot );
		}
		else if( id == head2.sourceID )
		{
			head2.Update( unityPos, unityRot );
		}
		else if( id == wand1.mocapID )
		{
			wand1.UpdateMocap( unityPos, unityRot );
		}
		else if( id == wand2.mocapID )
		{
			wand2.UpdateMocap( unityPos, unityRot );
		}
	}
	#endif

	public HeadTrackerState getHead(int ID)
	{
		if( ID == 2 )
			return head2;
		else if( ID == 1 )
			return head1;
		else
		{
			Debug.LogWarning("CAVE2Manager: getHead ID: " +ID+" does not exist. Returned Head1");
			return head1;
		}
	}
	
	public WandState getWand(int ID)
	{
		if( ID == 2 )
			return wand2;
		else if( ID == 1 )
			return wand1;
		else
		{
			Debug.LogWarning("CAVE2Manager: getWand ID: " +ID+" does not exist. Returned Wand1");
			return wand1;
		}
	}

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
