using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using omicron;
using omicronConnector;
using UnityEngine.VR;
public class CAVE2InputManager : OmicronEventClient
{
    Hashtable mocapSensors = new Hashtable();
    Hashtable wandControllers = new Hashtable();

    // [SerializeField]
    // float axisSensitivity = 1f;

    [SerializeField]
    float axisDeadzone = 0.2f;

    public Vector3[] wandTrackingOffset = new Vector3[]{
        new Vector3(-0.007781088f, -0.04959464f, -0.07368752f) // Wand 1
    };

    // Prevent other inputs from wand if it has opened a menu
    bool wand1MenuLock = false;
    bool wand2MenuLock = false;

    Hashtable unityInputToOmicronInput = new Hashtable();

    // public enum InputMappingMode { CAVE2Simulator, Vive, Oculus };

    // [SerializeField]
    // InputMappingMode inputMappingMode = InputMappingMode.CAVE2Simulator;

    [SerializeField]
    bool debug = false;

    public enum VRModel { None, Vive, Oculus };

    [SerializeField]
    VRModel vrModel = VRModel.None;

    public void Init()
    {
        base.Start();

        unityInputToOmicronInput[CAVE2.GetCAVE2Manager().wandSimulatorAnalogUD] = CAVE2.Axis.LeftAnalogStickUD;
        unityInputToOmicronInput[CAVE2.GetCAVE2Manager().wandSimulatorAnalogLR] = CAVE2.Axis.LeftAnalogStickLR;
        unityInputToOmicronInput[CAVE2.GetCAVE2Manager().wandSimulatorButton3] = CAVE2.Button.Button3;
        unityInputToOmicronInput[CAVE2.GetCAVE2Manager().wandSimulatorButton2] = CAVE2.Button.Button2;
        unityInputToOmicronInput[CAVE2.GetCAVE2Manager().wandSimulatorButton6] = CAVE2.Button.Button6;

        if (UnityEngine.XR.XRDevice.model == "Vive MV")
        {
            vrModel = VRModel.Vive;
        }
        else if (UnityEngine.XR.XRDevice.model.Length > 0)
        {
            Debug.Log("CAVE2InputManager: Detected VRDevice '" + UnityEngine.XR.XRDevice.model + "'.");
        }

        OmicronMocapSensor[] mocapSensors = GetComponents<OmicronMocapSensor>();
        foreach (OmicronMocapSensor ms in mocapSensors)
        {
            Destroy(ms);
            //mocapSensors[ms.sourceID] = ms;
            Debug.LogWarning("CAVE2InputManager: Found existing mocap sensor id '" + ms.sourceID + "'.");
        }
        OmicronController[] controllers = GetComponents<OmicronController>();
        foreach (OmicronController c in controllers)
        {
            Destroy(c);
            //wandControllers[c.sourceID] = c;
            Debug.LogWarning("CAVE2InputManager: Found existing controllers id '" + c.sourceID + "'.");
        }
    }
    public bool IsWandMenuLocked(int wandID)
    {
        if (wandID == 1)
            return wand1MenuLock;
        else if (wandID == 2)
            return wand2MenuLock;
        else
            return false;
    }

    public void SetWandMenuLock(int wandID, bool value)
    {
        if (wandID == 1)
            wand1MenuLock = value;
        else if (wandID == 2)
            wand2MenuLock = value;
    }

    public OmicronController.ButtonState GetButtonState(int wandID, CAVE2.Button button)
    {
        if(wandControllers != null && wandControllers.ContainsKey(wandID))
        {
            OmicronController wandController = (OmicronController)wandControllers[wandID];
            return wandController.GetButtonState(button);
        }
        return OmicronController.ButtonState.Idle;
    }

    public bool GetButtonDown(int wandID, CAVE2.Button button)
    {
        if (wandControllers != null && wandControllers.ContainsKey(wandID))
        {
            OmicronController wandController = (OmicronController)wandControllers[wandID];
            return wandController.GetButtonState(button) == OmicronController.ButtonState.Down;
        }
        return false;
    }

    public bool GetButton(int wandID, CAVE2.Button button)
    {
        if (wandControllers != null && wandControllers.ContainsKey(wandID))
        {
            OmicronController wandController = (OmicronController)wandControllers[wandID];
            return wandController.GetButtonState(button) == OmicronController.ButtonState.Held;
        }
        return false;
    }

    public bool GetButtonUp(int wandID, CAVE2.Button button)
    {
        if (wandControllers.ContainsKey(wandID))
        {
            OmicronController wandController = (OmicronController)wandControllers[wandID];
            return wandController.GetButtonState(button) == OmicronController.ButtonState.Up;
        }
        return false;
    }

    public bool GetButtonDown(int wandID, string button)
    {
        return GetButtonDown(wandID, (CAVE2.Button)unityInputToOmicronInput[button]);
    }

    public bool GetButton(int wandID, string button)
    {
        return GetButton(wandID, (CAVE2.Button)unityInputToOmicronInput[button]);
    }

    public bool GetButtonUp(int wandID, string button)
    {
        return GetButtonUp(wandID, (CAVE2.Button)unityInputToOmicronInput[button]);
    }

    public int GetButtonFlags(int wandID)
    {
        if (wandControllers.ContainsKey(wandID))
        {
            OmicronController wandController = (OmicronController)wandControllers[wandID];
            return wandController.rawFlags;
        }
        return 0;
    }

    public float GetAxis(int wandID, CAVE2.Axis axis)
    {
        if (wandControllers.ContainsKey(wandID))
        {
            OmicronController wandController = (OmicronController)wandControllers[wandID];
			float axisValue = wandController.GetAxis (axis);
			if ( Mathf.Abs(axisValue) <= axisDeadzone)
				axisValue = 0;
			return axisValue;
        }
        return 0;
    }

    public float GetAxis(string axis, int wandID = 1)
    {
        return GetAxis(wandID, (CAVE2.Axis)unityInputToOmicronInput[axis]);
    }

    public Vector3 GetMocapPosition(int ID)
    {
        if (mocapSensors.ContainsKey(ID))
        {
            OmicronMocapSensor mocap = (OmicronMocapSensor)mocapSensors[ID];
            return mocap.position;
        }
        return Vector3.zero;
    }

    public Quaternion GetMocapRotation(int ID)
    {
        if (mocapSensors.ContainsKey(ID))
        {
            OmicronMocapSensor mocap = (OmicronMocapSensor)mocapSensors[ID];
            return mocap.orientation;
        }
        return Quaternion.identity;
    }

    public float GetMocapTimeSinceUpdate(int ID)
    {
        if (mocapSensors.ContainsKey(ID))
        {
            OmicronMocapSensor mocap = (OmicronMocapSensor)mocapSensors[ID];
            return mocap.timeSinceLastUpdate;
        }
        return float.PositiveInfinity;
    }

    public Vector3 GetHeadPosition(int ID)
    {
        int headID = 1;
        switch(ID)
        {
            case (1): headID = CAVE2.GetCAVE2Manager().head1MocapID; break;
            case (2): headID = CAVE2.GetCAVE2Manager().head2MocapID; break;
        }
        return GetMocapPosition(headID);
    }

    public Quaternion GetHeadRotation(int ID)
    {
        int headID = 1;
        switch (ID)
        {
            case (1): headID = CAVE2.GetCAVE2Manager().head1MocapID; break;
            case (2): headID = CAVE2.GetCAVE2Manager().head2MocapID; break;
        }
        return GetMocapRotation(headID);
    }

    public Vector3 GetWandPosition(int ID)
    {
        int wandID = 1;
        switch (ID)
        {
            case (1): wandID = CAVE2.GetCAVE2Manager().wand1MocapID; break;
            case (2): wandID = CAVE2.GetCAVE2Manager().wand2MocapID; break;
        }
        return GetMocapPosition(wandID);
    }

    public Quaternion GetWandRotation(int ID)
    {
        int wandID = 1;
        switch (ID)
        {
            case (1): wandID = CAVE2.GetCAVE2Manager().wand1MocapID; break;
            case (2): wandID = CAVE2.GetCAVE2Manager().wand2MocapID; break;
        }
        return GetMocapRotation(wandID);
    }

    public float GetWandTimeSinceUpdate(int ID)
    {
        int wandID = 1;
        switch (ID)
        {
            case (1): wandID = CAVE2.GetCAVE2Manager().wand1MocapID; break;
            case (2): wandID = CAVE2.GetCAVE2Manager().wand2MocapID; break;
        }
        return GetMocapTimeSinceUpdate(wandID);
    }

    // Parses Omicron Input Data
    public override void OnEvent(EventData e)
    {
        //Debug.Log("CAVE2Manager_Legacy: '"+name+"' received " + e.serviceType);
        if (e.serviceType == EventBase.ServiceType.ServiceTypeMocap)
        {
            if (!mocapSensors.ContainsKey((int)e.sourceId))
            {
                OmicronMocapSensor mocapManager = gameObject.AddComponent<OmicronMocapSensor>();
                mocapManager.sourceID = (int)e.sourceId;
                if (CAVE2.GetCAVE2Manager().usingKinectTrackingSimulator)
                {
                    mocapManager.positionMod = new Vector3(1, 1, -1);
                }
                mocapSensors[(int)e.sourceId] = mocapManager;
            }
        }
        else if (e.serviceType == EventBase.ServiceType.ServiceTypeWand)
        {
            if ( !wandControllers.ContainsKey((int)e.sourceId) )
            {
                OmicronController wandController = gameObject.AddComponent<OmicronController>();
                wandController.sourceID = (int)e.sourceId;
                wandControllers[(int)e.sourceId] = wandController;
            }
        }
    }

    void FixedUpdate()
    {
        int headMocapID = CAVE2.GetCAVE2Manager().head1MocapID;
        int wandMocapID = CAVE2.GetCAVE2Manager().wand1MocapID;
        int wand2MocapID = CAVE2.GetCAVE2Manager().wand2MocapID;
        int wandID = CAVE2.GetCAVE2Manager().wand1ControllerID;
        int wand2ID = CAVE2.GetCAVE2Manager().wand2ControllerID;

        // Get/Create Primary Head
        OmicronMocapSensor mainHeadSensor;
        if (!mocapSensors.ContainsKey(headMocapID))
        {
            mainHeadSensor = gameObject.AddComponent<OmicronMocapSensor>();
            mainHeadSensor.sourceID = headMocapID;
            mocapSensors[headMocapID] = mainHeadSensor;
        }
        else
        {
            mainHeadSensor = (OmicronMocapSensor)mocapSensors[headMocapID];
        }

        // Get/Create Primary Wand
        OmicronMocapSensor wandMocapSensor;
        if (!mocapSensors.ContainsKey(wandMocapID))
        {
            wandMocapSensor = gameObject.AddComponent<OmicronMocapSensor>();
            wandMocapSensor.sourceID = wandMocapID;
            mocapSensors[wandMocapID] = wandMocapSensor;
        }
        else
        {
            wandMocapSensor = (OmicronMocapSensor)mocapSensors[wandMocapID];
        }

        // Get Primary Wand (ID 1)
        OmicronController wandController;
        if (!wandControllers.ContainsKey(wandID))
        {
            wandController = gameObject.AddComponent<OmicronController>();
            wandController.sourceID = wandID;
            wandControllers[wandID] = wandController;
        }
        else
        {
            wandController = (OmicronController)wandControllers[wandID];
        }

        // Get/Create Secondary Wand
        OmicronMocapSensor wand2MocapSensor;
        if (!mocapSensors.ContainsKey(wand2MocapID))
        {
            wand2MocapSensor = gameObject.AddComponent<OmicronMocapSensor>();
            wand2MocapSensor.sourceID = wand2MocapID;
            mocapSensors[wand2MocapID] = wand2MocapSensor;
        }
        else
        {
            wand2MocapSensor = (OmicronMocapSensor)mocapSensors[wand2MocapID];
        }

        // Get Secondary Wand (ID 1)
        OmicronController wandController2;
        if (!wandControllers.ContainsKey(wand2ID))
        {
            wandController2 = gameObject.AddComponent<OmicronController>();
            wandController2.sourceID = wand2ID;
            wandControllers[wand2ID] = wandController2;
        }
        else
        {
            wandController2 = (OmicronController)wandControllers[wand2ID];
        }
        
        Vector2 wand1_analog1 = Vector2.zero;
        Vector2 wand1_analog2 = Vector2.zero;
        Vector2 wand1_analog3 = Vector2.zero;
        int wand1_flags = 0;

        Vector2 wand2_analog1 = Vector2.zero;
        Vector2 wand2_analog2 = Vector2.zero;
        Vector2 wand2_analog3 = Vector2.zero;
        int wand2_flags = 0;
        
        // Mocap
        if (CAVE2.GetCAVE2Manager().mocapEmulation)
        {
            if (UnityEngine.XR.XRSettings.enabled)
            {

                if (UnityEngine.XR.XRDevice.model == "Vive MV")
                {
                    CAVE2.GetCAVE2Manager().simulatorHeadPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head);
                    CAVE2.GetCAVE2Manager().simulatorHeadRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head).eulerAngles;
#if UNITY_5_5_OR_NEWER
                    CAVE2.GetCAVE2Manager().simulatorWandPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftHand);
                    CAVE2.GetCAVE2Manager().simulatorWandRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftHand).eulerAngles;

                    wand2MocapSensor.position = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand);
                    wand2MocapSensor.orientation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand);
#endif
                }
                else if(UnityEngine.XR.XRDevice.model.Length > 0)
                {
                    // Hack: InputTracking isn't using some offset that the Main Camera is otherwise getting. Calculate the diff here:
                    Vector3 oculusRealHeadPosition = Camera.main.transform.localPosition;
                    Vector3 positionOffset = oculusRealHeadPosition - UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head);

                    CAVE2.GetCAVE2Manager().simulatorHeadPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head) + positionOffset;
                    CAVE2.GetCAVE2Manager().simulatorHeadRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.Head).eulerAngles;
#if UNITY_5_5_OR_NEWER
                    CAVE2.GetCAVE2Manager().simulatorWandPosition = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.LeftHand) + positionOffset;
                    CAVE2.GetCAVE2Manager().simulatorWandRotation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.LeftHand).eulerAngles;

                    wand2MocapSensor.position = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.RightHand) + positionOffset;
                    wand2MocapSensor.orientation = UnityEngine.XR.InputTracking.GetLocalRotation(UnityEngine.XR.XRNode.RightHand);
#endif
                }
            }

            mainHeadSensor.position = CAVE2.GetCAVE2Manager().simulatorHeadPosition;
            mainHeadSensor.orientation = Quaternion.Euler(CAVE2.GetCAVE2Manager().simulatorHeadRotation);
            mainHeadSensor.timeSinceLastUpdate = 0;

            wandMocapSensor.position = CAVE2.GetCAVE2Manager().simulatorWandPosition;
            wandMocapSensor.orientation = Quaternion.Euler(CAVE2.GetCAVE2Manager().simulatorWandRotation);
            wandMocapSensor.timeSinceLastUpdate = 0;
        }
        else if( CAVE2.GetCAVE2Manager().usingKinectTrackingSimulator )
        {
            CAVE2.GetCAVE2Manager().simulatorHeadPosition = GetHeadPosition(1);
            CAVE2.GetCAVE2Manager().simulatorWandPosition = GetWandPosition(1);

            mainHeadSensor.position = CAVE2.GetCAVE2Manager().simulatorHeadPosition;
            mainHeadSensor.orientation = Quaternion.Euler(CAVE2.GetCAVE2Manager().simulatorHeadRotation);
            mainHeadSensor.timeSinceLastUpdate = 0;

            wandMocapSensor.position = CAVE2.GetCAVE2Manager().simulatorWandPosition;
            wandMocapSensor.orientation = Quaternion.Euler(CAVE2.GetCAVE2Manager().simulatorWandRotation);
            wandMocapSensor.timeSinceLastUpdate = 0;
        }

        // Wand Buttons
        if (CAVE2.GetCAVE2Manager().keyboardEventEmulation)
        {
            float wand1_analogUD = Input.GetAxis(CAVE2.GetCAVE2Manager().wandSimulatorAnalogUD);
            float wand1_analogLR = Input.GetAxis(CAVE2.GetCAVE2Manager().wandSimulatorAnalogLR);
            //bool turn = CAVE2.GetCAVE2Manager().simulatorWandStrafeAsTurn;

            wand1_analog1 = new Vector2(wand1_analogLR, wand1_analogUD);

            if (Input.GetKey(CAVE2.GetCAVE2Manager().wandSimulatorDPadUp))
                wand1_flags += (int)EventBase.Flags.ButtonUp;
            if (Input.GetKey(CAVE2.GetCAVE2Manager().wandSimulatorDPadDown))
                wand1_flags += (int)EventBase.Flags.ButtonDown;
            if (Input.GetKey(CAVE2.GetCAVE2Manager().wandSimulatorDPadLeft))
                wand1_flags += (int)EventBase.Flags.ButtonLeft;
            if (Input.GetKey(CAVE2.GetCAVE2Manager().wandSimulatorDPadRight))
                wand1_flags += (int)EventBase.Flags.ButtonRight;

            // Wand Button 1 (Triangle/Y)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button1)))
            //    flags += (int)EventBase.Flags.Button1;
            // F -> Wand Button 2 (Circle/B)
            if (Input.GetButton(CAVE2.GetCAVE2Manager().wandSimulatorButton2))
                wand1_flags += (int)EventBase.Flags.Button2;
            // R -> Wand Button 3 (Cross/A)
            if (Input.GetButton(CAVE2.GetCAVE2Manager().wandSimulatorButton3))
                wand1_flags += (int)EventBase.Flags.Button3;
            // Wand Button 4 (Square/X)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button4)))
            //    flags += (int)EventBase.Flags.Button4;
            // Wand Button 8 (R1/RB)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton3)))
            //    flags += (int)EventBase.Flags.SpecialButton3;
            // Wand Button 5 (L1/LB)
            if (Input.GetKey(CAVE2.GetCAVE2Manager().wandSimulatorButton5))
                wand1_flags += (int)EventBase.Flags.Button5;
            // Wand Button 6 (L3)
            if (Input.GetButton(CAVE2.GetCAVE2Manager().wandSimulatorButton6))
                wand1_flags += (int)EventBase.Flags.Button6;
            // Wand Button 7 (L2)
            if (Input.GetKey(CAVE2.GetCAVE2Manager().wandSimulatorButton7))
                wand1_flags += (int)EventBase.Flags.Button7;
            // Wand Button 8 (R2)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button8)))
            //    flags += (int)EventBase.Flags.Button8;
            // Wand Button 9 (R3)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button9)))
            //    flags += (int)EventBase.Flags.Button9;
            // Wand Button SP1 (Back)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton1)))
            //    flags += (int)EventBase.Flags.SpecialButton1;
            // Wand Button SP2 (Start)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton2)))
            //    flags += (int)EventBase.Flags.SpecialButton2;
        }

        // getReal3D
#if USING_GETREAL3D
        if (!CAVE2.IsSimulatorMode())
        {
            wand1_analog1 = new Vector2(
                getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.LeftAnalogStickLR)),
                getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.LeftAnalogStickUD))
            );
            wand1_analog2 = new Vector2(
                getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.RightAnalogStickLR)),
                getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.RightAnalogStickUD))
            );

            wand1_flags = 0;
            // wand2_flags = 0;

            float wand1_DPadUD = getReal3D.Input.GetAxis("DPadUD");
            float wand1_DPadLR = getReal3D.Input.GetAxis("DPadLR");

            if (wand1_DPadUD > 0)
                wand1_flags += (int)EventBase.Flags.ButtonUp;
            else if (wand1_DPadUD < 0)
                wand1_flags += (int)EventBase.Flags.ButtonDown;
            if (wand1_DPadLR < 0)
                wand1_flags += (int)EventBase.Flags.ButtonLeft;
            else if (wand1_DPadLR > 0)
                wand1_flags += (int)EventBase.Flags.ButtonRight;

            // Wand Button 1 (Triangle/Y)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button1)))
                wand1_flags += (int)EventBase.Flags.Button1;
            // F -> Wand Button 2 (Circle/B)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button2)))
                wand1_flags += (int)EventBase.Flags.Button2;
            // R -> Wand Button 3 (Cross/A)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button3)))
                wand1_flags += (int)EventBase.Flags.Button3;
            // Wand Button 4 (Square/X)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button4)))
                wand1_flags += (int)EventBase.Flags.Button4;
            // Wand Button 8 (R1/RB)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton3)))
                wand1_flags += (int)EventBase.Flags.SpecialButton3;
            // Wand Button 5 (L1/LB)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button5)))
                wand1_flags += (int)EventBase.Flags.Button5;
            // Wand Button 6 (L3)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button6)))
                wand1_flags += (int)EventBase.Flags.Button6;
            // Wand Button 7 (L2)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button7)))
                wand1_flags += (int)EventBase.Flags.Button7;
            // Wand Button 8 (R2)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button8)))
                wand1_flags += (int)EventBase.Flags.Button8;
            // Wand Button 9 (R3)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button9)))
                wand1_flags += (int)EventBase.Flags.Button9;
            // Wand Button SP1 (Back)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton1)))
                wand1_flags += (int)EventBase.Flags.SpecialButton1;
            // Wand Button SP2 (Start)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton2)))
                wand1_flags += (int)EventBase.Flags.SpecialButton2;
        }
        
#endif
        if (UnityEngine.XR.XRSettings.enabled)
        {
            wand1_flags = 0;
            wand2_flags = 0;

            // VR Left Controller  ---------------------------------

            // Oculus Touch Left: Button.Three / X (Press)
            // Vive Left: Menu Button
            if (Input.GetKey(KeyCode.JoystickButton2))
            {
                if (vrModel == VRModel.Vive)
                {
                    // Ignore left menu button, mapped from right controller
                    // since accidental trackpad on left is common
                    //wand1_flags += (int)EventBase.Flags.Button2;
                }
                else
                {
                    wand1_flags += (int)EventBase.Flags.Button2;
                }
                if (debug)
                    Debug.Log("JoystickButton2");
            }

            // Oculus Touch Left: Button.Four / Y (Press)
            // Vive Left: N/A
            if (Input.GetKey(KeyCode.JoystickButton3))
            {
                wand1_flags += (int)EventBase.Flags.Button3;
                if (debug)
                    Debug.Log("JoystickButton3");
            }

            // Oculus Touch Left: Button.Start / Menu
            // Vive Left: N/A
            if (Input.GetKey(KeyCode.JoystickButton7))
            {
                if (debug)
                    Debug.Log("JoystickButton7");
            }

            // Oculus Touch Left: Analog Stick (Press)
            // Vive Left: Analog Stick (Press)
            if (Input.GetKey(KeyCode.JoystickButton8))
            {
                wand1_flags += (int)EventBase.Flags.Button6;
                if (debug)
                    Debug.Log("JoystickButton8");
            }

            // Oculus Touch Left: Button.Three / X (Touch)
            // Vive Left: N/A
            if (Input.GetKey(KeyCode.JoystickButton12))
            {
                if (debug)
                    Debug.Log("JoystickButton12");
            }

            // Oculus Touch Left: Button.Four / Y (Touch)
            // Vive Left: N/A
            if (Input.GetKey(KeyCode.JoystickButton13))
            {
                if (debug)
                    Debug.Log("JoystickButton13");
            }

            // Oculus Touch Left: IndexTrigger (Touch)
            // Vive Left: IndexTrigger (Touch)
            if (Input.GetKey(KeyCode.JoystickButton14))
            {
                if (debug)
                    Debug.Log("JoystickButton14");
            }

            // Oculus Touch Left: ThumbRest (Touch)
            // Vive Left: N/A
            if (Input.GetKey(KeyCode.JoystickButton18))
            {
                if (debug)
                    Debug.Log("JoystickButton18");
            }

            /*
             * Grip L/R are not default Unity InputManager axis, but 
             * are required for proper mapping of VR controller buttons.
             * The following can be added to the Axis. All other fields
             * are blank unless otherwise specified
             * 
             * Name: Trigger L
             * Gravity: 3
             * Dead: 0.001
             * Sensitivity: 3
             * Type: Joystick Axis
             * Axis: 9th axis (Joysticks)
             * JoyNum: Get Motion from all Joysticks
             *
             * Name: Trigger R
             * Gravity: 3
             * Dead: 0.001
             * Sensitivity: 3
             * Type: Joystick Axis
             * Axis: 10th axis (Joysticks)
             * JoyNum: Get Motion from all Joysticks
             * 
             * Name: Grip L
             * Gravity: 3
             * Dead: 0.001
             * Sensitivity: 3
             * Type: Joystick Axis
             * Axis: 11th axis (Joysticks)
             * JoyNum: Get Motion from all Joysticks
             * 
             * Name: Grip R
             * Gravity: 3
             * Dead: 0.001
             * Sensitivity: 3
             * Type: Joystick Axis
             * Axis: 12th axis (Joysticks)
             * JoyNum: Get Motion from all Joysticks
             */
            // Oculus Touch Left: IndexTrigger (Press)
            // Vive Left: IndexTrigger (Press)
            wand1_analog3.y = Input.GetAxis("Trigger L");
            if (Input.GetAxis("Trigger L") > 0.1f)
            {
                if (vrModel == VRModel.Vive)
                    wand1_flags += (int)EventBase.Flags.Button7;
                else
                    wand1_flags += (int)EventBase.Flags.Button5;
            }

            // Oculus Touch Left: HandTrigger (Press)
            // Vive Left: HandTrigger (Press)
            wand1_analog3.x = Input.GetAxis("Grip L");
            if (Input.GetAxis("Grip L") > 0.1f)
            {
                if (vrModel == VRModel.Vive)
                    wand1_flags += (int)EventBase.Flags.Button5;
                else
                    wand2_flags += (int)EventBase.Flags.Button7;
            }

            // Oculus Touch Right: HandTrigger (Press)
            // Vive Right: HandTrigger (Press)
            wand2_analog3.x = Input.GetAxis("Grip R");
            if (Input.GetAxis("Grip R") > 0.1f)
            {
                if (vrModel == VRModel.Vive)
                    wand1_flags += (int)EventBase.Flags.Button3;
                else
                    wand2_flags += (int)EventBase.Flags.Button7;
            }

            // Oculus Touch Right: IndexTrigger (Press)
            // Vive Right: IndexTrigger (Press)
            wand2_analog3.y = Input.GetAxis("Trigger R");
            if (Input.GetAxis("Trigger R") > 0.1f)
            {
                if (vrModel == VRModel.Vive)
                    wand2_flags += (int)EventBase.Flags.Button7;
                else
                    wand2_flags += (int)EventBase.Flags.Button5;
            }

            // VR Right Controller ---------------------------------
            // For CAVE2 simulator mode, we map the right controller
            // analog stick to the DPad
            bool rightAnalogDPadActivate = false;

            // Oculus Touch Right: Button.One / A (Press)
            // Vive Right: Menu Button
            if (Input.GetKey(KeyCode.JoystickButton0))
            {
                if (vrModel == VRModel.Vive)
                {
                    // Map the right menu button to main wand for Vive
                    // since accidental trackpad on left is common
                    wand1_flags += (int)EventBase.Flags.Button2;
                }
                else
                {
                    wand2_flags += (int)EventBase.Flags.Button2;
                }
                if (debug)
                    Debug.Log("JoystickButton0");
            }

            // Oculus Touch Right: Button.Two / B (Press)
            // Vive Right: N/A
            if (Input.GetKey(KeyCode.JoystickButton1))
            {
                wand2_flags += (int)EventBase.Flags.Button3;
                if (debug)
                    Debug.Log("JoystickButton1");
            }

            // Oculus Touch Right: Oculus Button Reserved

            // Oculus Touch Right: Analog Stick (Press)
            // Vive Right: Analog Stick (Press)
            if (Input.GetKey(KeyCode.JoystickButton9))
            {
                if( vrModel == VRModel.Vive)
                {
                    rightAnalogDPadActivate = true;
                }
                wand2_flags += (int)EventBase.Flags.Button6;
                if (debug)
                    Debug.Log("JoystickButton9");
            }

            // Oculus Touch Right: Button.One / A (Touch)
            // Vive Right: N/A
            if (Input.GetKey(KeyCode.JoystickButton10))
            {
                if (debug)
                    Debug.Log("JoystickButton10");
            }

            // Oculus Touch Right: Button.Two / B (Touch)
            // Vive Right: N/A
            if (Input.GetKey(KeyCode.JoystickButton11))
            {
                if (debug)
                    Debug.Log("JoystickButton11");
            }

            // Oculus Touch Right: IndexTrigger (Touch)
            // Vive Right: IndexTrigger (Touch)
            if (Input.GetKey(KeyCode.JoystickButton15))
            {
                if (debug)
                    Debug.Log("JoystickButton15");
            }

            // Oculus Touch Right: ThumbRest (Touch)
            // Vive Right: N/A
            if (Input.GetKey(KeyCode.JoystickButton19))
            {
                if (debug)
                    Debug.Log("JoystickButton19");
            }

            wand1_analog1 = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            wand2_analog1 = new Vector2(Input.GetAxis("Horizontal2"), -Input.GetAxis("Vertical2"));

            // Oculus Touch Right: Analog (Touch)
            // Vive Right: Analog (Touch)
            if (Input.GetKey(KeyCode.JoystickButton17))
            {
                if (vrModel == VRModel.Vive)
                {
                    // Right analog dpad is activated on pad press on Vive
                }
                else
                {
                    rightAnalogDPadActivate = true;
                }
            }

            // For CAVE2 simulator purposes, we're treating the right analog as the DPad
                if (rightAnalogDPadActivate &&
                (Input.GetAxis("Vertical2") != 0 ||
                Input.GetAxis("Horizontal2") != 0)
                )
            {
                /*
                 * Horizontal2/Vertical2 are not default Unity InputManager axis, but 
                 * are required for proper mapping of the second VR controller pad/analog stick.
                 * The following can be added to the Axis. All other fields
                 * are blank unless otherwise specified
                 * 
                 * Name: Horizontal2
                 * Gravity: 3
                 * Dead: 0.001
                 * Sensitivity: 3
                 * Type: Joystick Axis
                 * Axis: 4th axis (Joysticks)
                 * JoyNum: Get Motion from all Joysticks
                 * 
                 * Name: Vertical2
                 * Gravity: 3
                 * Dead: 0.001
                 * Sensitivity: 3
                 * Type: Joystick Axis
                 * Axis: 5th axis (Joysticks)
                 * JoyNum: Get Motion from all Joysticks
                 */
                float padAngle = Mathf.Rad2Deg * Mathf.Atan2(Input.GetAxis("Vertical2"), Input.GetAxis("Horizontal2"));

                if (padAngle < -45 && padAngle > -135)
                {
                    wand1_flags += (int)EventBase.Flags.ButtonUp;
                }
                else if (padAngle > -45 && padAngle < 45)
                {
                    wand1_flags += (int)EventBase.Flags.ButtonRight;
                }
                else if (padAngle > 45 && padAngle < 135)
                {
                    wand1_flags += (int)EventBase.Flags.ButtonDown;
                }
                else if (padAngle < -135 || padAngle > 135)
                {
                    wand1_flags += (int)EventBase.Flags.ButtonLeft;
                }
            }
        }

        // Only apply tracking when not in simulator mode
        if (!CAVE2.IsSimulatorMode())
        {
#if USING_GETREAL3D
            mainHeadSensor.position = getReal3D.Input.head.position;
            mainHeadSensor.orientation = getReal3D.Input.head.rotation;

            wandMocapSensor.position = getReal3D.Input.wand.position;
            wandMocapSensor.orientation = getReal3D.Input.wand.rotation;

            wand2MocapSensor.position = getReal3D.Input.GetSensor("Wand2").position;
            wand2MocapSensor.orientation = getReal3D.Input.GetSensor("Wand2").rotation;
#endif
        }

        if (!CAVE2.IsSimulatorMode() || CAVE2.GetCAVE2Manager().keyboardEventEmulation || UnityEngine.XR.XRSettings.enabled)
        {
            wandController.UpdateAnalog(wand1_analog1, wand1_analog2, wand1_analog3, Vector2.zero);
            wandController.rawFlags = wand1_flags;

            wandController2.UpdateAnalog(wand2_analog1, wand2_analog2, wand2_analog3, Vector2.zero);
            wandController2.rawFlags = wand2_flags;
        }

        /*
        // If Omicron server is enabled, let Omicron handle tracker/controller data instead of getReal3D
        // Unless keyboard emulation is enabled
        if (CAVE2.UsingOmicronServer() || CAVE2.GetCAVE2Manager().keyboardEventEmulation)
        {
            wandController.UpdateAnalog(wand1_analog1, wand1_analog2, wand1_analog3, Vector2.zero);
            wandController.rawFlags = wand1_flags;

            wandController2.UpdateAnalog(wand2_analog1, wand2_analog2, wand2_analog3, Vector2.zero);
            wandController2.rawFlags = wand2_flags;
        }
        */
    }

    public void UpdateWandController(int wandID, Vector2 analog1, Vector2 analog2, Vector2 analog3, Vector2 analog4, int buttonFlags)
    {
        OmicronController wand = (OmicronController)wandControllers[wandID];
        if(wand != null)
        {
            wand.UpdateAnalog(analog1, analog2, analog3, analog4);
            wand.rawFlags = buttonFlags;
        }
    }
}
