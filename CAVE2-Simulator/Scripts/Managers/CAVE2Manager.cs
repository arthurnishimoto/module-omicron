using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAVE2 : MonoBehaviour
{
    public enum Axis
    {
        None, LeftAnalogStickLR, LeftAnalogStickUD, RightAnalogStickLR, RightAnalogStickUD, AnalogTriggerL, AnalogTriggerR,
        LeftAnalogStickLR_Inverted, LeftAnalogStickUD_Inverted, RightAnalogStickLR_Inverted, RightAnalogStickUD_Inverted, AnalogTriggerL_Inverted, AnalogTriggerR_Inverted
    };
    public enum Button { Button1, Button2, Button3, Button4, Button5, Button6, Button7, Button8, Button9, SpecialButton1, SpecialButton2, SpecialButton3, ButtonUp, ButtonDown, ButtonLeft, ButtonRight, None };

    public static CAVE2InputManager Input;

    // CAVE2 Tracking Management -------------------------------------------------------------------
    public static Vector3 GetHeadPosition(int ID)
    {
        return CAVE2Manager.GetHeadPosition(ID);
    }

    public static Quaternion GetHeadRotation(int ID)
    {
        return CAVE2Manager.GetHeadRotation(ID);
    }

    public static Vector3 GetWandPosition(int ID)
    {
        return CAVE2Manager.GetWandPosition(ID);
    }

    public static Quaternion GetWandRotation(int ID)
    {
        return CAVE2Manager.GetWandRotation(ID);
    }

    public static Vector3 GetMocapPosition(int ID)
    {
        return CAVE2Manager.GetMocapPosition(ID);
    }

    public static Quaternion GetMocapRotation(int ID)
    {
        return CAVE2Manager.GetMocapRotation(ID);
    }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Input Management ----------------------------------------------------------------------
    public static bool UsingOmicronServer()
    {
        return CAVE2Manager.UsingOmicronServer();
    }

    public static float GetAxis(int wandID, CAVE2.Axis axis)
    {
        return CAVE2Manager.GetAxis(wandID, axis);
    }

    public static bool GetButton(int wandID, CAVE2.Button button)
    {
        return CAVE2Manager.GetButton(wandID, button);
    }

    public static bool GetButtonDown(int wandID, CAVE2.Button button)
    {
        return CAVE2Manager.GetButtonDown(wandID, button);
    }

    public static bool GetButtonUp(int wandID, CAVE2.Button button)
    {
        return CAVE2Manager.GetButtonUp(wandID, button);
    }

    public static OmicronController.ButtonState GetButtonState(int wandID, CAVE2.Button button)
    {
        return CAVE2Manager.GetButtonState(wandID, button);
    }

    public static CAVE2.Button GetReal3DToCAVE2Button(string name)
    {
        return CAVE2Manager.GetReal3DToCAVE2Button(name);
    }

    public static CAVE2.Axis GetReal3DToCAVE2Axis(string name)
    {
        return CAVE2Manager.GetReal3DToCAVE2Axis(name);
    }

    public static string CAVE2ToGetReal3DButton(CAVE2.Button name)
    {
        return CAVE2Manager.CAVE2ToGetReal3DButton(name);
    }

    public static string CAVE2ToGetReal3DAxis(CAVE2.Axis name)
    {
        return CAVE2Manager.CAVE2ToGetReal3DAxis(name);
    }

    // ---------------------------------------------------------------------------------------------


    // CAVE2 Cluster Management --------------------------------------------------------------------
    public static CAVE2Manager GetCAVE2Manager()
    {
        return CAVE2Manager.GetCAVE2Manager();
    }

    public static bool IsMaster()
    {
        return CAVE2Manager.IsMaster();
    }

    public static bool OnCAVE2Display()
    {
        return CAVE2Manager.OnCAVE2Display();
    }

    public static bool UsingGetReal3D()
    {
        return CAVE2Manager.UsingGetReal3D();
    }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Simulator Management ------------------------------------------------------------------
    public static bool IsSimulatorMode()
    {
        return CAVE2Manager.IsSimulatorMode();
    }
    // ---------------------------------------------------------------------------------------------

    // CAVE2 Omicron Management --------------------------------------------------------------------
    public static void AddCameraController(CAVE2CameraController cam)
    {
        CAVE2Manager.AddCameraController(cam);
    }
    // ---------------------------------------------------------------------------------------------

    // CAVE2 Synchronization Management ------------------------------------------------------------
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
    // ---------------------------------------------------------------------------------------------
}

public class CAVE2Manager : MonoBehaviour {

    static CAVE2Manager CAVE2Manager_Instance;
    CAVE2InputManager inputManager;

    static string machineName;

    public int head1MocapID = 0;
    public int wand1MocapID = 1;
    public int wand1ControllerID = 1;

    public int head2MocapID = 2;
    public int wand2MocapID = 3;
    public int wand2ControllerID = 2;

    ArrayList cameraControllers;
    public CAVE2CameraController mainCameraController;

    // Simulator
    public bool simulatorMode;
    public bool mocapEmulation;
    public bool keyboardEventEmulation;
    public bool wandMousePointerEmulation;
    public bool usingKinectTrackingSimulator;
    
    public Vector3 simulatorHeadPosition = new Vector3(0.0f, 1.6f, 0.0f);
    public Vector3 simulatorHeadRotation = new Vector3(0.0f, 0.0f, 0.0f);

    public Vector3 simulatorWandPosition = new Vector3(0.16f, 1.43f, 0.4f);
    public Vector3 simulatorWandRotation = new Vector3(0.0f, 0.0f, 0.0f);

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

    public enum TrackerEmulated { CAVE, Head, Wand };
    public enum TrackerEmulationMode { Pointer, Translate, Rotate, TranslateForward, TranslateVertical, RotatePitchYaw, RotateRoll };
    string[] trackerEmuStrings = { "CAVE", "Head", "Wand1" };
    string[] trackerEmuModeStrings = { "Pointer", "Translate", "Rotate" };

    public TrackerEmulationMode defaultWandEmulationMode = TrackerEmulationMode.Pointer;
    public TrackerEmulationMode toggleWandEmulationMode = TrackerEmulationMode.TranslateVertical;
    public TrackerEmulationMode wandEmulationMode = TrackerEmulationMode.Pointer;
    public KeyCode toggleWandModeKey = KeyCode.Tab;
    public bool wandModeToggled = false;
    Vector3 mouseLastPos;
    public Vector3 mouseDeltaPos;

    void Start()
    {
        CAVE2Manager_Instance = this;
        inputManager = GetComponent<CAVE2InputManager>();
        CAVE2.Input = inputManager;

        cameraControllers = new ArrayList();

        machineName = System.Environment.MachineName;
        Debug.Log(this.GetType().Name + ">\t initialized on " + machineName);

        if( OnCAVE2Display() || OnCAVE2Master() )
        {
#if UNITY_EDITOR
#else
            simulatorMode = false;
            mocapEmulation = false;
            keyboardEventEmulation = false;
            wandMousePointerEmulation = false;
            usingKinectTrackingSimulator = false;
#endif
        }
    }

    void Update()
    {
        if(CAVE2Manager_Instance == null)
        {
            CAVE2Manager_Instance = this;
        }

        if( !UsingGetReal3D() && simulatorMode )
        {
            if (mainCameraController)
            {
                mainCameraController.GetMainCamera().transform.localPosition = simulatorHeadPosition;
                mainCameraController.GetMainCamera().transform.localEulerAngles = simulatorHeadRotation;
            }
        }
    }

    // CAVE2 Tracking Management -------------------------------------------------------------------
    public static Vector3 GetHeadPosition(int ID)
    {
        int sensorID = 0;
        Vector3 pos = Vector3.zero;

        if (ID == 1)
        {
            sensorID = GetCAVE2Manager().head1MocapID;
            
        }
        pos = CAVE2.Input.GetMocapPosition(sensorID);
        return pos;
    }

    public static Quaternion GetHeadRotation(int ID)
    {
        int sensorID = 0;
        Quaternion rot = Quaternion.identity;
        if (ID == 1)
        {
            sensorID = GetCAVE2Manager().head1MocapID;
        }

        rot = CAVE2.Input.GetMocapRotation(sensorID);
        return rot;
    }

    public static Vector3 GetWandPosition(int ID)
    {
        return CAVE2.Input.GetWandPosition(ID);
    }

    public static Quaternion GetWandRotation(int ID)
    {
        return CAVE2.Input.GetWandRotation(ID);
    }

    public static Vector3 GetMocapPosition(int ID)
    {
        return CAVE2.Input.GetMocapPosition(ID);
    }

    public static Quaternion GetMocapRotation(int ID)
    {
        return CAVE2.Input.GetMocapRotation(ID);
    }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Input Management ----------------------------------------------------------------------
    public static bool UsingOmicronServer()
    {
        OmicronManager omicronManager = GetCAVE2Manager().GetComponent<OmicronManager>();
        if (omicronManager)
            return omicronManager.connectedToServer;
        else
            return false;
    }

    public static float GetAxis(int wandID, CAVE2.Axis axis)
    {
        return CAVE2.Input.GetAxis(wandID, axis);
    }

    public static bool GetButton(int wandID, CAVE2.Button button)
    {
        return CAVE2.Input.GetButton(wandID, button);
    }

    public static bool GetButtonDown(int wandID, CAVE2.Button button)
    {
        return CAVE2.Input.GetButtonDown(wandID, button);
    }

    public static bool GetButtonUp(int wandID, CAVE2.Button button)
    {
        return CAVE2.Input.GetButtonUp(wandID, button);
    }

    public static OmicronController.ButtonState GetButtonState(int wandID, CAVE2.Button button)
    {
        return CAVE2.Input.GetButtonState(wandID, button);
    }

    /*
          <map_button index = "2" name="WandButton"/> <!-- B / Circle --> 
          <map_button index = "3" name="ChangeWand"/> <!-- X / Square --> 
          <map_button index = "6" name="Reset"/> <!-- A / Cross -->
          <map_button index = "4" name="Jump"/> <!-- Y / Triangle -->
          <map_button index = "5" name="WandLook" /> <!-- RB / R1-->
          <map_button index = "1" name="NavSpeed" /> <!-- LB / L1-->
          <map_button index = "7" name="WandDrive" /> <!-- LT / L2 --> 
          <map_button index = "8" name="RT" /> <!-- RT / R2 --> 
          <map_button index = "9" name="L3" /> <!-- L3(Left analog button)-->
          <map_button index = "10" name="R3" /> <!-- R3(Right analog button)-->
          <map_button index = "11" name="Back" /> <!-- Back / Select-->
          <map_button index = "12" name="Start" /> <!-- Start -->
          
          <map_valuator dead_zone = ".1" index="1" name="Yaw"/>
          <map_valuator dead_zone = ".1" index="2" name="Forward"/>
          <map_valuator dead_zone = ".1" index="3" name="Strafe" />
          <map_valuator dead_zone = ".1" index="4" name="Pitch" />
          <map_valuator dead_zone = ".1" index="5" name="DPadLR" />
          <map_valuator dead_zone = ".1" index="6" name="DPadUD" />

          <map_tracker index = "1" name="Head" />
          <map_tracker index = "3" name="Wand" />
*/
    public static CAVE2.Button GetReal3DToCAVE2Button(string name)
    {
        switch(name)
        {
            case "WandButton": return CAVE2.Button.Button3;
            case "ChangeWand": return CAVE2.Button.Button2;
            case "Reset": return CAVE2.Button.Button8;
            case "Jump": return CAVE2.Button.Button1;
            case "WandLook": return CAVE2.Button.Button5;
            case "NavSpeed": return CAVE2.Button.Button4;
            case "WandDrive": return CAVE2.Button.Button7;
            case "RT": return CAVE2.Button.SpecialButton3;
            case "L3": return CAVE2.Button.Button6;
            case "R3": return CAVE2.Button.Button9;
            case "Back": return CAVE2.Button.SpecialButton1;
            case "Start": return CAVE2.Button.SpecialButton2;
        }
        return CAVE2.Button.None;
    }

    public static CAVE2.Axis GetReal3DToCAVE2Axis(string name)
    {
        switch (name)
        {
            case "Yaw": return CAVE2.Axis.LeftAnalogStickLR;
            case "Forward": return CAVE2.Axis.LeftAnalogStickUD;
            case "Strafe": return CAVE2.Axis.RightAnalogStickLR;
            case "Pitch": return CAVE2.Axis.RightAnalogStickUD;
        }
        return CAVE2.Axis.None;
    }

    public static string CAVE2ToGetReal3DButton(CAVE2.Button name)
    {
        switch (name)
        {
            case CAVE2.Button.Button3: return "WandButton";
            case CAVE2.Button.Button2: return "ChangeWand";
            case CAVE2.Button.Button8: return "Reset";
            case CAVE2.Button.Button1: return "Jump";
            case CAVE2.Button.Button5: return "WandLook";
            case CAVE2.Button.Button4: return "NavSpeed";
            case CAVE2.Button.Button7: return "WandDrive";
            case CAVE2.Button.SpecialButton3: return "RT";
            case CAVE2.Button.Button6: return "L3";
            case CAVE2.Button.Button9: return "R3";
            case CAVE2.Button.SpecialButton1: return "Back";
            case CAVE2.Button.SpecialButton2: return "Start";

            case CAVE2.Button.ButtonUp: return "DPadUD";
            case CAVE2.Button.ButtonDown: return "DPadUD";
            case CAVE2.Button.ButtonLeft: return "DPadLR";
            case CAVE2.Button.ButtonRight: return "DPadLR";
        }
        return "";
    }

    public static string CAVE2ToGetReal3DAxis(CAVE2.Axis name)
    {
        switch (name)
        {
            case CAVE2.Axis.LeftAnalogStickLR: return "Yaw";
            case CAVE2.Axis.LeftAnalogStickUD: return "Forward";
            case CAVE2.Axis.RightAnalogStickLR: return "Strafe";
            case CAVE2.Axis.RightAnalogStickUD: return "Pitch";
        }
        return "";
    }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Cluster Management --------------------------------------------------------------------
    public static CAVE2Manager GetCAVE2Manager()
    {
        if(CAVE2Manager_Instance == null)
        {
            Debug.LogWarning("CAVE2Manager_Instance is NULL - SHOULD NOT HAPPEN!");
        }
        return CAVE2Manager_Instance;
    }

    public static bool IsMaster()
    {
#if USING_GETREAL3D
		return getReal3D.Cluster.isMaster;
#else
        if (machineName.Contains("LYRA") && !machineName.Equals("LYRA-WIN"))
            return false;
        else // Assumes on LYRA-WIN or development machine
            return true;
#endif
    }

    public static bool OnCAVE2Display()
    {
        machineName = System.Environment.MachineName;
        if (machineName.Contains("LYRA") && !IsMaster())
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool OnCAVE2Master()
    {
        machineName = System.Environment.MachineName;
        if (machineName.Contains("LYRA-WIN") )
        {
            return true;
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
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Simulator Management ------------------------------------------------------------------
    public static bool IsSimulatorMode()
    {
        return CAVE2Manager_Instance.simulatorMode;
    }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Omicron Management --------------------------------------------------------------------
    public static void AddCameraController(CAVE2CameraController cam)
    {
        if (CAVE2Manager_Instance.cameraControllers == null)
        {
            CAVE2Manager_Instance.cameraControllers = new ArrayList();
        }

        if(CAVE2Manager_Instance.cameraControllers.Count == 0)
            CAVE2Manager_Instance.mainCameraController = cam;

        CAVE2Manager_Instance.cameraControllers.Add(cam);
    }

    public static CAVE2CameraController GetCameraController(int cam)
    {
        if (cam >= 0 && cam < CAVE2Manager_Instance.cameraControllers.Count - 1)
            return (CAVE2CameraController)CAVE2Manager_Instance.cameraControllers[cam];
        else
            return null;
    }
    // ---------------------------------------------------------------------------------------------

}
