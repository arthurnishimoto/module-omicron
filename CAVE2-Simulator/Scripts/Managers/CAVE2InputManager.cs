using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using omicron;
using omicronConnector;

public class CAVE2InputManager : OmicronEventClient
{
    Hashtable mocapSensors = new Hashtable();
    Hashtable wandControllers = new Hashtable();

    public float axisSensitivity = 1f;
    public float axisDeadzone = 0.2f;

    public Vector3 wand1TrackingOffset = new Vector3(-0.007781088f, -0.04959464f, -0.07368752f);

    // Prevent other inputs from wand if it has opened a menu
    bool wand1MenuLock = false;
    bool wand2MenuLock = false;

    // Use this for initialization
    new void Start () {
        base.Start();
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
        int wandID = CAVE2.GetCAVE2Manager().wand1ControllerID;

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

        // Get/Create Primary Head
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

        Vector2 analog1 = Vector2.zero;
        Vector2 analog2 = Vector2.zero;
        int flags = 0;

        // Mocap
        if (CAVE2.GetCAVE2Manager().mocapEmulation)
        {
            mainHeadSensor.position = CAVE2.GetCAVE2Manager().simulatorHeadPosition;
            mainHeadSensor.orientation = Quaternion.Euler(CAVE2.GetCAVE2Manager().simulatorHeadRotation);

            wandMocapSensor.position = CAVE2.GetCAVE2Manager().simulatorWandPosition;
            wandMocapSensor.orientation = Quaternion.Euler(CAVE2.GetCAVE2Manager().simulatorWandRotation);
        }
        else if( CAVE2.GetCAVE2Manager().usingKinectTrackingSimulator )
        {
            CAVE2.GetCAVE2Manager().simulatorHeadPosition = GetHeadPosition(1);
            CAVE2.GetCAVE2Manager().simulatorWandPosition = GetWandPosition(1);

            mainHeadSensor.position = CAVE2.GetCAVE2Manager().simulatorHeadPosition;
            mainHeadSensor.orientation = Quaternion.Euler(CAVE2.GetCAVE2Manager().simulatorHeadRotation);

            wandMocapSensor.position = CAVE2.GetCAVE2Manager().simulatorWandPosition;
            wandMocapSensor.orientation = Quaternion.Euler(CAVE2.GetCAVE2Manager().simulatorWandRotation);
        }

        // Wand Buttons
        if (CAVE2.GetCAVE2Manager().keyboardEventEmulation)
        {
            float analogUD = Input.GetAxis(CAVE2.GetCAVE2Manager().wandSimulatorAnalogUD);
            float analogLR = Input.GetAxis(CAVE2.GetCAVE2Manager().wandSimulatorAnalogLR);
            //bool turn = CAVE2.GetCAVE2Manager().simulatorWandStrafeAsTurn;

            analog1 = new Vector2(analogLR, analogUD);

            if (Input.GetKey(CAVE2.GetCAVE2Manager().wandSimulatorDPadUp))
                flags += (int)EventBase.Flags.ButtonUp;
            if (Input.GetKey(CAVE2.GetCAVE2Manager().wandSimulatorDPadDown))
                flags += (int)EventBase.Flags.ButtonDown;
            if (Input.GetKey(CAVE2.GetCAVE2Manager().wandSimulatorDPadLeft))
                flags += (int)EventBase.Flags.ButtonLeft;
            if (Input.GetKey(CAVE2.GetCAVE2Manager().wandSimulatorDPadRight))
                flags += (int)EventBase.Flags.ButtonRight;

            // Wand Button 1 (Triangle/Y)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button1)))
            //    flags += (int)EventBase.Flags.Button1;
            // F -> Wand Button 2 (Circle/B)
            if (Input.GetButton(CAVE2.GetCAVE2Manager().wandSimulatorButton2))
                flags += (int)EventBase.Flags.Button2;
            // R -> Wand Button 3 (Cross/A)
            if (Input.GetButton(CAVE2.GetCAVE2Manager().wandSimulatorButton3))
                flags += (int)EventBase.Flags.Button3;
            // Wand Button 4 (Square/X)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button4)))
            //    flags += (int)EventBase.Flags.Button4;
            // Wand Button 8 (R1/RB)
            //if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton3)))
            //    flags += (int)EventBase.Flags.SpecialButton3;
            // Wand Button 5 (L1/LB)
            if (Input.GetKey(CAVE2.GetCAVE2Manager().wandSimulatorButton5))
                flags += (int)EventBase.Flags.Button5;
            // Wand Button 6 (L3)
            if (Input.GetButton(CAVE2.GetCAVE2Manager().wandSimulatorButton6))
                flags += (int)EventBase.Flags.Button6;
            // Wand Button 7 (L2)
            if (Input.GetKey(CAVE2.GetCAVE2Manager().wandSimulatorButton7))
                flags += (int)EventBase.Flags.Button7;
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
            mainHeadSensor.position = getReal3D.Input.head.position;
            mainHeadSensor.orientation = getReal3D.Input.head.rotation;

            wandMocapSensor.position = getReal3D.Input.wand.position;
            wandMocapSensor.orientation = getReal3D.Input.wand.rotation;

            analog1 = new Vector2(
                getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.LeftAnalogStickLR)),
                getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.LeftAnalogStickUD))
            );
            analog2 = new Vector2(
                getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.RightAnalogStickLR)),
                getReal3D.Input.GetAxis(CAVE2.CAVE2ToGetReal3DAxis(CAVE2.Axis.RightAnalogStickUD))
            );

            flags = 0;
            float DPadUD = getReal3D.Input.GetAxis("DPadUD");
            float DPadLR = getReal3D.Input.GetAxis("DPadLR");
            if (DPadUD > 0)
                flags += (int)EventBase.Flags.ButtonUp;
            else if (DPadUD < 0)
                flags += (int)EventBase.Flags.ButtonDown;
            if (DPadLR < 0)
                flags += (int)EventBase.Flags.ButtonLeft;
            else if (DPadLR > 0)
                flags += (int)EventBase.Flags.ButtonRight;

            // Wand Button 1 (Triangle/Y)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button1)))
                flags += (int)EventBase.Flags.Button1;
            // F -> Wand Button 2 (Circle/B)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button2)))
                flags += (int)EventBase.Flags.Button2;
            // R -> Wand Button 3 (Cross/A)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button3)))
                flags += (int)EventBase.Flags.Button3;
            // Wand Button 4 (Square/X)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button4)))
                flags += (int)EventBase.Flags.Button4;
            // Wand Button 8 (R1/RB)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton3)))
                flags += (int)EventBase.Flags.SpecialButton3;
            // Wand Button 5 (L1/LB)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button5)))
                flags += (int)EventBase.Flags.Button5;
            // Wand Button 6 (L3)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button6)))
                flags += (int)EventBase.Flags.Button6;
            // Wand Button 7 (L2)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button7)))
                flags += (int)EventBase.Flags.Button7;
            // Wand Button 8 (R2)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button8)))
                flags += (int)EventBase.Flags.Button8;
            // Wand Button 9 (R3)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.Button9)))
                flags += (int)EventBase.Flags.Button9;
            // Wand Button SP1 (Back)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton1)))
                flags += (int)EventBase.Flags.SpecialButton1;
            // Wand Button SP2 (Start)
            if (getReal3D.Input.GetButton(CAVE2.CAVE2ToGetReal3DButton(CAVE2.Button.SpecialButton2)))
                flags += (int)EventBase.Flags.SpecialButton2;
        }
        
#endif
        // Update wandManager

        if ( CAVE2.UsingGetReal3D() || CAVE2.GetCAVE2Manager().keyboardEventEmulation)
        {
            wandController.UpdateAnalog(analog1, analog2, Vector2.zero, Vector2.zero);
            wandController.rawFlags = flags;
        }
    }
}
