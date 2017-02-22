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

    // CAVE2 Tracking Management -------------------------------------------------------------------
    static MocapState nullMocapState = new MocapState(-1);
    public static MocapState GetHead(int ID)
    {
        return nullMocapState;
    }

    public static Vector3 GetWandPosition(int wandID)
    {
        return Vector3.zero;
    }

    public static Quaternion GetWandRotation(int wandID)
    {
        return Quaternion.identity;
    }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Input Management ----------------------------------------------------------------------
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

    public static OmicronControllerManager.ButtonState GetButtonState(int wandID, CAVE2.Button button)
    {
        return CAVE2Manager.GetButtonState(wandID, button);
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

    public bool simulatorMode;
    public bool usingKinectTrackingSimulator;

    void Start()
    {
        CAVE2Manager_Instance = this;
        inputManager = GetComponent<CAVE2InputManager>();

        machineName = System.Environment.MachineName;
        Debug.Log(this.GetType().Name + ">\t initialized on " + machineName);
    }

    void Update()
    {
        if(CAVE2Manager_Instance == null)
        {
            CAVE2Manager_Instance = this;
        }
    }

    // CAVE2 Tracking Management -------------------------------------------------------------------
    static MocapState nullMocapState = new MocapState(-1);
    public static MocapState GetHead(int ID)
    {
        return nullMocapState;
    }

    public static Vector3 GetWandPosition(int wandID)
    {
        return Vector3.zero;
    }

    public static Quaternion GetWandRotation(int wandID)
    {
        return Quaternion.identity;
    }
    // ---------------------------------------------------------------------------------------------


    // CAVE2 Input Management ----------------------------------------------------------------------
    public static float GetAxis(int wandID, CAVE2.Axis axis)
    {
        return GetCAVE2Manager().inputManager.GetAxis(wandID, axis);
    }

    public static bool GetButton(int wandID, CAVE2.Button button)
    {
        return GetCAVE2Manager().inputManager.GetButton(wandID, button);
    }

    public static bool GetButtonDown(int wandID, CAVE2.Button button)
    {
        return GetCAVE2Manager().inputManager.GetButtonDown(wandID, button);
    }

    public static bool GetButtonUp(int wandID, CAVE2.Button button)
    {
        return GetCAVE2Manager().inputManager.GetButtonUp(wandID, button);
    }

    public static OmicronControllerManager.ButtonState GetButtonState(int wandID, CAVE2.Button button)
    {
        return GetCAVE2Manager().inputManager.GetButtonState(wandID, button);
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
        //CAVE2 manager = GameObject.Find("CAVE2-Manager").GetComponent<CAVE2>();
        //return (manager.simulatorMode || manager.usingKinectTrackingSimulator);
        return false;
    }
    // ---------------------------------------------------------------------------------------------



}
